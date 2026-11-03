# Bài 30 — Android release candidate

## Trạng thái

- **Đã làm**: Editor script build APK (`Assets/_Project/Editor/AndroidReleaseBuild.cs`), tài liệu bàn giao này, và `README.md` ở thư mục gốc.
- **Build APK thử (batch mode, bản sao project): THẤT BẠI.** Chi tiết và nguyên nhân ở mục "Kết quả build".
- **Chưa kiểm chứng**: cài APK lên máy Android thật, chơi Level 01–03, mute và progress. Đây là acceptance của bài và cần người dùng làm trên thiết bị.

## Cấu hình hiện tại (đã kiểm tra trong ProjectSettings)

| Mục | Giá trị | Ghi chú |
|---|---|---|
| Unity | 6000.3.25f1 | |
| Package name | `com.portfolio.colorfactory` | |
| Scripting backend | IL2CPP | |
| Target architecture | ARM64 | |
| Minimum API | 25 | |
| Bundle version | 1.0 | Chưa bump; xem "Versioning" |
| Scene list | `LevelSelect`, `GamePlay` | `SceneBenchmark` không có trong list (đúng theo Bài 26) |
| Keystore | chưa cấu hình | Unity dùng debug keystore; xem "Keystore policy" |
| Icon/splash | mặc định của Unity | Chưa có asset riêng; xem "Icon" |

## Development và Release

| | Development | Release candidate |
|---|---|---|
| Menu | `Color Factory > Build > Android APK (Development)` | `Color Factory > Build > Android APK (Release)` |
| Development Build | Bật | Tắt |
| Autoconnect Profiler | Bật | Tắt |
| Script Debugging | Bật | Tắt |
| Keystore | Debug keystore | Keystore phát hành (xem dưới) |
| Dùng để | Đo Profiler, test nội bộ | Gửi cho người chơi thử, portfolio |

## Keystore policy

- **Không commit keystore** và mật khẩu vào repo. `Builds/` và `*.keystore` cần nằm ngoài repo.
- Tạo keystore phát hành một lần: `Edit > Project Settings > Player > Android > Publishing Settings > Keystore Manager > Create New`. Lưu file ở nơi ngoài repo, ví dụ `D:\Keystores\ColorFactory\release.keystore`.
- Ghi mật khẩu store, mật khẩu key và alias vào trình quản lý mật khẩu. Mất keystore nghĩa là không cập nhật được app đã cài.
- Sau khi tạo, chọn **Use Custom Keystore** trong Publishing Settings. Nếu chỉ build để thử thì có thể dùng debug keystore.

## Versioning

- Trước khi làm release candidate, tăng `Bundle Version` (ví dụ `1.0.0-rc1`) và `Bundle Version Code` trong Player Settings. Tên APK lấy từ Bundle Version.
- Bài này **chưa đổi** version để không thay đổi cấu hình ngoài yêu cầu. Quyết định bump version thuộc về người phát hành.

## Icon

- Đang dùng icon mặc định của Unity. Nếu cần icon riêng cho portfolio: đặt ảnh vuông vào Assets (ví dụ `Assets/_Project/Art/AppIcon.png`), rồi trong `Player > Icon` kéo vào các slot.
- Không tạo icon trong bài này vì không có yêu cầu asset raster mới.

## Build APK

1. Mở project trong Editor, kiểm tra `File > Build Profiles > Android`: Scene List có `LevelSelect` và `GamePlay`, Platform là Android.
2. Chạy menu build tương ứng ở bảng trên.
3. Console phải in: `[AndroidReleaseBuild] PASS: <đường dẫn APK> (... MB, 2 scene, ...)`.
4. Nếu build lỗi, Console in `FAIL`. Không có APK mới trong thư mục đích.

### Vị trí output

```
<ColorFactory>/Builds/Android/ColorFactory-<BundleVersion>-dev.apk
<ColorFactory>/Builds/Android/ColorFactory-<BundleVersion>-release.apk
```
`Builds/` nằm trong `.gitignore`, nên APK không được commit.

### Chạy từ command line

