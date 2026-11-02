namespace ColorFactory.Levels
{
    /// <summary>
    /// Lưu level cần mở sau khi scene Gameplay được tải lại.
    /// </summary>
    public static class LevelSession
    {
        private static int requestedLevelId = -1;

        /// <summary>
        /// Đặt level sẽ được GameBootstrap load trong lần vào scene tiếp theo.
        /// </summary>
        public static void RequestLevel(int levelId)
        {
            requestedLevelId = levelId;
        }

        /// <summary>
        /// Lấy level đã yêu cầu rồi xóa request để không bị dùng lại ngoài ý muốn.
        /// </summary>
        public static int ConsumeRequestedLevelOrDefault(int fallbackLevelId)
        {
            int result = requestedLevelId > 0
                ? requestedLevelId
                : fallbackLevelId;

            requestedLevelId = -1;
            return result;
        }
    }
}