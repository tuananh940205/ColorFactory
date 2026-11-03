namespace ColorFactory.Benchmark
{
    /// <summary>
    /// Chọn nước đi tốt nhất cho từng bàn từ mảng điểm; dùng chung cho cả baseline và job.
    /// Khi hòa điểm, giữ nước đi có from/to nhỏ hơn, giống HintSolver.
    /// </summary>
    public static class HintBatchReducer
    {
        /// <summary>
        /// Duyệt mảng điểm theo thứ tự cố định để kết quả hai đường chạy giống hệt nhau.
        /// </summary>
        public static BatchHint[] ReduceBest(int[] scores, int boardCount, int trayCount)
        {
            var results = new BatchHint[boardCount];

            for (int board = 0; board < boardCount; board++)
            {
                int bestScore = int.MinValue;
                int bestFrom = -1;
                int bestTo = -1;

                for (int from = 0; from < trayCount; from++)
                {
                    for (int to = 0; to < trayCount; to++)
                    {
                        int score = scores[((board * trayCount) + from) * trayCount + to];

                        if (score == ManagedHintBatchScorer.InvalidScore || score <= bestScore)
                            continue;

                        bestScore = score;
                        bestFrom = from;
                        bestTo = to;
                    }
                }

                results[board] = bestFrom < 0
                    ? new BatchHint(false, -1, -1, 0)
                    : new BatchHint(true, bestFrom, bestTo, bestScore);
            }

            return results;
        }
    }
}
