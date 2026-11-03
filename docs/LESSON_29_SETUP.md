# Bài 29 — Profiling và Addressables lifecycle evidence

## Trạng thái

- **Code**: đã thêm `PoolService.ActiveCount` và `PoolService.InactiveCount` (đọc-only, dùng cho diagnostic và memory report). Có EditMode test `PoolServiceCounterTests`.
- **Tài liệu khung**: `docs/profiling/memory_report.md` và `docs/profiling/addressables_lifecycle.md` đã có cấu trúc, toàn bộ số liệu là **TBD**.
- **Chưa có evidence thật**: profiler screenshot, Memory Profiler snapshot, Addressables Event Viewer, 10 vòng thủ công, và `docs/profiling/benchmark.csv` (từ Bài 26) đều chưa được tạo. Các việc này cần chạy trên Android thật, nên không thể làm trong phiên này.

Acceptance của bài yêu cầu các file `profiler_gameplay.png`, `memory_report.md`, `addressables_lifecycle.md`, `benchmark.csv`, và ghi rõ thiết bị/build/workload. Hiện mới có hai file markdown khung.

## Build Android để đo

1. `File > Build Profiles`, chọn Android.
2. Bật: **Development Build**, **Autoconnect Profiler**. Tắt: **Deep Profiling** (làm lệch số đo).
3. Scripting Backend: **IL2CPP**; Target Architecture: **ARM64**.
4. Scene List: `LevelSelect` đứng đầu, `Gameplay` sau. Không thêm scene benchmark.
5. Cài bản build lên máy thật, cắm USB, bật USB debugging.

## Profiler

1. Mở `Window > Analysis > Profiler`.
2. Trong dropdown chọn **Android Device** và chọn đúng máy (không chọn Editor nếu muốn số liệu thiết bị).
3. Chạy app trên máy, vào Gameplay level 01, chơi một ván vài nước đi.
4. Chụp cửa sổ Profiler (có timeline CPU, Memory, Rendering) và lưu vào `docs/profiling/profiler_gameplay.png`.

## Memory Profiler

1. Cài package: `Window > Package Manager > Unity Registry > Memory Profiler`. Việc này sửa `Packages/manifest.json`, nên hãy commit cùng với lần cài.
2. Trên Profiler, chọn **Memory Profiler** khi đang kết nối máy.
3. Chụp snapshot ở bốn thời điểm trong bảng `memory_report.md`: mở Level Select lần đầu, giữa ván Gameplay, sau khi về Level Select vòng 1, sau khi về Level Select vòng 10.
4. Điền số thật vào `memory_report.md`. Không điền số ước lượng.

## Addressables Event Viewer

1. Mở `Window > Asset Management > Addressables > Event Viewer`.
2. Kết nối với Profiler đang chạy trên máy (hoặc Play Mode trong Editor để kiểm tra nhanh).
3. Sau mỗi vòng, xem các handle đang giữ. Sau khi về Level Select, chỉ asset của Level Select còn giữ; asset của Gameplay phải được release.
4. Ghi kết quả vào `addressables_lifecycle.md`.

## Test thủ công 10 vòng

Thực hiện một lần trên máy thật:
1. Mở app ở Level Select.
2. Chọn level 01 → Gameplay → quay về Level Select.
3. Lặp lại 10 lần. Sau mỗi vòng: ghi handle Addressables, số pool active/inactive (đếm trong Hierarchy dưới object `PoolService`), và cảnh báo Console.

Kết quả mong đợi trên Console: không có `A Native Collection has not been disposed`, không có cảnh báo Addressables về handle không release.

## Benchmark Bài 26 vẫn còn thiếu

`docs/profiling/benchmark.csv` là một phần của evidence Bài 29. Bài 26 vẫn chưa có số đo: cần chạy `HintBatchBenchmark` trên Android (xem `LESSON_26_SETUP.md`) rồi `adb pull` file CSV vào `docs/profiling/`.

## Ghi chú

- Diagnostic chỉ đọc. Không thêm fake metrics và không thêm log trong gameplay.
- Các file trong `docs/profiling/` chỉ được chứa số đo thật. Nếu chưa đo thì giữ nguyên `TBD`.
