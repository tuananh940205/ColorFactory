using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ColorFactory.UI
{
    /// <summary>
    /// Một button level trong màn hình chọn level.
    /// </summary>
    public sealed class LevelSelectButton : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private TMP_Text levelLabel;
        [SerializeField] private TMP_Text lockLabel;
        [SerializeField] private TMP_Text starLabel;
        [SerializeField] private TMP_Text bestMovesLabel;
        
        /// <summary>
        /// Gán trạng thái level, sao tốt nhất và callback khi người chơi bấm.
        /// </summary>
        public void Setup(
            int levelId,
            bool isUnlocked,
            int bestStars,
            int bestMoves,
            Action<int> onSelected)
        {
            levelLabel.text = $"LEVEL {levelId:00}";

            lockLabel.gameObject.SetActive(!isUnlocked);
            starLabel.gameObject.SetActive(isUnlocked);
            bestMovesLabel.gameObject.SetActive(isUnlocked);

            if (isUnlocked)
            {
                starLabel.text = BuildStarText(bestStars);

                bestMovesLabel.text = bestMoves > 0
                    ? $"BEST: {bestMoves} MOVES"
                    : "NOT COMPLETED";
            }

            button.interactable = isUnlocked;
            button.onClick.RemoveAllListeners();

            if (isUnlocked)
                button.onClick.AddListener(() => onSelected?.Invoke(levelId));
        }

        /// <summary>
        /// Hiển thị NEW nếu chưa hoàn thành hoặc tối đa ba sao khi đã hoàn thành.
        /// </summary>
        private static string BuildStarText(int stars)
        {
            return stars switch
            {
                3 => "★ ★ ★",
                2 => "★ ★ ☆",
                1 => "★ ☆ ☆",
                _ => "NEW"
            };
        }
    }
}