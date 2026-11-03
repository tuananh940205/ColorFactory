# Bài 26 — Hint solver Jobs/Burst benchmark

## Trạng thái đo

**Chưa có số đo.** Code benchmark đã được viết, nhưng chưa chạy trên Unity Editor hay thiết bị Android, nên `docs/profiling/benchmark.csv` chưa được tạo. Theo AGENTS.md, file này chỉ được chứa số đo thật từ thiết bị. Không ghi số ước lượng.

Gameplay vẫn dùng `HintSolver` managed cho một bàn chơi. Đường Jobs/Burst chỉ tồn tại trong benchmark. Chỉ đưa Jobs vào gameplay nếu số đo trên Android cho thấy nhanh hơn có ý nghĩa.

## Code đã thêm

Thư mục `Assets/_Project/Scripts/Benchmark/`:

- `HintBatchData`: dữ liệu phẳng của batch (màu, số toy theo khay), readonly, kiểm tra kích thước trong constructor.
- `HintBatchGenerator`: sinh batch tổng hợp với seed cố định, mỗi màu đúng bằng sức chứa một khay, mỗi bàn có ít nhất một khay trống.
- `ManagedHintBatchScorer`: baseline C# managed. Chấm điểm mọi cặp (from, to) của mỗi bàn, cùng luật và điểm với `HintSolver`. Có `IsValidMove` độc lập để kiểm tra kết quả.
- `HintScoreJob`: `IJobParallelFor` với `[BurstCompile]`, cùng logic chấm điểm trên `NativeArray`. Mỗi index là một cặp (board, from, to).
- `NativeHintBatch`: sở hữu mọi `NativeArray` (Persistent). `Dispose` luôn `Complete` job trước khi giải phóng và gọi nhiều lần vẫn an toàn.
- `HintBatchReducer`: chọn nước đi tốt nhất cho mỗi bàn từ mảng điểm, dùng chung cho baseline và job, nên tie-break giống nhau.
- `BatchHint`: kết quả một bàn (Found, From, To, Score).
- `HintBatchBenchmark`: MonoBehaviour harness. Warm-up, đo trung bình, kiểm tra parity, ghi CSV.

Package: thêm `com.unity.burst` 1.8.17, `com.unity.collections` 2.4.3, `com.unity.profiling.core` 1.0.2 vào `Packages/manifest.json`. Đây là các phiên bản đã được resolve sẵn trong `packages-lock.json`, nên không đổi version.

## Cách đo đúng

- **Workload mặc định**: 4096 bàn, 8 khay, sức chứa 6, 6 màu, seed `20260101`. Chỉnh trên Inspector; mọi giá trị được ghi vào CSV.
- **Baseline và job** cùng làm đúng các bước: upload/chấm điểm, copy kết quả, reduce. Đường job có tính thời gian upload và copy về managed, vì đó là chi phí thật khi dùng Jobs.
- **Warm-up** 3 vòng, không tính vào số đo. Số đo là trung bình 20 vòng.
- **Parity**: mọi bàn phải có cùng `Found`, `From`, `To`, `Score` giữa hai đường, và nước đi được chọn phải hợp lệ theo `IsValidMove`. Nếu không, dòng CSV ghi `parity_ok = False` và Console báo lỗi.
- Build Editor không phải bằng chứng. Phải chạy **Development Build trên Android**, scripting backend IL2CPP, ARM64.

## Cấu hình Unity Editor

1. **Packages**: mở Unity, để Package Manager resolve lại. Kiểm tra Burst đã có trong Packages: `Burst`, `Collections`, `Profiling Core`.
2. **Burst**: Jobs > Burst > Enable Compilation phải được bật (mặc định bật).
3. **Scene benchmark riêng**: tạo scene `HintBenchmark` và **không** thêm vào Scene List của Build Profile. Trong scene, tạo GameObject `HintBatchBenchmark` và add component `HintBatchBenchmark`.
   - Giữ `Workload`, `Measurement` mặc định cho lần đầu.
   - Với lần chạy trên thiết bị, bật `Run On Start`.
4. **Build Android**: Build Profile Android, Development Build bật, Autoconnect Profiler bật, Deep Profiling tắt (deep profiling làm lệch số đo), IL2CPP, ARM64.
5. **Chạy trên máy thật**: cài build, mở app, chờ Console in dòng `[HintBatchBenchmark] managed=... job=...`.
6. **Lấy CSV**: file nằm trong `Application.persistentDataPath`, trên Android là `/sdcard/Android/data/<package name>/files/benchmark.csv`. Dùng:
   ```
   adb pull /sdcard/Android/data/<package name>/files/benchmark.csv docs/profiling/benchmark.csv
   ```
   Giữ nguyên header và các dòng; đừng sửa số.
7. **Profiler screenshot**: khi chạy benchmark, mở Profiler và chụp màn hình timeline có marker `HintBatch.Managed` và `HintBatch.Job`. Lưu thành `docs/profiling/profiler_hint_batch.png`.
8. **Native leak**: trong Console không được có cảnh báo `A Native Collection has not been disposed`.

## Kiểm thử

- Parity: `parity_ok` phải là `True` trong mọi dòng CSV.
- Chạy lại với seed khác và kích thước batch khác (ví dụ 512 và 16384) để thấy xu hướng; mỗi cấu hình là một dòng riêng.
- Acceptance của bài: baseline và job trả hint hợp lệ; NativeContainer luôn được Dispose; `docs/profiling/benchmark.csv` có số đo thật.

## Ghi chú

- Chưa có Edit Mode test cho `ManagedHintBatchScorer` và `HintBatchReducer`. Các test này sẽ được viết trong Bài 28 cùng bộ test còn lại.
- Lifecycle: benchmark chạy đồng bộ trong một lần gọi, nên không có job nào còn chạy khi `OnDestroy`. Nếu sau này đổi sang async, phải `Complete` job trước khi `Dispose`.
- Benchmark không dùng Addressables, nên không cần lifecycle Addressables cho bài này.
