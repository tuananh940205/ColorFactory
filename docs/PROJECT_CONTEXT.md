# Color Factory — Project Context

## Sản phẩm

Color Factory là mobile toy-sorting puzzle Android portrait. Người chơi chọn khay nguồn, chọn khay đích hợp lệ, sắp màu để mỗi khay đầy chỉ chứa một màu.

## Gameplay hiện có

- `BoardModel`: rule move, completion và undo history.
- `BoardController`: input, MoveCount, completion, undo/hint request.
- `BoardView`, `TrayView`, `ToyView`: render pooled tray/toy.
- `AddressableService`, `SceneAssetScope`, `PoolService`: ownership lifecycle.
- `LevelData`, `LevelRepository`, `LevelCatalog`, `LevelSession`, `LevelProgressService`.
- `GameBootstrap`: load catalog, LevelData, templates, pool và BoardModel.
- `GameplayHeaderView`, `LevelCompleteView`, `LevelSelectController`, `LevelSelectButton`.

## Asset convention

```text
TrayPrefab                 Addressable prefab root name and key
ToyPrefab                  Addressable prefab root name and key
Level_001                  Addressable LevelData key
Level_002                  Addressable LevelData key
Level_003                  Addressable LevelData key
ColorFactoryLevelCatalog   Addressable LevelCatalog key
LevelSelectButton          Addressable UI prefab root key
FactoryBackground          Background Sprite key
```

## Current content

- Level 01–03 exist in catalog.
- Level completion unlocks next level and persists stars/best move count with PlayerPrefs.
- Level Select shows lock state, stars and best moves after its UI fields are configured.
- Generated art currently includes factory background, tray sprite and neutral tintable toy sprite.

## Important current limitations

- Move animation is being added; Undo must animate symmetrically after move animation is verified.
- Hint baseline chooses a legal high-score move. Jobs/Burst version is deliberately deferred to lesson 26.
- Economy, sound, particles, analytics mock, complete level set, profiling and Android build are not complete.

## Scenes

```text
Assets/_Project/Scenes/LevelSelect.unity
Assets/_Project/Scenes/GamePlay.unity
```

`LevelSelect` should be first in Build Profiles Scene List, then `GamePlay`.
