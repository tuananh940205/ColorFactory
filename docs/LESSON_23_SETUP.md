# Bài 23 — Audio feedback

## Code đã thêm

- `Audio/IAudioService`: giao diện phát SFX và trạng thái mute. Gameplay/UI chỉ gọi `Services.Audio?.Play(SfxType.X)`.
- `Audio/AudioService`: MonoBehaviour persistent (`DontDestroyOnLoad`), tạo pool 4 `AudioSource` một lần lúc Awake, load toàn bộ clip bằng `AddressableService` qua `SceneAssetScope` riêng, lưu mute vào PlayerPrefs key `audio.muted`.
- `Audio/SfxKeyMap`: đọc `AudioConfig` (TextAsset JSON) và kiểm tra mọi SFX có key hợp lệ; thiếu key thì báo lỗi ngay khi load.
- `Audio/SfxType`: Tap, Move, Undo, Hint, Win, Button.
- `UI/AudioToggleButton`: nút bật/tắt sound, nhãn tự cập nhật qua `MutedChanged`.
- `Core/Services`: thêm `Audio` để truy cập service qua service locator.

Điểm phát sound (mỗi event chỉ phát đúng một sound):

| Sự kiện | SFX | Nơi phát |
|---|---|---|
| Chọn khay nguồn | Tap | `BoardController.SelectTray` |
| Move hợp lệ (không thắng) | Move | `BoardController.HandleTrayClicked`, lúc bắt đầu animate |
| Move làm level thắng | Win | `BoardController.HandleMoveAnimationCompleted`, sau khi toy đáp; không phát Move |
| Undo | Undo | `BoardController.TryUndoLastMove` |
| Hint tìm được nước đi | Hint | `BoardController.RequestHint` |
| Restart, Next Level, mở level từ Level Select | Button | `GameplayHeaderView`, `LevelCompleteView`, `GameBootstrap.GoToNextLevel`, `LevelSelectController.OpenLevel` |

Các âm thanh được phát sau khi `SceneFader` chấp nhận request, nên request bị chặn lúc đang fade sẽ không kêu.

## Asset đã thêm

- `Assets/_Project/Audio/SFX/Sfx_Tap.wav`, `Sfx_Move.wav`, `Sfx_Undo.wav`, `Sfx_Hint.wav`, `Sfx_Win.wav`, `Sfx_Button.wav`: placeholder tạo bằng sóng sin (không có license bên ngoài). Có thể thay bằng file âm thanh thật, giữ nguyên tên file và Addressables key.
- `Assets/_Project/Data/Audio/AudioConfig.json`: ánh xạ SFX sang key.

## Cấu hình Unity Editor

1. **Addressables key**
   - Mở Addressables Groups. Thêm 6 file trong `Audio/SFX` vào group (ví dụ Default Local Group), đặt Address đúng bằng tên: `Sfx_Tap`, `Sfx_Move`, `Sfx_Undo`, `Sfx_Hint`, `Sfx_Win`, `Sfx_Button`.
   - Thêm `Data/Audio/AudioConfig.json` vào group, Address: `AudioConfig`.
   - Import xong, kiểm tra Inspector của mỗi WAV: Load Type `Decompress On Load` hoặc `Compressed In Memory` đều được; với placeholder ngắn, `Decompress On Load` là đủ.
2. **AudioMixer (không bắt buộc)**
   - Tạo `Assets/_Project/Audio/MainMixer.mixer`, thêm group `SFX`.
   - Chọn object `AudioService` trong cả hai scene, gán `Sfx Mixer Group` là group `SFX`. Bỏ trống nếu chưa cần mixer.
3. **AudioService trong scene**
   - Trong `LevelSelect` và `Gameplay`, tạo một GameObject tên `AudioService` (cùng cấp với object chứa `AddressableService` và `SceneFader`).
   - Add component `AudioService`. Để mặc định: `Audio Config Key = AudioConfig`, `Source Pool Size = 4`.
   - Chỉ cần một object mỗi scene; object thứ hai sẽ tự hủy khi đã có service từ scene trước.
4. **Nút bật/tắt sound**
   - Trong `LevelSelect`: tạo Button (ví dụ góc trên phải) và add `AudioToggleButton`. Kéo Button vào `Button`, TMP_Text của nút vào `Label`.
   - Trong `Gameplay`: làm tương tự trong header. Nếu header đã có nhiều nút, đặt nút này cạnh nút Restart.
   - Kiểm tra `Sound On Text` và `Sound Off Text`; mặc định là `SOUND: ON` và `SOUND: OFF`.
5. **Build Profiles**: đảm bảo `LevelSelect` và `Gameplay` vẫn là hai scene trong Scene List; không cần thêm scene mới.

## Kiểm thử Unity Editor

1. Play từ `LevelSelect`, mở Level 01. Console không được có lỗi `AudioService`, `AddressableService` hay `Addressables failed to load key`.
2. Chạm một khay có toy: nghe Tap. Chạm khay đích hợp lệ: nghe Move đúng một lần.
3. Undo: nghe Undo đúng một lần. Hint: nghe Hint đúng một lần, board không thay đổi.
4. Hoàn thành level: nghe Win đúng một lần, sau khi toy cuối đáp. Không có Move ở nước đi cuối.
5. Bấm Restart, Next Level hoặc mở level từ Level Select: nghe Button đúng một lần.
6. Bấm nút SOUND để tắt: không còn tiếng nào; đổi sang Gameplay, rồi thoát app và mở lại: nút vẫn hiển thị `SOUND: OFF` và không có tiếng.
7. Hierarchy: mỗi scene chỉ có một `AudioService` sau khi chuyển scene (object `DontDestroyOnLoad`), không có AudioSource tăng lên khi đổi scene.

## Ghi chú

- Chưa có Edit Mode test cho `SfxKeyMap.FromJson`. Sẽ thêm trong Bài 28 cùng test suite.
- Đây là code chưa được kiểm chứng trong Unity Editor; cần chạy các bước kiểm thử ở trên.
