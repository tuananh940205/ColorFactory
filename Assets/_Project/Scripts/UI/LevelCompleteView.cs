using UnityEngine;
using UnityEngine.SceneManagement;

namespace ColorFactory.UI
{
    /// <summary>
    /// Hiển thị panel khi người chơi hoàn thành level.
    /// </summary>
    public sealed class LevelCompleteView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;

        /// <summary>
        /// Ẩn panel khi scene vừa bắt đầu.
        /// </summary>
        private void Awake()
        {
            Hide();
        }

        /// <summary>
        /// Hiển thị panel và chặn thao tác với board phía sau.
        /// </summary>
        public void Show()
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        /// <summary>
        /// Ẩn panel, dùng khi khởi tạo hoặc cần đóng UI.
        /// </summary>
        public void Hide()
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        /// <summary>
        /// Tải lại scene hiện tại để chơi lại level.
        /// </summary>
        public void RestartLevel()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}