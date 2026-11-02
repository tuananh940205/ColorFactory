namespace ColorFactory.Levels
{
    /// <summary>
    /// Tính số sao dựa trên số nước đi của người chơi.
    /// </summary>
    public static class LevelResultCalculator
    {
        /// <summary>
        /// Trả về từ một đến ba sao cho một level đã hoàn thành.
        /// </summary>
        public static int CalculateStars(LevelData level, int moveCount)
        {
            if (moveCount <= level.threeStarMoveTarget)
                return 3;

            if (moveCount <= level.twoStarMoveTarget)
                return 2;

            return 1;
        }
    }
}