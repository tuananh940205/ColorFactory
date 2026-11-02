# Bài 24 — Economy cho Hint và Undo

## Cấu hình đã thêm

`WalletService` được gắn vào object `AddressableService` trong cả `LevelSelect` và `GamePlay`, nên ví còn hoạt động khi mở trực tiếp từng scene và chỉ có một instance persistent.

| Giá trị | Mặc định |
| --- | ---: |
| Coin lần đầu | 30 |
| Hint | 3 |
| Undo | 2 |
| Hoàn thành level | +10 |

Các giá trị này nằm trên Inspector của `WalletService`. Số dư dùng PlayerPrefs key `wallet.coins`; mọi giao dịch được clamp để không thể âm.

Gameplay header hiển thị coin trong Moves badge. Nút Hint/Undo tự ghi cost vào label và bị khóa khi thiếu coin. Completion card hiển thị reward trong dòng summary.

## Kiểm thử Unity Editor

1. Xóa PlayerPrefs của app hoặc đổi key `wallet.coins`, sau đó Play từ `LevelSelect`; HUD phải bắt đầu `COINS: 30`.
2. Nhấn Hint khi có nước hợp lệ: highlight xuất hiện trước, coin giảm 3; restart scene vẫn giữ số dư.
3. Thực hiện một move rồi nhấn Undo: toy undo thành công, coin giảm 2. Khi không có history hoặc không đủ coin, board và coin không đổi.
4. Dùng hết coin, xác nhận Hint/Undo bị khóa và không thực thi action.
5. Hoàn thành level: completion summary hiện `REWARD: +10 COINS`; restart game, tổng coin vẫn còn.

Kết quả Console mong đợi: không có exception WalletService, không có số dư âm.
