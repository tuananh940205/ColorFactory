using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ColorFactory.Core
{
    /// <summary>
    /// Phủ màn hình khi chuyển scene để không lộ frame UI cũ hoặc frame chưa sẵn sàng.
    /// </summary>
    [DefaultExecutionOrder(-900)]
    public sealed class SceneFader : MonoBehaviour
    {
        [SerializeField, Min(0.01f)] private float fadeDuration = 0.25f;
        [SerializeField] private Color fadeColor = new(0.025f, 0.07f, 0.14f, 1f);
        [SerializeField, Min(1)] private int sortingOrder = 1000;

        private CanvasGroup canvasGroup;
        private Coroutine transitionRoutine;

        public bool IsTransitioning => transitionRoutine != null;

        /// <summary>
        /// Đăng ký service một lần và giữ overlay sống khi scene hiện tại bị thay thế.
        /// </summary>
        private void Awake()
        {
            if (Services.SceneFader != null && Services.SceneFader != this)
            {
                Destroy(gameObject);
                return;
            }

            Services.Register(this);
            DontDestroyOnLoad(gameObject);
            CreateOverlay();
            SetOverlayAlpha(0f);
        }

        /// <summary>
        /// Xóa đăng ký khi object service gốc bị hủy lúc thoát ứng dụng.
        /// </summary>
        private void OnDestroy()
        {
            Services.Unregister(this);
        }

        /// <summary>
        /// Bắt đầu chuyển scene nếu chưa có transition khác đang khóa thao tác.
        /// </summary>
        public bool TryLoadScene(string sceneName, Action onAccepted = null)
        {
            if (IsTransitioning || string.IsNullOrWhiteSpace(sceneName))
                return false;

            onAccepted?.Invoke();
            transitionRoutine = StartCoroutine(LoadSceneRoutine(sceneName));
            return true;
        }

        /// <summary>
        /// Bắt đầu tải lại scene hiện tại dưới lớp fade.
        /// </summary>
        public bool TryReloadActiveScene(Action onAccepted = null)
        {
            return TryLoadScene(SceneManager.GetActiveScene().name, onAccepted);
        }

        /// <summary>
        /// Tạo Canvas overlay bằng code để cùng một bootstrap hoạt động khi mở trực tiếp từng scene.
        /// </summary>
        private void CreateOverlay()
        {
            var overlayObject = new GameObject(
                "SceneFadeOverlay",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster),
                typeof(CanvasGroup),
                typeof(Image));

            overlayObject.transform.SetParent(transform, false);

            RectTransform overlayTransform = overlayObject.GetComponent<RectTransform>();
            overlayTransform.anchorMin = Vector2.zero;
            overlayTransform.anchorMax = Vector2.one;
            overlayTransform.offsetMin = Vector2.zero;
            overlayTransform.offsetMax = Vector2.zero;

            Canvas canvas = overlayObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true;
            canvas.sortingOrder = sortingOrder;

            CanvasScaler scaler = overlayObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;

            Image image = overlayObject.GetComponent<Image>();
            image.color = fadeColor;
            image.raycastTarget = true;

            canvasGroup = overlayObject.GetComponent<CanvasGroup>();
        }

        /// <summary>
        /// Fade ra, tải scene bất đồng bộ khi màn hình đã che kín, rồi fade vào.
        /// </summary>
        private IEnumerator LoadSceneRoutine(string sceneName)
        {
            yield return FadeTo(1f);

            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

            if (operation == null)
            {
                yield return FadeTo(0f);
                transitionRoutine = null;
                yield break;
            }

            operation.allowSceneActivation = false;

            while (operation.progress < 0.9f)
                yield return null;

            operation.allowSceneActivation = true;

            while (!operation.isDone)
                yield return null;

            yield return null;
            yield return FadeTo(0f);
            transitionRoutine = null;
        }

        /// <summary>
        /// Nội suy alpha theo thời gian thực để fade vẫn mượt khi Time.timeScale bằng không.
        /// </summary>
        private IEnumerator FadeTo(float targetAlpha)
        {
            float startAlpha = canvasGroup.alpha;
            float elapsed = 0f;
            canvasGroup.blocksRaycasts = true;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                SetOverlayAlpha(Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration));
                yield return null;
            }

            SetOverlayAlpha(targetAlpha);
            canvasGroup.blocksRaycasts = targetAlpha > 0f;
        }

        /// <summary>
        /// Cập nhật alpha của overlay an toàn sau khi Canvas đã được tạo.
        /// </summary>
        private void SetOverlayAlpha(float alpha)
        {
            canvasGroup.alpha = Mathf.Clamp01(alpha);
            // Overlay trong suốt phải trả lại thao tác cho UI của scene hiện tại.
            canvasGroup.blocksRaycasts = canvasGroup.alpha > 0f;
        }
    }
}
