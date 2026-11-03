# Addressables lifecycle — Color Factory

**Trạng thái: CHƯA ĐO.** Các số liệu dưới đây phải lấy từ Addressables Event Viewer trên thiết bị hoặc Editor Play Mode. Không ghi số ước lượng.

## Kiểm thử tự động đã có (đã chạy)

| Bộ test | Kết quả | Evidence |
|---|---|---|
| Play Mode `AddressableLifecycleTests` | 5/5 passed | `docs/profiling/test_playmode_results.xml` |

Các test này kiểm tra: key sai, key không tồn tại (`Level_999`), hủy load, release nhiều lần, hai scope chia sẻ một asset.

## Kiểm thử thủ công 10 vòng

Quy trình: Level Select → chọn level 01 → Gameplay → quay về Level Select, lặp 10 lần.

| Vòng | Handle còn giữ sau khi về Level Select | Asset còn giữ (danh sách) | Pool active | Pool inactive | Cảnh báo Console | Ghi chú |
|---|---|---|---|---|---|---|
| 1 | TBD | TBD | TBD | TBD | TBD | |
| 2 | TBD | TBD | TBD | TBD | TBD | |
| 3 | TBD | TBD | TBD | TBD | TBD | |
| 4 | TBD | TBD | TBD | TBD | TBD | |
| 5 | TBD | TBD | TBD | TBD | TBD | |
| 6 | TBD | TBD | TBD | TBD | TBD | |
| 7 | TBD | TBD | TBD | TBD | TBD | |
| 8 | TBD | TBD | TBD | TBD | TBD | |
| 9 | TBD | TBD | TBD | TBD | TBD | |
| 10 | TBD | TBD | TBD | TBD | TBD | |

## Kết luận

- Handle có tăng qua các vòng không: TBD.
- Pool có tăng qua các vòng không: TBD.
- Scene có unload đúng sau mỗi vòng không: TBD.
- Có cảnh báo Addressables hoặc Native Collection nào không: TBD.

Thiết bị, build và thời điểm đo: TBD (xem `memory_report.md`).
