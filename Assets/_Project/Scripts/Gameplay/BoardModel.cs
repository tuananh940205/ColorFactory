
using System;
using System.Collections.Generic;
using System.Linq;
using ColorFactory.Core;
using ColorFactory.Levels;

namespace ColorFactory.Gameplay
{
    public class BoardModel
    {
        private readonly List<List<ToyColor>> trays = new();

        public int Capacity { get; }
        public int TrayCount => trays.Count;

        public BoardModel(LevelData level)
        {
            if (level == null)
                throw new ArgumentNullException(nameof(level));

            if (level.trayCapacity <= 0)
                throw new ArgumentException("Invalid tray capacity");

            Capacity = level.trayCapacity;

            foreach (var tray in level.trays)
            {
                if (tray == null || tray.colors == null ||
                    tray.colors.Count > Capacity)
                    throw new ArgumentException("Invalid tray data");

                trays.Add(new List<ToyColor>(tray.colors));
            }
        }

        public IReadOnlyList<ToyColor> GetTray(int index)
        {
            return trays[index].AsReadOnly();
        }

        public bool CanMove(int from, int to)
        {
            if (from < 0 || from >= TrayCount ||
                to < 0 || to >= TrayCount ||
                from == to)
                return false;

            var source = trays[from];
            var target = trays[to];

            if (source.Count == 0 ||
                target.Count >= Capacity)
                return false;

            return target.Count == 0 ||
                target[^1] == source[^1];
        }

        public bool TryMove(int from, int to)
        {
            if (!CanMove(from, to))
                return false;

            var source = trays[from];
            var target = trays[to];

            var color = source[^1];
            source.RemoveAt(source.Count - 1);
            target.Add(color);

            return true;
        }

        public bool IsCompleted()
        {
            return trays.All(tray =>
                tray.Count == 0 ||
                (tray.Count == Capacity &&
                 tray.All(color => color == tray[0])));
        }
    }
}
