namespace ColorFactory.Analytics
{
    /// <summary>
    /// Ghi nhận người chơi yêu cầu chơi lại level hiện tại.
    /// </summary>
    public readonly struct RestartRequestedEvent : IAnalyticsEvent
    {
        public string EventName => "restart_requested";

        public int LevelId { get; }

        /// <summary>
        /// Khởi tạo event restart với id level đang chơi.
        /// </summary>
        public RestartRequestedEvent(int levelId)
        {
            LevelId = levelId;
        }

        public override string ToString() => $"levelId={LevelId}";
    }
}
