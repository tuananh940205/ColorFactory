namespace ColorFactory.Analytics
{
    /// <summary>
    /// Ghi nhận một nước đi hợp lệ, kèm tổng số nước đi sau khi thực hiện.
    /// </summary>
    public readonly struct MoveMadeEvent : IAnalyticsEvent
    {
        public string EventName => "move_made";

        public int LevelId { get; }
        public int MoveCount { get; }
        public int FromTrayIndex { get; }
        public int ToTrayIndex { get; }

        /// <summary>
        /// Khởi tạo event nước đi với khay nguồn và khay đích.
        /// </summary>
        public MoveMadeEvent(int levelId, int moveCount, int fromTrayIndex, int toTrayIndex)
        {
            LevelId = levelId;
            MoveCount = moveCount;
            FromTrayIndex = fromTrayIndex;
            ToTrayIndex = toTrayIndex;
        }

        public override string ToString() =>
            $"levelId={LevelId} moves={MoveCount} from={FromTrayIndex} to={ToTrayIndex}";
    }
}
