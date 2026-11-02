using System;
using System.Collections.Generic;
using System.Linq;
using ColorFactory.Core;
using ColorFactory.Levels;

namespace ColorFactory.Gameplay
{
    /// <summary>
    /// Lưu thông tin một nước đi để gameplay có thể hoàn tác và animate đúng chiều.
    /// </summary>
    public readonly struct BoardMove
    {
        public int From { get; }
        public int To { get; }
        public ToyColor Color { get; }

        public BoardMove(int from, int to, ToyColor color)
        {
            From = from;
            To = to;
            Color = color;
        }
    }
    /// <summary>
    /// Model thuần C# lưu trạng thái khay, luật di chuyển
    /// và lịch sử nước đi để hỗ trợ Undo.
    /// </summary>
    public sealed class BoardModel
    {
        /// <summary>
        /// Dữ liệu tối thiểu cần thiết để hoàn tác một nước đi.
        /// </summary>
        private readonly struct MoveRecord
        {
            public readonly int From;
            public readonly int To;
            public readonly ToyColor Color;

            public MoveRecord(int from, int to, ToyColor color)
            {
                From = from;
                To = to;
                Color = color;
            }
        }

        private readonly List<List<ToyColor>> trays = new();
        private readonly Stack<BoardMove> moveHistory = new();

        public int Capacity { get; }
        public int TrayCount => trays.Count;
        public bool CanUndo => moveHistory.Count > 0;

        /// <summary>
        /// Tạo trạng thái board độc lập từ dữ liệu level.
        /// </summary>
        public BoardModel(LevelData level)
        {
            if (level == null)
                throw new ArgumentNullException(nameof(level));

            if (level.trayCapacity <= 0)
                throw new ArgumentException("Invalid tray capacity.");

            Capacity = level.trayCapacity;

            foreach (TrayData tray in level.trays)
            {
                if (tray?.colors == null || tray.colors.Count > Capacity)
                    throw new ArgumentException("Invalid tray data.");

                trays.Add(new List<ToyColor>(tray.colors));
            }
        }

        /// <summary>
        /// Trả danh sách màu từ đáy lên đỉnh của một khay.
        /// </summary>
        public IReadOnlyList<ToyColor> GetTray(int index)
        {
            return trays[index].AsReadOnly();
        }

        /// <summary>
        /// Kiểm tra nước đi theo luật game mà không thay đổi board.
        /// </summary>
        public bool CanMove(int from, int to)
        {
            if (from < 0 || from >= TrayCount ||
                to < 0 || to >= TrayCount ||
                from == to)
            {
                return false;
            }

            List<ToyColor> source = trays[from];
            List<ToyColor> target = trays[to];

            if (source.Count == 0 || target.Count >= Capacity)
                return false;

            return target.Count == 0 || target[^1] == source[^1];
        }

        /// <summary>
        /// Di chuyển toy trên cùng và lưu lịch sử để có thể Undo.
        /// </summary>
        public bool TryMove(int from, int to)
        {
            if (!CanMove(from, to))
                return false;

            List<ToyColor> source = trays[from];
            List<ToyColor> target = trays[to];

            ToyColor color = source[^1];

            source.RemoveAt(source.Count - 1);
            target.Add(color);

            moveHistory.Push(new BoardMove(from, to, color));
            return true;
        }

        /// <summary>
        /// Hoàn tác nước đi gần nhất khi caller không cần biết hướng di chuyển.
        /// </summary>
        public bool TryUndo()
        {
            return TryUndo(out _);
        }

        /// <summary>
        /// Hoàn tác nước đi gần nhất và trả thông tin nước đi vừa bị hoàn tác.
        /// </summary>
        public bool TryUndo(out BoardMove undoneMove)
        {
            undoneMove = default;

            if (moveHistory.Count == 0)
                return false;

            BoardMove move = moveHistory.Peek();

            List<ToyColor> source = trays[move.From];
            List<ToyColor> target = trays[move.To];

            // Board hiện tại phải còn đúng toy ở đỉnh khay đích của nước đi trước.
            if (target.Count == 0 ||
                target[^1] != move.Color ||
                source.Count >= Capacity)
            {
                throw new InvalidOperationException(
                    "Board state does not match its undo history.");
            }

            target.RemoveAt(target.Count - 1);
            source.Add(move.Color);

            moveHistory.Pop();
            undoneMove = move;

            return true;
        }

        /// <summary>
        /// Level hoàn thành khi mọi khay không trống đều đầy và chỉ có một màu.
        /// </summary>
        public bool IsCompleted()
        {
            return trays.All(tray =>
                tray.Count == 0 ||
                (tray.Count == Capacity &&
                 tray.All(color => color == tray[0])));
        }
    }
}