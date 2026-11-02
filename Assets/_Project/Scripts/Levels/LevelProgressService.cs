using UnityEngine;

namespace ColorFactory.Levels
{
    /// <summary>
    /// Lưu level cao nhất người chơi đã mở khóa bằng PlayerPrefs.
    /// </summary>
    public static class LevelProgressService
    {
        private const string HighestUnlockedLevelKey =
            "ColorFactory.HighestUnlockedLevel";

        /// <summary>
        /// Trả level cao nhất đã mở; game mới luôn bắt đầu từ level 1.
        /// </summary>
        public static int GetHighestUnlockedLevel()
        {
            return Mathf.Max(
                1,
                PlayerPrefs.GetInt(HighestUnlockedLevelKey, 1));
        }

        /// <summary>
        /// Kiểm tra một level đã được mở khóa hay chưa.
        /// </summary>
        public static bool IsUnlocked(int levelId)
        {
            return levelId <= GetHighestUnlockedLevel();
        }

        /// <summary>
        /// Khi hoàn thành level N, mở khóa tối thiểu level N + 1.
        /// </summary>
        public static void MarkLevelCompleted(int completedLevelId)
        {
            int currentHighest = GetHighestUnlockedLevel();
            int newHighest = Mathf.Max(
                currentHighest,
                completedLevelId + 1);

            PlayerPrefs.SetInt(HighestUnlockedLevelKey, newHighest);
            PlayerPrefs.Save();
        }
    }
}