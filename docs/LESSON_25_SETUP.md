# Bài 25 — Analytics mock và gameplay events

## Code đã thêm

- `Analytics/IAnalyticsService`: giao diện ghi event. Gameplay chỉ gọi `Services.Analytics?.Track(new XxxEvent(...))`, không biết SDK thật.
- `Analytics/IAnalyticsEvent`: hợp đồng của mọi event, mỗi event có `EventName` ổn định.
- `Analytics/Events/*`: 8 event kiểu `readonly struct`, payload chỉ gồm id level, số nước, khay, sao, coin và enum.
- `Analytics/LocalAnalyticsService`: bản mock lưu event trong bộ nhớ (giới hạn số lượng để không tăng mãi), có thể in ra Console.
- `Analytics/AnalyticsService`: MonoBehaviour persistent, tạo mock và đăng ký vào `Services.Analytics`.
- `Analytics/BoosterType`, `Analytics/BoosterFailureReason`: enum cho event booster.
- `Core/Services`: thêm `Analytics`.

Event được ghi nhận:

| EventName | Khi nào | Nơi ghi |
|---|---|---|
| `level_started` | Level load xong, trước khi nhận input | `GameBootstrap.InitializeAsync` |
| `move_made` | Nước đi hợp lệ | `BoardController.HandleTrayClicked` |
| `undo_used` | Undo thành công và đã trừ coin | `BoardController.TryUndoLastMove` |
| `hint_used` | Hint đã hiển thị và đã trừ coin | `BoardController.RequestHint` |
| `booster_failed` | Hint/Undo bị từ chối vì thiếu coin hoặc không có ví | `BoardController.TrackBoosterFailure` |
| `level_won` | Level hoàn thành, kèm sao và coin thưởng | `BoardController.HandleMoveAnimationCompleted` |
| `restart_requested` | Bấm Restart từ header hoặc completion card, khi scene fade chấp nhận | `GameplayHeaderView`, `LevelCompleteView` |
| `level_selected` | Chọn level ở Level Select, khi scene fade chấp nhận | `LevelSelectController.OpenLevel` |

Không có dữ liệu định danh người chơi, thiết bị hay thông tin nhạy cảm nào được gửi. Chỉ có id level, số nước, chỉ số khay, số sao và coin.

Event được ghi sau khi hành động đã thành công. Move bị từ chối, Undo/Hint không đủ điều kiện, hay nút đang bị khóa đều không tạo event (ngoại trừ `booster_failed` khi thiếu coin, vì đó là thông tin cần biết để đánh giá booster).

## Cấu hình Unity Editor

1. Mở `LevelSelect`, chọn object đang chứa `AddressableService` và `WalletService` (cùng object persistent).
2. Add component `AnalyticsService`.
   - `Log To Console`: tắt mặc định. Bật khi cần xem event trong Console khi phát triển.
   - `Max Buffered Events`: 200.
3. Lặp lại bước 1–2 trong `GamePlay`. Chỉ có một instance mỗi lần chạy: object thứ hai tự hủy khi đã có service từ scene trước.
4. Không cần Addressables key hay prefab mới.

## Kiểm thử Unity Editor

1. Play từ `LevelSelect`, bật `Log To Console`. Console hiển thị `[Analytics] level_selected levelId=1` khi chọn Level 01.
2. Vào Gameplay: Console hiển thị `[Analytics] level_started levelId=1`.
3. Thực hiện một move hợp lệ: `move_made` với `moves=1`. Move bị từ chối không có log.
4. Undo (khi đủ coin): `undo_used`. Hint (khi đủ coin): `hint_used`.
5. Khi coin không đủ, bấm Hint hoặc Undo: `booster_failed ... reason=InsufficientCoins`, board không đổi.
6. Hoàn thành level: `level_won` với số sao và coin thưởng, đúng một lần.
7. Bấm Restart từ header hoặc completion card: `restart_requested` đúng một lần, sau khi scene fade bắt đầu.
8. Đổi scene nhiều lần: Hierarchy chỉ có một `AnalyticsService`, không có log trùng.
9. Tắt `Log To Console`: không còn log, nhưng gameplay vẫn hoạt động bình thường.

Kết quả Console mong đợi: không có exception từ `Analytics`, không có log lặp.

## Ghi chú

- `LocalAnalyticsService.BufferedEvents` có thể dùng để xác minh event trong Edit Mode test. Test này sẽ được viết trong Bài 28 cùng bộ test còn lại.
- Chưa có SDK analytics thật; khi tích hợp, chỉ cần thay implementation của `IAnalyticsService`.
