using System;
using System.Collections;
using System.Collections.Generic;
using ColorFactory.Core;
using UnityEngine;

namespace ColorFactory.Gameplay
{
    /// <summary>
    /// Chuyển BoardModel thành GameObject được lấy từ pool.
    /// </summary>
    public sealed class BoardView : MonoBehaviour
    {
        [SerializeField] private GameBootstrap gameBootstrap;
        [SerializeField] private Transform boardRoot;
        [Header("Responsive Tray Layout")]
        [SerializeField, Min(1)] private int maximumColumns = 3;
        [SerializeField] private float trayHorizontalSpacing = 1.9f;
        [SerializeField] private float trayVerticalSpacing = 3.1f;
        [SerializeField] private float boardCenterLocalY = -0.5f;

        private readonly Dictionary<int, TrayView> trayViews = new();
        private readonly List<GameObject> trayInstances = new();
        private readonly List<GameObject> toyInstances = new();
        // Lưu toy theo từng khay để biết toy nào cần animate khi di chuyển.
        private readonly Dictionary<int, List<GameObject>> toyStacks = new();

        private PoolService poolService;
        private BoardFeedbackView feedbackView;
        private Coroutine hintRoutine;

        public bool IsBuilt { get; private set; }

        /// <summary>
        /// BoardController nghe sự kiện này để nhận click từ TrayView.
        /// </summary>
        public event Action<int> TrayClicked;

        /// <summary>
        /// Báo rằng các tray đã được tạo xong.
        /// </summary>
        public event Action<BoardView> Built;

        /// <summary>
        /// Chờ GameBootstrap load xong rồi dựng board.
        /// </summary>
        private void Start()
        {
            if (gameBootstrap == null)
            {
                Debug.LogError("BoardView requires GameBootstrap.", this);
                return;
            }

            if (gameBootstrap.IsReady)
                Build(gameBootstrap);
            else
                gameBootstrap.Ready += Build;
        }

        /// <summary>
        /// Hủy event và trả object về pool nếu scene vẫn đang hoạt động.
        /// </summary>
        private void OnDestroy()
        {
            if (gameBootstrap != null)
                gameBootstrap.Ready -= Build;

            ClearAll();
        }

        /// <summary>
        /// Tạo tray một lần, sau đó render toy từ BoardModel.
        /// </summary>
        private void Build(GameBootstrap bootstrap)
        {
            if (IsBuilt)
                return;

            boardRoot ??= transform;
            feedbackView ??= GetComponent<BoardFeedbackView>();
            poolService = FindFirstObjectByType<PoolService>();

            if (poolService == null || bootstrap.Board == null)
            {
                Debug.LogError("BoardView cannot initialize.", this);
                return;
            }

            for (int index = 0; index < bootstrap.Board.TrayCount; index++)
            {
                GameObject trayObject =
                    poolService.Get(bootstrap.CurrentLevel.assets.trayPrefabKey);

                trayObject.transform.SetParent(boardRoot, false);
                // Tự xếp khay thành nhiều hàng khi level có nhiều hơn ba khay.
                trayObject.transform.localPosition = GetTrayLocalPosition(
                    index,
                    bootstrap.Board.TrayCount);

                TrayView trayView = trayObject.GetComponent<TrayView>();

                if (trayView == null)
                {
                    Debug.LogError("TrayPrefab needs TrayView.", trayObject);
                    poolService.Return(trayObject);
                    continue;
                }

                trayView.Setup(index);
                trayView.Clicked += HandleTrayClicked;

                trayInstances.Add(trayObject);
                trayViews.Add(index, trayView);
                // Mỗi TrayView có một stack GameObject tương ứng với BoardModel.
                toyStacks.Add(index, new List<GameObject>());
            }

            IsBuilt = true;
            Render(bootstrap.Board);
            Built?.Invoke(this);
        }

