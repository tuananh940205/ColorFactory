# Bài 27 — Level 04–15 và cân bằng

## Trạng thái

Code, data và validator đã được viết. **Chưa chạy được trong Unity Editor từ phiên làm việc này**, nên chưa có kết quả Console thật. Các kiểm tra dưới đây phải do người dùng chạy trong Editor.

Đã kiểm chứng bằng công cụ Python (`docs/tools/level_solver.py`): decode lại toàn bộ `Level_004`–`Level_015`, số màu, sức chứa, khay trống và số nước tối thiểu (BFS mô phỏng đúng luật `BoardModel`).

## Dữ liệu đã thêm

| Level | Số màu | Sức chứa | Số khay | Nước tối thiểu (BFS) | 3 sao | 2 sao |
|---|---|---|---|---|---|---|
| 04 | 2 | 4 | 3 | 5 | 6 | 8 |
| 05 | 2 | 4 | 3 | 5 | 6 | 8 |
| 06 | 3 | 4 | 4 | 7 | 9 | 12 |
| 07 | 3 | 5 | 4 | 10 | 12 | 16 |
| 08 | 3 | 5 | 4 | 11 | 14 | 18 |
| 09 | 4 | 4 | 5 | 12 | 15 | 20 |
| 10 | 4 | 5 | 5 | 14 | 17 | 23 |
| 11 | 4 | 6 | 5 | 17 | 21 | 28 |
| 12 | 5 | 4 | 6 | 15 | 18 | 24 |
| 13 | 5 | 5 | 6 | 19 | 23 | 31 |
| 14 | 5 | 6 | 6 | 22 | 27 | 36 |
| 15 | 5 | 6 | 7 | 23 | 28 | 37 |

- Quy tắc sao: 3 sao = `ceil(1.2 × nước tối thiểu)`, 2 sao = `ceil(1.6 × nước tối thiểu)`.
- Layout được sinh ngẫu nhiên có seed cố định (`1000 + levelId`) và được chọn khi số nước tối thiểu nằm trong băng mục tiêu. Mọi level đều có ít nhất một khay trống và mỗi màu đúng bằng sức chứa.
- Level 04–15 dùng đúng 5 màu có trong `ToyColor`. Không thêm màu mới.
- `Level_001`–`Level_003` không bị sửa (xem mục "Vấn đề cần quyết định").
- Các file `.asset` được viết theo đúng định dạng YAML của `Level_002.asset` (cùng `m_Script` GUID, cùng cách mã hóa hex cho `colors`).

Các file đã thay đổi:
- `Assets/_Project/Data/Levels/Level_004.asset` … `Level_015.asset` và `.asset.meta` (mới, 24 file).
- `Assets/_Project/Data/ColorFactoryLevelCatalog.asset`: thêm 12 entry `levelId` 4–15 → `Level_004`–`Level_015`.
- `Assets/AddressableAssetsData/AssetGroups/Local_Levels.asset`: thêm 12 entry Addressables, address trùng tên asset.

## Code

- `Assets/_Project/Scripts/Levels/LevelBatchRules.cs` (mới): luật kiểm tra thuần C#, trả danh sách lỗi. `CheckCatalog` kiểm tra levelId/key trùng và rỗng. `CheckLevel` kiểm tra levelId khớp, sức chứa, star target, màu đủ đúng sức chứa, **có khay trống**, và số khay thừa.
- `Assets/_Project/Editor/LevelBatchValidator.cs` (mới): menu `Color Factory > Validate Level Catalog`. Đọc catalog, tìm LevelData theo tên, gọi `LevelBatchRules`, và kiểm tra Addressables entry có tồn tại và address bằng key. Chỉ đọc asset, không sửa.
- Không sửa `LevelData.ValidateOrThrow` và gameplay runtime.

## Unity Editor — các bước cần làm

1. **Để Unity compile xong.** Console không được có lỗi compile.
2. **Kiểm tra Addressables**: `Window > Asset Management > Addressables > Groups`, chọn group `Local_Levels`. Phải thấy `Level_004` … `Level_015` với Address đúng tên. Các asset này được thêm bằng YAML nên hãy mở group để xác nhận. Nếu Unity không hiển thị, kéo `Level_004`–`Level_015` từ Project vào group `Local_Levels` và đặt Address bằng tên asset.
3. **Chạy batch check**: menu `Color Factory > Validate Level Catalog`.
   - Kết quả mong đợi hiện tại: **FAIL** với đúng một lỗi: `Level_003`: "Level không có khay trống nào." (xem mục dưới).
   - Mọi lỗi khác phải không xuất hiện. Nếu có lỗi về Addressables thì làm bước 2.
4. **Kiểm tra Level Select**: Build Profile giữ nguyên Scene List (`LevelSelect`, `Gameplay`). Chạy Play từ scene `LevelSelect`.
   - Phải thấy 15 button, Level 04 trở đi bị khóa cho đến khi level trước hoàn thành.
   - Dùng Inspector hoặc đoạn tạm để mở khóa (hoặc xóa `PlayerPrefs`), rồi chọn từng level 04–15: mỗi level phải load, board hiển thị đúng số khay và số màu.
5. **Kiểm tra sao**: hoàn thành một level bằng đúng số nước tối thiểu trong bảng trên thì phải được 3 sao. Tăng số nước vượt 3 sao target thì phải còn 2 sao.
6. **Console**: không có `Level` error nào khi load từng level.

## Vấn đề cần quyết định (không tự sửa)

1. **Level_003 không có khay trống.** Cả 5 khay đều có toy. Acceptance của bài yêu cầu mọi level có ít nhất một khay trống. Đề xuất: thêm một khay trống (`trays` thêm `colors: ` rỗng, sau đó chỉnh `Level_003`), hoặc chuyển một khay thành rỗng bằng cách di chuyển toy. Việc này đổi gameplay của Level 03 nên cần người dùng chọn cách.
2. **Level_001 có 3 sao target = 8, nhưng nước tối thiểu là 10 (BFS).** Nghĩa là không thể đạt 3 sao ở Level 01. Level_002 (3 sao = 16, tối thiểu 12) và Level_003 (3 sao = 24, tối thiểu 20) đạt được. Đề xuất: đặt 3 sao của Level_001 thành 12 và 2 sao thành 16, hoặc giữ nguyên nếu đây là chủ ý.

## Kiểm thử

- Đã chạy: BFS trên 15 level (`docs/tools/level_solver.py`) — kết quả như bảng trên.
- Chưa chạy: Edit Mode test. Repo chưa có Edit Mode test assembly và Bài 28 là bài dành cho test suite. Luật trong `LevelBatchRules` sẽ được thêm vào test ở Bài 28.
- Chưa chạy: Validator trong Unity Editor và Level Select trên thiết bị. Các kết quả này phải được ghi lại sau khi chạy.

## Cách tái tạo số liệu

```
python -I docs/tools/level_solver.py Assets/_Project/Data/Levels/Level_0*.asset
```
In ra số nước tối thiểu của từng level. Nếu BFS vượt giới hạn 3.000.000 trạng thái, hàm trả `None`.
