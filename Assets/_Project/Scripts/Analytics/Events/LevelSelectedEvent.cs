namespace ColorFactory.Analytics
{
    /// <summary>
    /// Ghi nhận người chơi chọn một level ở màn hình Level Select.
    /// </summary>
    public readonly struct LevelSelectedEvent : IAnalyticsEvent
    {
        public string EventName => "level_selected";

        public int LevelId { get; }

        /// <summary>
        /// Khởi tạo event chọn level với id level được chọn.
        /// </summary>
        public LevelSelectedEvent(int levelId)
        {
            LevelId = levelId;
        }

        public override string ToString() => $"levelId={LevelId}";
    }
}
