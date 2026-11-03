using System;
using Unity.Collections;
using Unity.Jobs;

namespace ColorFactory.Benchmark
{
    /// <summary>
    /// Sở hữu toàn bộ NativeArray của một batch hint và quản lý job đang chạy.
    /// Dispose luôn Complete job trước khi giải phóng bộ nhớ native, và gọi nhiều lần vẫn an toàn.
    /// </summary>
    public sealed class NativeHintBatch : IDisposable
    {
        private readonly int boardCount;
        private readonly int trayCount;
        private readonly int capacity;

        private NativeArray<int> colors;
        private NativeArray<int> counts;
        private NativeArray<int> scores;

        private JobHandle pendingJob;
        private bool hasPendingJob;
        private bool isDisposed;

        /// <summary>
        /// Cấp phát bộ nhớ Persistent đúng kích thước batch; gọi một lần cho mỗi lần đo.
        /// </summary>
        public NativeHintBatch(int boardCount, int trayCount, int capacity)
        {
            this.boardCount = boardCount;
            this.trayCount = trayCount;
            this.capacity = capacity;

            colors = new NativeArray<int>(
                boardCount * trayCount * capacity,
                Allocator.Persistent,
                NativeArrayOptions.UninitializedMemory);

            counts = new NativeArray<int>(
                boardCount * trayCount,
                Allocator.Persistent,
                NativeArrayOptions.UninitializedMemory);

            scores = new NativeArray<int>(
                boardCount * trayCount * trayCount,
                Allocator.Persistent,
                NativeArrayOptions.UninitializedMemory);
        }

        /// <summary>
        /// Sao chép dữ liệu batch vào bộ nhớ native; kích thước phải khớp lúc tạo.
        /// </summary>
        public void Upload(HintBatchData data)
        {
            ThrowIfDisposed();
            Complete();

            if (data.BoardCount != boardCount ||
                data.TrayCount != trayCount ||
                data.Capacity != capacity)
            {
                throw new ArgumentException("Batch shape does not match this native batch.");
            }

            colors.CopyFrom(data.Colors);
            counts.CopyFrom(data.Counts);
        }

        /// <summary>
        /// Lên lịch job chấm điểm; không được gọi khi job trước vẫn đang chạy.
        /// </summary>
        public void Schedule()
        {
            ThrowIfDisposed();
            Complete();

            var job = new HintScoreJob
            {
                Colors = colors,
                Counts = counts,
                Scores = scores,
                TrayCount = trayCount,
                Capacity = capacity
            };

            pendingJob = job.Schedule(scores.Length, 64);
            hasPendingJob = true;
        }

        /// <summary>
        /// Chờ job đang chạy hoàn tất; an toàn để gọi nhiều lần.
        /// </summary>
        public void Complete()
        {
            if (!hasPendingJob)
                return;

            pendingJob.Complete();
            hasPendingJob = false;
        }

        /// <summary>
        /// Chờ job xong rồi sao chép điểm sang mảng managed.
        /// </summary>
        public void CopyScoresTo(int[] destination)
        {
            ThrowIfDisposed();
            Complete();

            scores.CopyTo(destination);
        }

        /// <summary>
        /// Complete job, rồi giải phóng toàn bộ NativeArray; gọi lần thứ hai không làm gì.
        /// </summary>
        public void Dispose()
        {
            if (isDisposed)
                return;

            Complete();

            colors.Dispose();
            counts.Dispose();
            scores.Dispose();

            isDisposed = true;
        }

        /// <summary>
        /// Ngăn dùng NativeArray sau khi đã giải phóng.
        /// </summary>
        private void ThrowIfDisposed()
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(NativeHintBatch));
        }
    }
}
