# Bài 20 — Scene fade

## Cấu hình đã có trong project

`AddressableService` trong cả `LevelSelect` và `GamePlay` đã có component `SceneFader`.
Khi chạy, component tạo child `SceneFadeOverlay` persistent với:

- Canvas `Screen Space - Overlay`, override sorting order `1000`.
- Image stretch toàn màn hình, navy `#061224`.
- CanvasGroup để nội suy alpha và chặn raycast trong lúc chuyển scene.

Không cần tạo prefab hay gán reference thủ công. Cách này vẫn cho phép mở trực tiếp từng scene trong Editor và giữ đúng một fader qua các scene.

## Kiểm thử thủ công

1. Mở `LevelSelect`, vào Play Mode, nhấn Level 01. Màn hình phải fade sang navy rồi hiện Gameplay, không lộ frame của menu cũ.
2. Trong Gameplay, nhấn Restart ở header và Play Again trong completion card. Cả hai phải fade-out/fade-in và tải lại cùng level.
3. Hoàn thành Level 01 rồi nhấn Next Level. Level được chọn phải là level kế tiếp và fade diễn ra một lần.
4. Bấm nhanh nhiều lần vào một nút chuyển scene. Chỉ request đầu tiên được nhận; CanvasGroup chặn các request còn lại.

Kết quả Console mong đợi: không có lỗi hay exception liên quan đến `SceneFader`, `AddressableService`, hoặc scene loading.
