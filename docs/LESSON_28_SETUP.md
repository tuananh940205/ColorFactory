# Bài 28 — Test suite và regression

## Trạng thái

Test suite đã được viết và **đã chạy thật** bằng Unity 6000.3.25f1 ở chế độ batch:

| Bộ test | Kết quả | File evidence |
|---|---|---|
| Edit Mode | **41 / 41 passed**, 0 failed, 0 skipped | `docs/profiling/test_editmode_results.xml` |
| Play Mode | **5 / 5 passed**, 0 failed, 0 skipped | `docs/profiling/test_playmode_results.xml` |

Lưu ý về cách chạy: batch được chạy trên một **bản sao** của project (Assets, Packages, ProjectSettings), vì Editor đang mở project gốc nên không thể chạy batch trên đó. Library của bản sao được import mới nên thời gian chạy dài. Vẫn cần chạy lại trong Test Runner của Editor để xác nhận trên project gốc.

## Cấu trúc test

Asmdef mới:
- `Assets/_Project/Scripts/ColorFactory.Runtime.asmdef`: toàn bộ code runtime của `_Project` (Scripts) giờ nằm trong assembly `ColorFactory.Runtime`, không còn trong `Assembly-CSharp`. Đây là điều kiện để test assembly tham chiếu được code game.
- `Assets/_Project/Tests/EditMode/ColorFactory.Tests.EditMode.asmdef`: chỉ Editor, có `TestAssemblies`.
- `Assets/_Project/Tests/PlayMode/ColorFactory.Tests.PlayMode.asmdef`: runtime test, có `TestAssemblies`.

### Edit Mode (41 test)

| File | Phạm vi | Failure scenario được cover |
|---|---|---|
| `BoardModelTests` | Luật move, Undo, hoàn thành, khởi tạo | Nước đi sai (cùng khay, khay rỗng nguồn, khay đích đầy, lệch màu, index ngoài phạm vi), **undo khi lịch sử rỗng**, dữ liệu khay vượt sức chứa |
| `LevelDataValidationTests` | `LevelData.ValidateOrThrow` | **Malformed level**: lệch số toy/màu, khay vượt sức chứa, không có khay thừa, không có toy, 3 sao > 2 sao, thiếu/khoảng trắng key |
| `LevelBatchRulesTests` | Luật batch check Bài 27 | Catalog trùng id/key, key rỗng, level lỗi nhiều loại |
| `LevelProgressServiceTests` | Mở khóa và PlayerPrefs | **Level bị khóa** (level 2 khi mới chơi), mở khóa đúng 1 level, best result không bị ghi đè, kẹp sao 1–3, không giảm mức mở khóa, **độ bền PlayerPrefs** |
| `LevelResultCalculatorTests` | Ngưỡng sao | Các giá trị biên 8/9, 12/13 |
| `LevelCatalogTests` | Tra cứu key và level kế tiếp | Level không tồn tại, level cuối, key rỗng |
| `LevelSessionTests` | Request level | Request chỉ dùng một lần |
| `PoolServiceTests` | Vòng đời pool | Tái sử dụng instance, pool không tồn tại, **từ chối request khi shutdown** |

Helper: `TestLevelFactory` tạo `LevelData` trong bộ test và hủy sau mỗi test.

### Play Mode (5 test)

`AddressableLifecycleTests`, dùng `AddressableService` và `SceneAssetScope` thật:
- Key có khoảng trắng bị từ chối, không để lại entry (failure scenario).
- `Level_999` (key không tồn tại): test khai báo trước `InvalidOperationException`, log lỗi Addressables được bỏ qua có chủ đích và reset trong TearDown; không để lại handle.
- Hủy trước khi load báo `OperationCanceledException`.
- Load `Level_001` qua `LevelRepository`, `ReleaseAllAsync` nhiều lần vẫn an toàn, scope đã đóng từ chối load mới.
- Hai scope cùng giữ một key: dùng chung một asset, chỉ giải phóng khi cả hai đóng.

