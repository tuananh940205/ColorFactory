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
- [ ] Verify current move animation and Undo animation after the corresponding lessons are merged.

## Upcoming

- [ ] Bài 23 through Bài 30 in `LESSON_PLAN.md`.

## Manual regression checklist after every gameplay lesson

- [ ] No Console errors.
- [ ] Level 01 loads from Level Select.
- [ ] Valid move changes MoveCount once; invalid move does not.
- [ ] Undo restores board and MoveCount.
- [ ] Hint does not modify board.
- [ ] Complete level unlocks the next level and result persists after restart.
- [ ] Leaving/re-entering Gameplay does not retain stale pooled objects.
