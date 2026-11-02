# Bài 22 — Feedback khi di chuyển và hoàn thành

## Cấu hình đã thêm

`BoardRoot` trong `GamePlay` có component `BoardFeedbackView`.

- `Landing Particle Count`: 8.
- `Completion Particle Count`: 36.
- ParticleSystem được tạo một lần lúc Awake, world-space, sorting order 6 (landing) và 7 (completion), không Instantiate theo từng move.
- Pulse 0.12 giây chạy sau khi toy đổi parent vào khay đích; callback animation chỉ được gọi sau pulse, nên completion card không thể hiện trước khi toy đáp.

Không cần particle prefab hoặc texture mới: built-in ParticleSystem đáp ứng sparkle nhẹ và được hủy cùng `BoardRoot` trước khi `GameBootstrap` release Addressables scope.

## Kiểm thử Unity Editor

1. Mở `GamePlay`, chọn `BoardRoot`; Inspector phải có `Board Feedback View` với hai particle count phía trên.
2. Play từ `LevelSelect`, thực hiện move hợp lệ: toy bay sang khay đích, toy/khay pulse và có sparkle màu toy.
3. Trong lúc toy bay hoặc pulse, chạm tray và Undo; MoveCount, board state và selection không được đổi lần nữa.
4. Undo: toy bay theo chiều ngược lại và có landing feedback như move.
5. Hoàn thành level: completion sparkle xuất hiện trước hoặc cùng completion card, chỉ sau toy cuối đáp.
6. Chuyển về Level Select trong khi particle còn sống; Console không được có lỗi pool, missing reference hay Addressables release.