```
Unity.exe -batchmode -nographics -quit -projectPath <ColorFactory> -buildTarget Android -executeMethod ColorFactory.EditorTools.AndroidReleaseBuild.BuildReleaseFromCommandLine -logFile build_release.log
```
Mã thoát: 0 nếu thành công, 1 nếu lỗi. Lưu ý: chạy command line với project đang mở trong Editor sẽ bị khóa; hãy đóng Editor trước.

## Kết quả build

Mục này được điền từ lần build thật. Không điền số ước lượng.

| Mục | Kết quả |
|---|---|
| Build Development APK (batch, bản sao project) | **FAIL**: `BuildFailedException: Failed to build Addressables content` |
| Kích thước APK | Không có (không tạo được APK) |
| Build Release APK | Chưa chạy |

**Nguyên nhân (từ log build):** trong bước build nội dung Addressables, Unity không nén được clip `Packages/com.unity.ai.assistant/Modules/Unity.AI.Sound/Sounds/VoiceSamples/Liam.wav` cho Android. Log báo `FSBTool ERROR: Internal error from FMOD sub-system` (42 lần) và `Errors during import of AudioClip`.

- Clip này thuộc package `com.unity.ai.assistant` (22 file voice mẫu), không phải asset của `_Project`. Không có file nào trong `Assets/` hoặc `ProjectSettings/` tham chiếu tới nó.
- 6 file SFX của dự án (`Assets/_Project/Audio/SFX/*.wav`) là PCM 16-bit mono 22050 Hz, không phải nguyên nhân.
- Chưa xác định được vì sao Addressables đưa clip của package vào content build.

**Cách xử lý đề xuất (cần quyết định):**
1. Gỡ `com.unity.ai.assistant` khỏi `Packages/manifest.json` nếu game không dùng (công cụ AI của Editor, không có `using Unity.AI` trong `_Project`). Đây là lựa chọn đơn giản nhất.
2. Giữ package nhưng loại các voice mẫu khỏi build. Cần tìm đúng group/nguồn đưa chúng vào Addressables trước.

Sau khi xử lý, chạy lại `Color Factory > Build > Android APK (Development)`.

## Test trên thiết bị (người dùng thực hiện)

1. Cài APK: `adb install -r Builds/Android/ColorFactory-<ver>-dev.apk`. Hoặc copy file APK sang máy và cài (cần bật cài từ nguồn không rõ).
2. Mở app: phải vào được Level Select, thấy 15 button, Level 01–03 mở đúng theo progress.
3. Chơi Level 01 đến khi hoàn thành: sao và số nước được lưu. Đóng app hoàn toàn, mở lại: progress vẫn còn.
4. Bật/tắt mute bằng nút audio: trạng thái đúng, và còn sau khi mở lại app.
5. Vào Gameplay, quay lại Level Select nhiều lần: không crash, không dừng ở màn hình trắng.
6. Kiểm tra Level 02 và 03: chơi được, hoàn thành mở khóa level kế tiếp.

Ghi lại kết quả từng bước vào `README.md` mục "Trạng thái" (đạt / không đạt, máy, phiên bản Android).

## Vấn đề đã biết trước khi phát hành

- **Level_001: 3 sao = 8 trong khi nước tối thiểu là 10** (BFS). Không ai đạt 3 sao ở level 1. Level_001 không có star target trong asset nên đang dùng giá trị mặc định 8/12. Cần quyết định trước khi phát hành.
- Level 04–15 đã được kiểm tra bằng BFS, nhưng Bài 30 chỉ yêu cầu Level 01–03 trên thiết bị.
- Số liệu profiling và benchmark của Bài 26 và Bài 29 vẫn là TBD.

## Unity Editor — checklist trước khi build

- [ ] Console không có lỗi compile.
- [ ] `Color Factory > Validate Level Catalog` trả PASS (hoặc chỉ còn lỗi đã biết ở trên).
- [ ] Edit Mode và Play Mode test pass (xem `docs/profiling/test_*_results.xml`).
- [ ] Scene List đúng thứ tự.
- [ ] Keystore và Bundle Version đã được chọn cho bản phát hành.
