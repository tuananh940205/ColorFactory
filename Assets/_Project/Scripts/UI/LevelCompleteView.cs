using ColorFactory.Audio;
using ColorFactory.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ColorFactory.UI
{
    /// <summary>
    /// Hiển thị panel khi người chơi hoàn thành level.
    /// </summary>
    public sealed class LevelCompleteView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private GameBootstrap gameBootstrap;
        [SerializeField] private Button nextLevelButton;
        [SerializeField] private TMP_Text movesSummaryText;
        [SerializeField] private TMP_Text starsText;
        
        /// <summary>
        /// Ẩn panel khi scene vừa bắt đầu.
        /// </summary>
        private void Awake()
        {
            Hide();
        }

        /// <summary>
        /// Hiển thị kết quả level gồm số nước đi và số sao đạt được.
        /// </summary>
        public void Show(int moveCount, int stars, int reward)
        {
            movesSummaryText.text =
                $"COMPLETED IN {moveCount} MOVES\nREWARD: +{reward} COINS";
            starsText.text = BuildStarText(stars);

            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;

            nextLevelButton.interactable =
                gameBootstrap != null &&
                gameBootstrap.CanGoToNextLevel();
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
            Services.SceneFader?.TryReloadActiveScene(
                () => Services.Audio?.Play(SfxType.Button));
        }
        
        /// <summary>
        /// Chuyển sang level kế tiếp đã được mở khóa.
        /// </summary>
        public void GoToNextLevel()
        {
            gameBootstrap.GoToNextLevel();
        }
        
        /// <summary>
        /// Tạo chuỗi ba sao, sao đạt được màu vàng và sao còn lại màu trống.
        /// </summary>
        private static string BuildStarText(int stars)
        {
            stars = Mathf.Clamp(stars, 1, 3);

            return stars switch
            {
                3 => "★ ★ ★",
                2 => "★ ★ ☆",
                _ => "★ ☆ ☆"
            };
        }
    }
}
