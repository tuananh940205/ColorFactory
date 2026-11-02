namespace ColorFactory.Analytics
{
    /// <summary>
    /// Ghi nhận booster bị từ chối vì thiếu coin hoặc không có ví.
    /// </summary>
    public readonly struct BoosterFailedEvent : IAnalyticsEvent
    {
        public string EventName => "booster_failed";

        public int LevelId { get; }
        public BoosterType Booster { get; }
        public BoosterFailureReason Reason { get; }

        /// <summary>
        /// Khởi tạo event booster thất bại với booster và lý do cụ thể.
        /// </summary>
        public BoosterFailedEvent(
            int levelId,
            BoosterType booster,
            BoosterFailureReason reason)
        {
            LevelId = levelId;
            Booster = booster;
            Reason = reason;
        }

        public override string ToString() =>
            $"levelId={LevelId} booster={Booster} reason={Reason}";
    }
}
