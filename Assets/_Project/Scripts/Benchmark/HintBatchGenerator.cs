using System;

namespace ColorFactory.Benchmark
{
    /// <summary>
    /// Sinh batch bàn chơi tổng hợp với seed cố định để mọi lần đo dùng cùng workload.
    /// </summary>
    public static class HintBatchGenerator
    {
        /// <summary>
        /// Chia đều toy của mỗi màu vào các khay, đảm bảo mỗi bàn có ít nhất một khay trống.
        /// </summary>
        public static HintBatchData Generate(
            int boardCount,
            int trayCount,
            int capacity,
            int colorCount,
            int seed)
        {
            if (colorCount < 1 || trayCount < colorCount + 1)
                throw new ArgumentException("Need at least one empty tray above the color count.");

            var random = new Random(seed);
            var colors = new int[boardCount * trayCount * capacity];
            var counts = new int[boardCount * trayCount];
            var toys = new int[colorCount * capacity];

            for (int board = 0; board < boardCount; board++)
            {
                FillToys(toys, capacity);
                Shuffle(toys, random);

                int boardTrayBase = board * trayCount;

                foreach (int color in toys)
                {
                    int tray = PickTrayWithRoom(counts, boardTrayBase, trayCount, capacity, random);
                    int slotIndex = ((board * trayCount) + tray) * capacity + counts[boardTrayBase + tray];

                    colors[slotIndex] = color;
                    counts[boardTrayBase + tray]++;
                }
            }

            return new HintBatchData(boardCount, trayCount, capacity, colors, counts);
        }

        /// <summary>
        /// Tạo danh sách toy: mỗi màu đúng bằng sức chứa một khay.
        /// </summary>
        private static void FillToys(int[] toys, int capacity)
        {
            for (int index = 0; index < toys.Length; index++)
                toys[index] = index / capacity;
        }

        /// <summary>
        /// Xáo trộn Fisher-Yates bằng random đã có seed.
        /// </summary>
        private static void Shuffle(int[] items, Random random)
        {
            for (int index = items.Length - 1; index > 0; index--)
            {
                int swapIndex = random.Next(index + 1);
                (items[index], items[swapIndex]) = (items[swapIndex], items[index]);
            }
        }

        /// <summary>
        /// Chọn ngẫu nhiên một khay còn chỗ; nếu rơi vào khay đầy thì quét tuyến tính.
        /// </summary>
        private static int PickTrayWithRoom(
            int[] counts,
            int boardTrayBase,
            int trayCount,
            int capacity,
            Random random)
        {
            for (int attempt = 0; attempt < trayCount; attempt++)
            {
                int tray = random.Next(trayCount);

                if (counts[boardTrayBase + tray] < capacity)
                    return tray;
            }

            for (int tray = 0; tray < trayCount; tray++)
            {
                if (counts[boardTrayBase + tray] < capacity)
                    return tray;
            }

            throw new InvalidOperationException("No tray has room for the toy.");
        }
    }
}
