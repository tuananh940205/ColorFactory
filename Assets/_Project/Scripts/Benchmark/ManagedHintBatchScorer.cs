namespace ColorFactory.Benchmark
{
    /// <summary>
    /// Baseline C# managed cho batch hint: chấm điểm mọi cặp (from, to) của mỗi bàn.
    /// Luật và điểm phải khớp với HintSolver và HintScoreJob.
    /// </summary>
    public static class ManagedHintBatchScorer
    {
        /// <summary>
        /// Điểm của nước đi không hợp lệ; mọi nước hợp lệ có điểm không âm.
        /// </summary>
        public const int InvalidScore = -1;

        /// <summary>
        /// Chấm điểm toàn bộ batch, kết quả theo chỉ số (board * T + from) * T + to.
        /// </summary>
        public static int[] ScoreAll(HintBatchData data)
        {
            int trayCount = data.TrayCount;
            var scores = new int[data.BoardCount * trayCount * trayCount];

            for (int board = 0; board < data.BoardCount; board++)
            {
                for (int from = 0; from < trayCount; from++)
                {
                    for (int to = 0; to < trayCount; to++)
                    {
                        int scoreIndex = ((board * trayCount) + from) * trayCount + to;
                        scores[scoreIndex] = ScoreMove(data, board, from, to);
                    }
                }
            }

            return scores;
        }

        /// <summary>
        /// Kiểm tra nước đi theo luật game một cách độc lập với bước chấm điểm.
        /// </summary>
        public static bool IsValidMove(HintBatchData data, int board, int from, int to)
        {
            int trayCount = data.TrayCount;
            int capacity = data.Capacity;

            if (from == to)
                return false;

            int fromCount = data.Counts[board * trayCount + from];
            int toCount = data.Counts[board * trayCount + to];

            if (fromCount == 0 || toCount >= capacity)
                return false;

            if (toCount == 0)
                return true;

            int fromTop = data.Colors[ColorIndex(data, board, from, fromCount - 1)];
            int toTop = data.Colors[ColorIndex(data, board, to, toCount - 1)];

            return fromTop == toTop;
        }

        /// <summary>
        /// Chấm điểm một nước đi giống HintSolver.ScoreMove; trả InvalidScore nếu không hợp lệ.
        /// </summary>
        private static int ScoreMove(HintBatchData data, int board, int from, int to)
        {
            if (!IsValidMove(data, board, from, to))
                return InvalidScore;

            int trayCount = data.TrayCount;
            int capacity = data.Capacity;
            int fromCount = data.Counts[board * trayCount + from];
            int toCount = data.Counts[board * trayCount + to];

            int score = 0;

            if (toCount > 0)
                score += 100 + CountTopColorRun(data, board, to, toCount) * 10;
            else
                score += 20;

            if (fromCount == 1)
                score += 30;

            if (toCount + 1 == capacity)
                score += 15;

            return score;
        }

        /// <summary>
        /// Đếm số toy liên tiếp cùng màu ở đỉnh khay.
        /// </summary>
        private static int CountTopColorRun(HintBatchData data, int board, int tray, int count)
        {
            int topColor = data.Colors[ColorIndex(data, board, tray, count - 1)];
            int run = 0;

            for (int slot = count - 1; slot >= 0; slot--)
            {
                if (data.Colors[ColorIndex(data, board, tray, slot)] != topColor)
                    break;

                run++;
            }

            return run;
        }

        /// <summary>
        /// Quy đổi (board, tray, slot) sang chỉ số trong mảng Colors.
        /// </summary>
        private static int ColorIndex(HintBatchData data, int board, int tray, int slot)
        {
            return ((board * data.TrayCount) + tray) * data.Capacity + slot;
        }
    }
}