## Quyết định kỹ thuật

- **Không dùng `PlayerPrefs.DeleteAll`** trong test. Test chỉ xóa đúng các key tiến độ của level 1–3, để không xóa dữ liệu Editor của bạn.
- **Mọi lần chờ Task có timeout 20s** (`WaitForTask`): Run All ở PlayMode luôn kết thúc, Task treo chỉ làm test fail với thông báo rõ.
- **Exception được khai báo trước** trong từng test theo AGENTS.md ("Write test cases that can predict the exception").
- **Không dùng try-catch** trong test. Task lỗi được kiểm tra qua `Task.IsFaulted` và `Task.Exception.InnerException`, chờ bằng `WaitUntil`.
- **Không có scene load/unload smoke**. Scene `LevelSelect`/`Gameplay` cần bootstrap object (`AddressableService`, `SceneFader`…) từ scene đầu, nên test độc lập sẽ thiếu Services. Phần này được kiểm tra thủ công (bước 4 bên dưới).
- Test PlayMode dùng Addressables thật. Chúng chạy được vì Level_001 đã có trong group `Local_Levels`.

## Sửa lỗi phát sinh trong quá trình chạy

1. `HintBatchBenchmark.cs` (Bài 26): `using System.Diagnostics;` làm `Debug` bị mơ hồ với `UnityEngine.Debug`, khiến cả assembly không compile. Đã bỏ `using` và gọi `System.Diagnostics.Stopwatch` đầy đủ. Chức năng không đổi.
2. `PoolServiceTests`: `PoolService.ClearAll` gọi `Destroy` trong Edit Mode gây lỗi log. Đã đổi TearDown và trả instance về pool cuối test để nó là con của root trước khi hủy.

## Unity Editor — các bước cần làm

1. **Mở Test Runner**: `Window > General > Test Runner`.
2. **Edit Mode**: chọn tab `EditMode` > **Run All**. Kỳ vọng: 41 passed, 0 failed.
3. **Play Mode**: chọn tab `PlayMode` > **Run All**. Kỳ vọng: 5 passed, 0 failed.
   - Nếu test Addressables lỗi "key not found": kiểm tra `Addressables > Play Mode Script` là `Use Asset Database`, hoặc build content (`Addressables > Build > New Build > Default Build Script`).
4. **Kiểm tra thủ công Level Select** (thay cho scene smoke test): chạy scene `LevelSelect` từ Play, xác nhận 15 button, level 1 mở, level 2 khóa; chọn level, hoàn thành, quay lại Level Select và kiểm tra mở khóa/sao còn sau khi dừng Play.
5. **Screenshot kết quả**: sau khi Run All, chụp Test Runner (cả hai tab, có số passed) và lưu vào:
   - `docs/profiling/test_editmode_screenshot.png`
   - `docs/profiling/test_playmode_screenshot.png`
   Đây là bằng chứng portfolio theo AGENTS.md; phần này chưa có vì không chụp được trong batch.
6. **Console**: sau Run All không có lỗi compile. Các cảnh báo Burst từ package `Unity.AI.Sound` là từ bên thứ ba, không liên quan dự án.

## Lưu ý khi thêm script mới

Từ bài này, script trong `Assets/_Project/Scripts/` thuộc assembly `ColorFactory.Runtime`. Nếu thêm package mới (ví dụ `Unity.Burst`, `Unity.Addressables`), cần thêm tên assembly vào `references` của `ColorFactory.Runtime.asmdef`. Script trong `Assets/_Project/Editor/` vẫn tham chiếu được đến `ColorFactory.Runtime`.

## Chưa làm / cần người dùng quyết định

- Screenshot Test Runner (bước 5).
- Scene-level smoke test (load/unload `LevelSelect` và `Gameplay`) — hiện được kiểm tra thủ công.
- `LevelBatchValidator` (menu Bài 27) chưa có test vì nó phụ thuộc AssetDatabase. Luật của nó đã được test qua `LevelBatchRules`.
