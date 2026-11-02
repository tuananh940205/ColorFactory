namespace ColorFactory.Analytics
{
    /// <summary>
    /// Ghi nhận một Hint đã hiển thị nước đi và đã bị trừ coin.
    /// </summary>
    public readonly struct HintUsedEvent : IAnalyticsEvent
    {
        public string EventName => "hint_used";

        public int LevelId { get; }
        public int MoveCount { get; }

        /// <summary>
        /// Khởi tạo event Hint với số nước đi hiện tại.
        /// </summary>
        public HintUsedEvent(int levelId, int moveCount)
        {
            LevelId = levelId;
            MoveCount = moveCount;
        }

        public override string ToString() => $"levelId={LevelId} moves={MoveCount}";
    }
}
