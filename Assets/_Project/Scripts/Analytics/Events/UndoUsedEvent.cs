namespace ColorFactory.Analytics
{
    /// <summary>
    /// Ghi nhận một Undo đã thực hiện thành công và đã bị trừ coin.
    /// </summary>
    public readonly struct UndoUsedEvent : IAnalyticsEvent
    {
        public string EventName => "undo_used";

        public int LevelId { get; }
        public int MoveCount { get; }

        /// <summary>
        /// Khởi tạo event Undo với số nước đi còn lại sau khi hoàn tác.
        /// </summary>
        public UndoUsedEvent(int levelId, int moveCount)
        {
            LevelId = levelId;
            MoveCount = moveCount;
        }

        public override string ToString() => $"levelId={LevelId} moves={MoveCount}";
    }
}