        /// <summary>
        /// Chỉ trả toy cũ về pool rồi tạo toy mới theo BoardModel hiện tại.
        /// Tray được giữ nguyên để tránh tạo lại không cần thiết.
        /// </summary>
        public void Render(BoardModel board)
        {
            if (!IsBuilt || board == null)
                return;

            ClearToys();

            foreach ((int trayIndex, TrayView trayView) in trayViews)
            {
                IReadOnlyList<ToyColor> colors = board.GetTray(trayIndex);

                for (int toyIndex = 0; toyIndex < colors.Count; toyIndex++)
                {
                    GameObject toyObject = poolService.Get(
                        gameBootstrap.CurrentLevel.assets.toyPrefabKey);

                    toyObject.transform.SetParent(trayView.ToyContainer, false);
                    toyObject.transform.localPosition =
                        trayView.GetToyLocalPosition(toyIndex);

                    ToyView toyView = toyObject.GetComponent<ToyView>();

                    if (toyView == null)
                    {
                        Debug.LogError("ToyPrefab needs ToyView.", toyObject);
                        poolService.Return(toyObject);
                        continue;
                    }

                    toyView.SetColor(colors[toyIndex]);
                    toyInstances.Add(toyObject);
                    // Lưu thứ tự toy từ đáy lên đỉnh, giống thứ tự trong BoardModel.
                    toyStacks[trayIndex].Add(toyObject);
                }
            }
        }

        /// <summary>
        /// Cho BoardController tìm TrayView để bật/tắt highlight.
        /// </summary>
        public bool TryGetTrayView(int trayIndex, out TrayView trayView)
        {
            return trayViews.TryGetValue(trayIndex, out trayView);
        }

        /// <summary>
        /// Chuyển click từ prefab thành event cấp board.
        /// </summary>
        private void HandleTrayClicked(int trayIndex)
        {
            TrayClicked?.Invoke(trayIndex);
        }

        /// <summary>
        /// Trả các toy đang hiển thị về pool trước mỗi lần render lại.
        /// </summary>
        private void ClearToys()
        {
            if (poolService == null || poolService.IsShuttingDown)
                return;

            for (int index = toyInstances.Count - 1; index >= 0; index--)
                poolService.Return(toyInstances[index]);

            toyInstances.Clear();
            // Xóa mapping cũ trước khi render lại toàn bộ toy.
            foreach (List<GameObject> stack in toyStacks.Values)
                stack.Clear();
        }

        /// <summary>
        /// Trả toàn bộ object về pool khi view bị hủy.
        /// </summary>
        private void ClearAll()
        {
            if (poolService == null || poolService.IsShuttingDown)
                return;

            ClearToys();

            foreach (GameObject trayObject in trayInstances)
            {
                if (trayObject == null)
                    continue;

                TrayView trayView = trayObject.GetComponent<TrayView>();

                if (trayView != null)
                    trayView.Clicked -= HandleTrayClicked;

                poolService.Return(trayObject);
            }

            trayInstances.Clear();
            trayViews.Clear();
            toyStacks.Clear();
            IsBuilt = false;
        }
        
        /// <summary>
        /// Hiển thị tạm thời hai khay mà HintSolver đề xuất.
        /// Khay nguồn và đích cùng sáng màu xanh.
        /// </summary>
        public void ShowHint(int from, int to)
        {
            if (!IsBuilt)
                return;

            if (hintRoutine != null)
                StopCoroutine(hintRoutine);

            hintRoutine = StartCoroutine(ShowHintRoutine(from, to));
        }

        /// <summary>
        /// Bật highlight, chờ ngắn rồi trả khay về màu bình thường.
        /// </summary>
        private IEnumerator ShowHintRoutine(int from, int to)
        {
            if (TryGetTrayView(from, out TrayView source))
                source.SetHint(true);

            if (TryGetTrayView(to, out TrayView target))
                target.SetHint(true);

            yield return new WaitForSeconds(0.9f);

            if (source != null)
                source.SetHint(false);

            if (target != null)
                target.SetHint(false);

            hintRoutine = null;
        }
        
        /// <summary>
        /// Tính vị trí local của một khay theo grid responsive.
        /// Mỗi hàng có tối đa maximumColumns khay và luôn được căn giữa.
        /// </summary>
        private Vector3 GetTrayLocalPosition(int trayIndex, int totalTrayCount)
        {
            int columnCount = Mathf.Min(maximumColumns, totalTrayCount);
            int rowCount = Mathf.CeilToInt(
                totalTrayCount / (float)columnCount);

            int rowIndex = trayIndex / columnCount;
            int columnIndex = trayIndex % columnCount;

            // Hàng cuối có thể ít khay hơn, ví dụ Level 02 là hàng 3 + 2.
            int firstIndexInRow = rowIndex * columnCount;
            int itemsInCurrentRow = Mathf.Min(
                columnCount,
                totalTrayCount - firstIndexInRow);

            float x = (columnIndex - (itemsInCurrentRow - 1) * 0.5f) *
                      trayHorizontalSpacing;

            float y = boardCenterLocalY +
                      ((rowCount - 1) * 0.5f - rowIndex) *
                      trayVerticalSpacing;

            return new Vector3(x, y, 0f);
        }
        
