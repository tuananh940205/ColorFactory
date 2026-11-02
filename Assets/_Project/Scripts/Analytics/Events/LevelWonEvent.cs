namespace ColorFactory.Analytics
{
    /// <summary>
    /// Ghi nhận level hoàn thành với kết quả cuối cùng và phần thưởng coin.
    /// </summary>
    public readonly struct LevelWonEvent : IAnalyticsEvent
    {
        public string EventName => "level_won";

        public int LevelId { get; }
        public int MoveCount { get; }
        public int Stars { get; }
        public int CoinReward { get; }

        /// <summary>
        /// Khởi tạo event thắng level với số nước, sao và coin thưởng.
        /// </summary>
        public LevelWonEvent(int levelId, int moveCount, int stars, int coinReward)
        {
            LevelId = levelId;
            MoveCount = moveCount;
            Stars = stars;
            CoinReward = coinReward;
        }

        public override string ToString() =>
            $"levelId={LevelId} moves={MoveCount} stars={Stars} reward={CoinReward}";
    }
}
