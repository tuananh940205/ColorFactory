# Bài 21 — Gameplay SpriteAtlas

## Asset đã tạo

`Assets/_Project/Art/Atlases/GameplayAtlas.spriteatlas` đóng gói toàn bộ thư mục `Art/Sprites/Gameplay`, gồm tray và toy sprites. Atlas được `Include in Build`, padding 4, không mipmap, và Android override 2048 với texture compression.

Atlas được đánh dấu Addressable trong group `Local_Gameplay` bằng key `GameplayAtlas`. Level 01–03 cùng tham chiếu key này qua `gameplaySpriteAtlasKey`; `GameBootstrap` đã load atlas theo scope trước khi gameplay dùng prefab.

Không tạo `UIAtlas`: UI hiện chỉ dùng sprite built-in của Unity và TMP text, không có sprite UI project nào để đóng gói.

## Unity Editor verify

1. Chọn `GameplayAtlas` trong Project, xác nhận Pack Preview có `ColorFactory_Toy_v1` và `ColorFactory_Tray_v1`.
2. Mở **Window > Asset Management > Addressables > Groups**, xác nhận `GameplayAtlas` nằm trong `Local_Gameplay` với địa chỉ đúng.
3. Chạy **Build > New Build > Default Build Script** trong cửa sổ Addressables.
4. Vào Play Mode từ Level Select và mở Level 01–03; tray/toy phải render đúng như trước, Console không có Addressables error.
