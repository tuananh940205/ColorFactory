using System;
using System.Diagnostics;
using System.IO;
using Unity.Profiling;
using UnityEngine;

namespace ColorFactory.Benchmark
{
    /// <summary>
    /// Đo baseline managed và đường Jobs/Burst trên cùng một batch tổng hợp,
    /// kiểm tra hai đường cho kết quả giống nhau rồi ghi một dòng vào CSV.
    /// Chạy đồng bộ trong một lần gọi nên không còn job nào khi OnDestroy;
    /// nếu chuyển sang async thì phải Complete job trước khi Dispose.
    /// </summary>
    public sealed class HintBatchBenchmark : MonoBehaviour
    {
        private const string CsvHeader =
            "timestamp_utc,platform,device_model,processor,scripting_backend,development_build," +
            "boards,trays,capacity,colors,seed,iterations,managed_ms_avg,job_ms_avg,speedup,parity_ok";

        private static readonly ProfilerMarker ManagedMarker = new("HintBatch.Managed");
        private static readonly ProfilerMarker JobMarker = new("HintBatch.Job");

        [Header("Workload")]
        [SerializeField, Min(1)] private int boardCount = 4096;
        [SerializeField, Min(2)] private int trayCount = 8;
        [SerializeField, Min(1)] private int capacity = 6;
        [SerializeField, Min(1)] private int colorCount = 6;
        [SerializeField] private int seed = 20260101;

        [Header("Measurement")]
        [SerializeField, Min(0)] private int warmupIterations = 3;
        [SerializeField, Min(1)] private int iterations = 20;
        [SerializeField] private bool runOnStart;

        [Header("Output")]
        [Tooltip("Ghi vào Application.persistentDataPath; lấy file bằng adb pull hoặc xem đường dẫn trong Console.")]
        [SerializeField] private string outputFileName = "benchmark.csv";

        /// <summary>
        /// Chạy benchmark khi Inspector bật runOnStart.
        /// </summary>
        private void Start()
        {
            if (runOnStart)
                Run();
        }

        /// <summary>
        /// Warm-up, đo trung bình hai đường, kiểm tra parity và ghi một dòng CSV.
        /// </summary>
        [ContextMenu("Run Benchmark")]
        public void Run()
        {
            HintBatchData data = HintBatchGenerator.Generate(
                boardCount,
                trayCount,
                capacity,
                colorCount,
                seed);

            var managedScores = new int[boardCount * trayCount * trayCount];

            // Một NativeHintBatch cho cả lần chạy, cấp phát ngoài vùng đo và được Dispose khi xong.
            using var nativeBatch = new NativeHintBatch(boardCount, trayCount, capacity);

            // Warm-up không tính vào số đo; lần đầu cũng là lúc Burst biên dịch job.
            for (int warmup = 0; warmup < warmupIterations; warmup++)
            {
                RunManaged(data);
                RunJob(data, nativeBatch, managedScores);
            }

            BatchHint[] managedResults = RunManaged(data);
            BatchHint[] jobResults = RunJob(data, nativeBatch, managedScores);

            bool parityOk = CheckParity(data, managedResults, jobResults);

            double managedMs = MeasureAverageMs(() => RunManaged(data));
            double jobMs = MeasureAverageMs(() => RunJob(data, nativeBatch, managedScores));

            double speedup = jobMs > 0 ? managedMs / jobMs : 0;

            string csvPath = Path.Combine(Application.persistentDataPath, outputFileName);
            AppendCsvRow(csvPath, managedMs, jobMs, speedup, parityOk);

            UnityEngine.Debug.Log(
                $"[HintBatchBenchmark] managed={managedMs:F3} ms job={jobMs:F3} ms " +
                $"speedup={speedup:F2} parity={parityOk} csv={csvPath}",
                this);

            if (!parityOk)
                UnityEngine.Debug.LogError("[HintBatchBenchmark] Managed and job results differ.", this);
        }

        /// <summary>
        /// Chấm điểm và chọn nước đi tốt nhất bằng baseline managed.
        /// </summary>
        private static BatchHint[] RunManaged(HintBatchData data)
        {
            using (ManagedMarker.Auto())
            {
                int[] scores = ManagedHintBatchScorer.ScoreAll(data);
                return HintBatchReducer.ReduceBest(scores, data.BoardCount, data.TrayCount);
            }
        }

        /// <summary>
        /// Upload dữ liệu, schedule job, chờ xong, copy điểm về managed rồi reduce cùng baseline.
        /// </summary>
        private static BatchHint[] RunJob(
            HintBatchData data,
            NativeHintBatch nativeBatch,
            int[] managedScores)
        {
            using (JobMarker.Auto())
            {
                nativeBatch.Upload(data);
                nativeBatch.Schedule();
                nativeBatch.CopyScoresTo(managedScores);

                return HintBatchReducer.ReduceBest(managedScores, data.BoardCount, data.TrayCount);
            }
        }

        /// <summary>
        /// So sánh từng bàn và kiểm tra mọi nước đi được chọn đều hợp lệ theo luật game.
        /// </summary>
        private static bool CheckParity(
            HintBatchData data,
            BatchHint[] managedResults,
            BatchHint[] jobResults)
        {
            for (int board = 0; board < data.BoardCount; board++)
            {
                BatchHint managed = managedResults[board];
                BatchHint job = jobResults[board];

                if (managed.Found != job.Found ||
                    managed.From != job.From ||
                    managed.To != job.To ||
                    managed.Score != job.Score)
                {
                    return false;
                }

                if (managed.Found &&
                    !ManagedHintBatchScorer.IsValidMove(data, board, managed.From, managed.To))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Đo trung bình thời gian của một hành động, đơn vị mili giây.
        /// </summary>
        private double MeasureAverageMs(Action action)
        {
            var stopwatch = Stopwatch.StartNew();

            for (int iteration = 0; iteration < iterations; iteration++)
                action();

            stopwatch.Stop();

            return stopwatch.Elapsed.TotalMilliseconds / iterations;
        }

        /// <summary>
        /// Ghi header nếu file chưa có, rồi thêm một dòng kết quả kèm thông tin thiết bị.
        /// </summary>
        private void AppendCsvRow(
            string path,
            double managedMs,
            double jobMs,
            double speedup,
            bool parityOk)
        {
            if (!File.Exists(path))
                File.WriteAllText(path, CsvHeader + Environment.NewLine);

            string backend =
#if ENABLE_IL2CPP
                "IL2CPP";
#else
                "Mono";
#endif

            string row = string.Join(",",
                DateTime.UtcNow.ToString("O"),
                Application.platform,
                SystemInfo.deviceModel,
                SystemInfo.processorType,
                backend,
                UnityEngine.Debug.isDebugBuild,
                boardCount,
                trayCount,
                capacity,
                colorCount,
                seed,
                iterations,
                managedMs.ToString("F4"),
                jobMs.ToString("F4"),
                speedup.ToString("F3"),
                parityOk);

            File.AppendAllText(path, row + Environment.NewLine);
        }
    }
}
