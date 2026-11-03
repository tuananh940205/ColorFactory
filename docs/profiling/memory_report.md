# Memory report — Color Factory

**Trạng thái: CHƯA ĐO.** File này chỉ có cấu trúc. Không ghi số ước lượng. Điền số chỉ khi lấy từ Unity Profiler / Memory Profiler trên thiết bị Android thật.

## Thiết bị và build

| Mục | Giá trị |
|---|---|
| Thiết bị | TBD (model, RAM, Android version) |
| Unity | 6000.3.25f1 |
| Build | Development Build, IL2CPP, ARM64, Autoconnect Profiler bật, Deep Profiling tắt |
| Workload | 10 vòng Level Select → Gameplay → Level Select, level 01 |
| Ngày đo | TBD |

## Số liệu (Memory Profiler snapshot)

| Thời điểm | Total Used (MB) | Managed Heap (MB) | Native (MB) | Ghi chú |
|---|---|---|---|---|
| Sau khi mở Level Select lần đầu | TBD | TBD | TBD | |
| Trong Gameplay (level 01, giữa ván) | TBD | TBD | TBD | |
| Sau khi về Level Select (vòng 1) | TBD | TBD | TBD | |
| Sau khi về Level Select (vòng 10) | TBD | TBD | TBD | So sánh với vòng 1 để kiểm tra rò rỉ |

Kết luận rò rỉ: TBD (tăng đáng kể giữa vòng 1 và vòng 10 thì ghi là nghi ngờ rò rỉ, kèm snapshot).

## Asset và pool

| Mục | Vòng 1 | Vòng 10 | Ghi chú |
|---|---|---|---|
| Addressables handle còn giữ (Event Viewer) | TBD | TBD | Kỳ vọng sau khi về Level Select chỉ còn asset của scene đó |
| Số instance pool active | TBD | TBD | Đếm trong Hierarchy dưới PoolService khi đang chơi |
| Số instance pool inactive | TBD | TBD | Kỳ vọng không tăng vô hạn |

## Native collections

Console không được có cảnh báo `A Native Collection has not been disposed`: TBD (ghi kết quả thật).

## Ảnh đính kèm

- Snapshot Memory Profiler: TBD (đường dẫn file `.snap` nếu lưu).
- Screenshot Profiler: `docs/profiling/profiler_gameplay.png` (chưa có).
