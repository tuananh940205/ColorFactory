# Color Factory — Progress

## Completed or in review

- [x] Unity 6.3 LTS project foundation and Android portrait configuration.
- [x] BoardModel rule, move, completion and data validation.
- [x] Addressables service, scene scope, pool lifecycle and LevelRepository.
- [x] Tray/toy prefabs, generated background, generated tray and generated tintable toy art.
- [x] Gameplay board, selection, movement, header, completion card, undo and hint baseline.
- [x] Level 01–03, catalog, PlayerPrefs unlock progression, star targets and best-result persistence.
- [x] Level Select scene and dynamic Addressable level buttons.
- [x] Bài 20 — Scene fade persistent cho Level Select, restart và next level; chặn request khi fade.
- [x] Bài 21 — Gameplay SpriteAtlas và Addressables key `GameplayAtlas`.
- [x] Bài 22 — Pulse toy/khay, sparkle built-in tái sử dụng và khóa input trong lúc animate.
- [x] Bài 24 — Wallet coin mock cho Hint/Undo, reward hoàn thành và HUD/cost label.
- [ ] Verify current move animation and Undo animation after the corresponding lessons are merged.
- [ ] Bài 23 — Audio feedback: code `AudioService`/`SfxKeyMap`/`AudioToggleButton`, placeholder SFX và `LESSON_23_SETUP.md` đã thêm; chờ kiểm thử trong Unity Editor.

## Upcoming

- [ ] Bài 25 — Analytics mock và gameplay events: `IAnalyticsService`, `LocalAnalyticsService`, 8 event kiểu hóa và `LESSON_25_SETUP.md` đã thêm; chờ kiểm thử trong Unity Editor.
- [ ] Bài 26 — Hint solver Jobs/Burst benchmark: `ManagedHintBatchScorer`, `HintScoreJob`, `NativeHintBatch`, harness và `LESSON_26_SETUP.md` đã thêm; **chưa có số đo**, cần chạy Development Build trên Android để tạo `docs/profiling/benchmark.csv`.
- [ ] Bài 27 — Level 04–15 và cân bằng: 12 LevelData, catalog 15 level, `LevelBatchRules` + menu `Color Factory > Validate Level Catalog`, `LESSON_27_SETUP.md` đã thêm. **Chờ Unity validate; còn 2 vấn đề cần quyết định:** Level_003 không có khay trống, Level_001 có 3 sao (8) thấp hơn nước tối thiểu (10).
- [x] Bài 28 — Test suite: 41 Edit Mode + 5 Play Mode test đã chạy thật (batch, Unity 6000.3.25f1) đều pass; evidence trong `docs/profiling/test_*_results.xml`; `LESSON_28_SETUP.md` đã thêm. Còn screenshot Test Runner và smoke scene thủ công.
- [ ] Bài 29 — Profiling và Addressables lifecycle evidence: đã thêm `PoolService.ActiveCount/InactiveCount` + test, khung `memory_report.md` và `addressables_lifecycle.md` (toàn bộ **TBD**), `LESSON_29_SETUP.md`. **Chưa có evidence thật:** profiler screenshot, Memory Profiler snapshot, Event Viewer, 10 vòng thủ công, `benchmark.csv`.
- [ ] Bài 30 — Android release candidate: đã thêm `AndroidReleaseBuild` (menu + command line), `LESSON_30_SETUP.md`, `README.md`. **Build APK thử thất bại** do clip voice mẫu của `com.unity.ai.assistant` không nén được cho Android; cần quyết định gỡ package hoặc loại voice mẫu. Chưa có APK; chưa test trên thiết bị.

## Manual regression checklist after every gameplay lesson

- [ ] No Console errors.
- [ ] Level 01 loads from Level Select.
- [ ] Valid move changes MoveCount once; invalid move does not.
- [ ] Undo restores board and MoveCount.
- [ ] Hint does not modify board.
- [ ] Complete level unlocks the next level and result persists after restart.
- [ ] Leaving/re-entering Gameplay does not retain stale pooled objects.
