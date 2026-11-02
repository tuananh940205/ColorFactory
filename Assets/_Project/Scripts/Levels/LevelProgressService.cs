using UnityEngine;

namespace ColorFactory.Levels
{
    /// <summary>
    /// Lưu tiến độ mở khóa, số sao tốt nhất và số nước đi tốt nhất.
    /// </summary>
    public static class LevelProgressService
    {
        private const string HighestUnlockedLevelKey =
            "ColorFactory.HighestUnlockedLevel";

        /// <summary>
        /// Trả level cao nhất đã mở khóa; game mới luôn bắt đầu ở level 1.
        /// </summary>
        public static int GetHighestUnlockedLevel()
        {
            return Mathf.Max(
                1,
                PlayerPrefs.GetInt(HighestUnlockedLevelKey, 1));
        }

        /// <summary>
        /// Kiểm tra level đã mở khóa hay chưa.
        /// </summary>
        public static bool IsUnlocked(int levelId)
        {
            return levelId <= GetHighestUnlockedLevel();
        }

        /// <summary>
        /// Lưu kết quả hoàn thành và mở khóa level kế tiếp.
        /// </summary>
        public static void RecordCompletion(
            int levelId,
            int moveCount,
            int stars)
        {
            stars = Mathf.Clamp(stars, 1, 3);

            int previousBestStars = GetBestStars(levelId);

            if (stars > previousBestStars)
            {
                PlayerPrefs.SetInt(
                    GetBestStarsKey(levelId),
                    stars);
            }

            int previousBestMoves = GetBestMoves(levelId);

            if (previousBestMoves == 0 || moveCount < previousBestMoves)
            {
                PlayerPrefs.SetInt(
                    GetBestMovesKey(levelId),
                    moveCount);
            }

            int highestUnlocked = GetHighestUnlockedLevel();

            PlayerPrefs.SetInt(
                HighestUnlockedLevelKey,
                Mathf.Max(highestUnlocked, levelId + 1));

            PlayerPrefs.Save();
        }

        /// <summary>
        /// Trả số sao cao nhất đã đạt; 0 nghĩa là chưa hoàn thành level.
        /// </summary>
        public static int GetBestStars(int levelId)
        {
            return Mathf.Clamp(
                PlayerPrefs.GetInt(GetBestStarsKey(levelId), 0),
                0,
                3);
        }

        /// <summary>
        /// Trả số nước đi tốt nhất; 0 nghĩa là chưa hoàn thành level.
        /// </summary>
        public static int GetBestMoves(int levelId)
        {
            return Mathf.Max(
                0,
                PlayerPrefs.GetInt(GetBestMovesKey(levelId), 0));
        }

        /// <summary>
        /// Tạo PlayerPrefs key riêng cho số sao của từng level.
        /// </summary>
        private static string GetBestStarsKey(int levelId)
        {
            return $"ColorFactory.Level.{levelId}.BestStars";
        }

        /// <summary>
        /// Tạo PlayerPrefs key riêng cho số nước đi của từng level.
        /// </summary>
        private static string GetBestMovesKey(int levelId)
        {
            return $"ColorFactory.Level.{levelId}.BestMoves";
        }
    }
}