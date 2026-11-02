using ColorFactory.Core;

namespace ColorFactory.Gameplay
{
    /// <summary>
    /// Đại diện cho một nước đi được đề xuất cho người chơi.
    /// </summary>
    public readonly struct MoveHint
    {
        public int From { get; }
        public int To { get; }

        public MoveHint(int from, int to)
        {
            From = from;
            To = to;
        }
    }

    /// <summary>
    /// Solver baseline chạy bằng C# thông thường.
    /// Nhiệm vụ hiện tại là chọn một nước đi hợp lệ, ưu tiên ghép cùng màu.
    /// </summary>
    public static class HintSolver
    {
        /// <summary>
        /// Tìm nước đi có điểm cao nhất mà không thay đổi BoardModel.
        /// </summary>
        public static bool TryFindHint(BoardModel board, out MoveHint hint)
        {
            hint = default;

            if (board == null)
                return false;

            int bestScore = int.MinValue;
            bool foundMove = false;

            for (int from = 0; from < board.TrayCount; from++)
            {
                for (int to = 0; to < board.TrayCount; to++)
                {
                    if (!board.CanMove(from, to))
                        continue;

                    int score = ScoreMove(board, from, to);

                    if (score <= bestScore)
                        continue;

                    bestScore = score;
                    hint = new MoveHint(from, to);
                    foundMove = true;
                }
            }

            return foundMove;
        }

        /// <summary>
        /// Chấm điểm đơn giản cho một nước đi.
        /// Ghép vào khay đã có cùng màu luôn tốt hơn chuyển vào khay trống.
        /// </summary>
        private static int ScoreMove(BoardModel board, int from, int to)
        {
            var source = board.GetTray(from);
            var target = board.GetTray(to);

            int score = 0;

            // Ưu tiên ghép màu vào khay đã có toy.
            if (target.Count > 0)
                score += 100 + CountTopColorRun(target) * 10;
            else
                score += 20;

            // Thưởng khi giải phóng hoàn toàn khay nguồn.
            if (source.Count == 1)
                score += 30;

            // Thưởng nhẹ khi khay đích gần hoàn thành.
            if (target.Count + 1 == board.Capacity)
                score += 15;

            return score;
        }

        /// <summary>
        /// Đếm số toy liên tiếp cùng màu ở đỉnh khay.
        /// </summary>
        private static int CountTopColorRun(
            System.Collections.Generic.IReadOnlyList<ToyColor> tray)
        {
            if (tray.Count == 0)
                return 0;

            ToyColor topColor = tray[^1];
            int count = 0;

            for (int index = tray.Count - 1; index >= 0; index--)
            {
                if (tray[index] != topColor)
                    break;

                count++;
            }

            return count;
        }
    }
}