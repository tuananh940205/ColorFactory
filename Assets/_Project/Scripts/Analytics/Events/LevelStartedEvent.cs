namespace ColorFactory.Analytics
{
    /// <summary>
    /// Ghi nhận level vừa load xong và sẵn sàng cho người chơi.
    /// </summary>
    public readonly struct LevelStartedEvent : IAnalyticsEvent
    {
        public string EventName => "level_started";

        public int LevelId { get; }

        /// <summary>
        /// Khởi tạo event với id level đang chơi.
        /// </summary>
        public LevelStartedEvent(int levelId)
        {
            LevelId = levelId;
        }

        public override string ToString() => $"levelId={LevelId}";
    }
}
