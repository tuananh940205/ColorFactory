# Color Factory — Remaining Lesson Plan

## Operating rule

For every lesson, AI Agent must deliver: complete code changes, required generated/edited assets, exact Unity Editor steps, tests, expected Console result, and `PROGRESS.md` update. It must preserve existing Addressables/pool ownership architecture.
After operation completed:
- Describe what you change in scene/prefab/gameobject steps by steps in existing LESSION_$IINDEX_SETUP.md file

## Bài 20 — Scene fade và polish luồng chuyển scene

**Goal:** chuyển giữa Level Select và Gameplay có fade-out/fade-in, không nháy frame hoặc để UI cũ lộ ra.

**Code:** tạo persistent `SceneFader` dùng Canvas, CanvasGroup và async scene load. Thay các `SceneManager.LoadScene` trong Level Select, restart, next level bằng API fader. Không thay đổi `LevelSession`.

**Asset:** không bắt buộc; dùng Image UI màu xanh navy.

**Unity Editor:** tạo `SceneFader` prefab hoặc bootstrap object trong cả hai scene; hướng dẫn Canvas sorting order, Image stretch và CanvasGroup reference.

**Acceptance:** Level 01 mở từ Level Select, restart và next-level có fade mượt; request scene lặp khi đang fade bị chặn; Console không lỗi.

## Bài 21 — SpriteAtlas chính thức

**Goal:** đóng gói sprite gameplay/UI vào atlas, kiểm tra prefab dependency và Addressables load path.

**Code:** chỉ thêm code nếu cần diagnostics; không thay sprite runtime bằng direct Addressables load riêng.

**Asset:** tạo `GameplayAtlas` gồm tray/toy/background-adjacent sprites phù hợp; tạo `UIAtlas` nếu UI sprite cần thiết.

**Unity Editor:** tạo SpriteAtlas, thêm folder/object, cấu hình Include in Build, platform override Android, packing, Addressable key nếu atlas được LevelData yêu cầu.

**Acceptance:** sprite không mất reference, build Addressables thành công, gameplay and Level Select render giống trước.

## Bài 22 — Feedback khi di chuyển và hoàn thành

**Goal:** hoàn thiện move/undo animation, visual pulse, particle completion nhẹ và chặn input khi animation đang chạy.

**Code:** giữ mapping toy instance theo tray; animate move/undo đối xứng; thêm reusable feedback view không tạo GC mỗi nước đi.

**Asset:** tạo particle sparkle nhỏ nền trong suốt chỉ nếu Unity built-in particle không đủ.

**Unity Editor:** cấu hình ParticleSystem hoặc prefab effect, sorting layer/order, pooled use và auto-return.

**Acceptance:** move/undo mượt, không double-input, completion chỉ hiện sau khi toy đáp, particle được cleanup trước scope release.

## Bài 23 — Audio feedback

**Goal:** thêm tap, move, undo, hint, win và button sound qua một AudioService đơn giản.

**Code:** interface audio, Addressable AudioClip load by string, small SFX pool/source reuse, mute persistence PlayerPrefs.

**Asset:** dùng asset audio người dùng cung cấp hoặc tạo placeholder hợp pháp; không nhúng asset không rõ license.

**Unity Editor:** AudioMixer, volume group, AudioSource setup, Addressable group/key và test mute.

**Acceptance:** mỗi event phát đúng một sound; mute tồn tại sau restart; không có AudioSource leak khi đổi scene.

## Bài 24 — Economy cho Hint và Undo

**Goal:** Hint/Undo có coin cost mock, reward khi thắng và UI hiển thị coin.

**Code:** `WalletService`, transaction result, PlayerPrefs persistence, không cho âm coin; Hint/Undo chỉ tiêu coin sau action thành công.

**Asset:** icon coin nếu cần, PNG nền trong suốt.

**Unity Editor:** thêm header coin text, cost label/button state và completion reward display.

**Acceptance:** số coin đúng sau restart, insufficient coin không thực thi booster, result UI hiển thị reward.

## Bài 25 — Analytics mock và gameplay events

**Goal:** ghi event local cho level start, move, undo, hint, win, restart, level select và booster failure.

**Code:** `IAnalyticsService`, mock implementation, strongly typed event payload và development Console logging tắt được.

**Asset:** không cần.

**Unity Editor:** gán service bootstrap/reference nếu cần.

**Acceptance:** event không ảnh hưởng gameplay, không log dữ liệu nhạy cảm và có test/mock verification.

## Bài 26 — Hint solver Jobs/Burst benchmark

**Goal:** giữ managed baseline và tạo Jobs/Burst version cho batch hint scoring trên workload synthetic lớn.

**Code:** NativeArray lifecycle, IJobParallelFor, Burst attributes, cancellation/completion before scene cleanup, benchmark harness.

**Asset:** không cần.

**Unity Editor:** bật Burst package/menu nếu cần; chỉ dẫn Development Build benchmark Android.

**Acceptance:** baseline và job trả hint hợp lệ; NativeContainer luôn Dispose; `docs/profiling/benchmark.csv` có số đo thật, không phải ước lượng.

## Bài 27 — Level 04–15 và cân bằng

**Goal:** tạo 12 LevelData còn lại, validation pass, star targets hợp lý và catalog hoàn chỉnh.

**Code:** có thể thêm Editor validator/batch check, không tạo level runtime bằng code.

**Asset:** không bắt buộc.

**Unity Editor:** duplicate/create LevelData, Addressable name, catalog entries, level authoring checklist.

**Acceptance:** tất cả 15 level load qua Level Select, không duplicate address, mỗi level có ít nhất một khay trống và color count hợp lệ.

## Bài 28 — Test suite và regression

**Goal:** Edit Mode tests cho BoardModel, validation, Undo, result calculation, progression persistence; Play Mode smoke tests cho load/unload.

**Code:** NUnit test files trong `Assets/_Project/Tests/EditMode` và thích hợp `PlayMode`; reset PlayerPrefs in test safely.

**Asset:** không cần.

**Unity Editor:** Test Runner setup, cách chạy và lưu screenshot kết quả.

**Acceptance:** suite pass; ít nhất các failure scenario được test: invalid move, undo empty, malformed level, locked level và progress persistence.

## Bài 29 — Profiling và Addressables lifecycle evidence

**Goal:** thu thập evidence thật cho portfolio.

**Code:** thêm diagnostic counters chỉ development build nếu cần; không thêm fake metrics.

**Asset:** screenshot Unity Profiler và Memory Profiler do người dùng chụp từ lần chạy thật.

**Unity Editor:** hướng dẫn Profiler, Memory Profiler, Addressables Event Viewer, test 10 vòng Level Select -> Gameplay -> Level Select.

**Acceptance:** tạo `docs/profiling/profiler_gameplay.png`, `memory_report.md`, `addressables_lifecycle.md`, `benchmark.csv` và ghi rõ thiết bị/build/workload.

## Bài 30 — Android release candidate

**Goal:** tạo APK/AAB portfolio-ready và handoff documentation.

**Code:** chỉ sửa build configuration cần thiết; không thêm SDK monetization thật nếu không được yêu cầu.

**Asset:** icon/splash image nếu còn thiếu.

**Unity Editor:** Android Build Profile, IL2CPP, ARM64, package name, keystore policy, Development/Release differences và build output location.

**Acceptance:** Android build cài được, chơi Level 01–03/15, mute/progress hoạt động, docs và README handoff hoàn chỉnh.