        /// <summary>
        /// Animate toy trên cùng từ khay nguồn sang khay đích.
        /// BoardModel phải được cập nhật trước khi gọi hàm này.
        /// </summary>
        public void AnimateMove(
            int fromTrayIndex,
            int toTrayIndex,
            System.Action onCompleted)
        {
            if (!toyStacks.TryGetValue(fromTrayIndex, out List<GameObject> sourceStack) ||
                !toyStacks.TryGetValue(toTrayIndex, out List<GameObject> targetStack) ||
                sourceStack.Count == 0 ||
                !TryGetTrayView(toTrayIndex, out TrayView targetTray))
            {
                onCompleted?.Invoke();
                return;
            }

            // Toy cuối stack chính là toy đang nằm trên cùng của khay nguồn.
            GameObject movingToy = sourceStack[^1];
            sourceStack.RemoveAt(sourceStack.Count - 1);

            // Thêm toy vào stack đích trước để tính được slot local cuối cùng.
            targetStack.Add(movingToy);

            Vector3 destinationLocalPosition =
                targetTray.GetToyLocalPosition(targetStack.Count - 1);

            Vector3 destinationWorldPosition =
                targetTray.ToyContainer.TransformPoint(destinationLocalPosition);

            StartCoroutine(AnimateMoveRoutine(
                movingToy,
                targetTray,
                destinationLocalPosition,
                destinationWorldPosition,
                onCompleted));
        }

        /// <summary>
        /// Phát feedback hoàn thành tại tâm board sau khi toy cuối cùng đã đáp.
        /// </summary>
        public void PlayCompletionFeedback()
        {
            if (feedbackView != null)
                feedbackView.PlayCompletion(boardRoot.position);
        }

        /// <summary>
        /// Di chuyển toy trong world space, rồi parent lại vào khay đích khi kết thúc.
        /// </summary>
        private System.Collections.IEnumerator AnimateMoveRoutine(
            GameObject movingToy,
            TrayView targetTray,
            Vector3 destinationLocalPosition,
            Vector3 destinationWorldPosition,
            System.Action onCompleted)
        {
            const float duration = 0.22f;

            Vector3 startWorldPosition = movingToy.transform.position;

            // Parent tạm vào BoardRoot để tọa độ world không bị ảnh hưởng bởi khay nguồn.
            movingToy.transform.SetParent(boardRoot, true);

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;

                float progress = Mathf.Clamp01(elapsed / duration);

                // Ease-out làm toy chậm dần khi đến khay đích.
                float easedProgress = 1f - Mathf.Pow(1f - progress, 3f);

                movingToy.transform.position = Vector3.Lerp(
                    startWorldPosition,
                    destinationWorldPosition,
                    easedProgress);

                yield return null;
            }

            // Kết thúc animation: toy trở thành child thật của khay đích.
            movingToy.transform.SetParent(targetTray.ToyContainer, false);
            movingToy.transform.localPosition = destinationLocalPosition;

            ToyView toyView = movingToy.GetComponent<ToyView>();
            Vector3 toyScale = movingToy.transform.localScale;
            Vector3 trayScale = targetTray.transform.localScale;
            const float pulseDuration = 0.12f;

            elapsed = 0f;

            while (elapsed < pulseDuration)
            {
                elapsed += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsed / pulseDuration);
                float pulse = 1f + Mathf.Sin(progress * Mathf.PI) * 0.12f;

                movingToy.transform.localScale = toyScale * pulse;
                targetTray.transform.localScale = trayScale * pulse;
                yield return null;
            }

            movingToy.transform.localScale = toyScale;
            targetTray.transform.localScale = trayScale;

            if (feedbackView != null)
            {
                Color feedbackColor = toyView != null
                    ? toyView.CurrentColor
                    : Color.white;
                feedbackView.PlayLanding(destinationWorldPosition, feedbackColor);
            }

            onCompleted?.Invoke();
        }
    }
}
