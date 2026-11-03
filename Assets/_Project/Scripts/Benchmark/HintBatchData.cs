using System;

namespace ColorFactory.Benchmark
{
    /// <summary>
    /// Dữ liệu phẳng của một batch nhiều bàn chơi, dùng chung cho baseline managed và Jobs/Burst.
    /// </summary>
    public sealed class HintBatchData
    {
        public int BoardCount { get; }
        public int TrayCount { get; }
        public int Capacity { get; }

        /// <summary>
        /// Màu của toy theo chỉ số ((board * TrayCount + tray) * Capacity + slot), slot 0 là đáy khay.
        /// Chỉ các slot dưới Counts có ý nghĩa.
        /// </summary>
        public int[] Colors { get; }

        /// <summary>
        /// Số toy trong từng khay theo chỉ số board * TrayCount + tray.
        /// </summary>
        public int[] Counts { get; }

        /// <summary>
        /// Nhận dữ liệu đã dựng sẵn và kiểm tra kích thước mảng trước khi dùng.
        /// </summary>
        public HintBatchData(
            int boardCount,
            int trayCount,
            int capacity,
            int[] colors,
            int[] counts)
        {
            if (boardCount < 1 || trayCount < 2 || capacity < 1)
                throw new ArgumentOutOfRangeException(nameof(boardCount));

            if (colors == null || colors.Length != boardCount * trayCount * capacity)
                throw new ArgumentException("Colors length does not match the batch shape.", nameof(colors));

            if (counts == null || counts.Length != boardCount * trayCount)
                throw new ArgumentException("Counts length does not match the batch shape.", nameof(counts));

            BoardCount = boardCount;
            TrayCount = trayCount;
            Capacity = capacity;
            Colors = colors;
            Counts = counts;
        }
    }
}
