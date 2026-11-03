using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace ColorFactory.Benchmark
{
    /// <summary>
    /// Job chấm điểm song song mọi cặp (from, to) của batch; logic phải khớp ManagedHintBatchScorer.
    /// Mỗi index tương ứng một cặp (board, from, to) và chỉ ghi vào đúng một phần tử của Scores.
    /// </summary>
    [BurstCompile]
    public struct HintScoreJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<int> Colors;
        [ReadOnly] public NativeArray<int> Counts;
        [WriteOnly] public NativeArray<int> Scores;

        public int TrayCount;
        public int Capacity;

        /// <summary>
        /// Giải mã index thành (board, from, to) rồi ghi điểm của cặp đó.
        /// </summary>
        public void Execute(int index)
        {
            int pairsPerBoard = TrayCount * TrayCount;
            int board = index / pairsPerBoard;
            int pair = index - board * pairsPerBoard;
            int from = pair / TrayCount;
            int to = pair - from * TrayCount;

            Scores[index] = ScoreMove(board, from, to);
        }

        /// <summary>
        /// Trả điểm của một nước đi, hoặc -1 nếu không hợp lệ.
        /// </summary>
        private int ScoreMove(int board, int from, int to)
        {
            int fromCount = Counts[board * TrayCount + from];
            int toCount = Counts[board * TrayCount + to];

            if (from == to || fromCount == 0 || toCount >= Capacity)
                return -1;

            if (toCount > 0 &&
                Colors[ColorIndex(board, from, fromCount - 1)] !=
                Colors[ColorIndex(board, to, toCount - 1)])
            {
                return -1;
            }

            int score = 0;

            if (toCount > 0)
                score += 100 + CountTopColorRun(board, to, toCount) * 10;
            else
                score += 20;

            if (fromCount == 1)
                score += 30;

            if (toCount + 1 == Capacity)
                score += 15;

            return score;
        }

        /// <summary>
        /// Đếm số toy liên tiếp cùng màu ở đỉnh khay.
        /// </summary>
        private int CountTopColorRun(int board, int tray, int count)
        {
            int topColor = Colors[ColorIndex(board, tray, count - 1)];
            int run = 0;

            for (int slot = count - 1; slot >= 0; slot--)
            {
                if (Colors[ColorIndex(board, tray, slot)] != topColor)
                    break;

                run++;
            }

            return run;
        }

        /// <summary>
        /// Quy đổi (board, tray, slot) sang chỉ số trong mảng Colors.
        /// </summary>
        private int ColorIndex(int board, int tray, int slot)
        {
            return ((board * TrayCount) + tray) * Capacity + slot;
        }
    }
}
