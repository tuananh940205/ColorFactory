# Color Factory — Agent Instructions

## Khi người dùng nói “Triển khai bài N”

1. Đọc toàn bộ `docs/PROJECT_CONTEXT.md`, `docs/LESSON_PLAN.md` và `docs/PROGRESS.md`.
2. Tìm đúng heading `Bài N` trong `docs/LESSON_PLAN.md`.
3. Thực hiện trọn vẹn phạm vi bài đó: code, asset nếu được yêu cầu, Unity Editor setup, test và cập nhật progress.
4. Không thực hiện nội dung của bài tiếp theo khi chưa được người dùng yêu cầu rõ ràng.
5. Không nói bài đã pass nếu chưa chạy được validation khả dụng; nêu rõ phần nào cần người dùng kiểm tra trong Unity Editor.
6. Tóm tắt những thay đổi có liên quan trực tiếp đến trò chơi sau khi công việc hoàn tất

## Unity và kiến trúc bắt buộc

- Unity 6.3 LTS, Universal 2D (URP), Android portrait 9:16.
- Mọi method tạo hoặc sửa trong `Assets/` phải có XML comment tiếng Việt giải thích trách nhiệm.
- Giữ gameplay rule trong pure C# (`BoardModel`), Unity views chỉ render/input.
- Runtime asset dùng Addressables string key. Key phải chính xác bằng tên asset hoặc root GameObject.
- `LevelData` chỉ lưu string key cho runtime asset, không lưu direct prefab/sprite reference.
- Chỉ `AddressableService` và `SceneAssetScope` được release Addressables handles.
- Khi rời scene: dừng input/job -> dispose native data -> destroy active/inactive pool instances -> release SceneAssetScope.
- Không release prefab template khi `PoolService` còn instance.
- Không gọi `Addressables.Release` trực tiếp trong gameplay/view/UI code.
- All popups/panels view should be packed by addressable prefab, not placed in scene at initial, shown by Instantiate Async and release when closed
Only entry scene set in scene list. The another scene will be loaded by addressables

## Asset và UI

- Dùng SpriteAtlas cho sprite gameplay và UI đã ổn định.
- Nếu bài yêu cầu asset raster mới, dùng Image Generation, lưu PNG vào `Assets/_Project/Art/`, kiểm tra alpha nếu yêu cầu nền trong suốt, rồi hướng dẫn import chi tiết.
- Không tạo asset Addressable riêng nếu nó chỉ là dependency của một Addressable prefab, trừ khi lesson ghi rõ.
- Mọi hướng dẫn Unity Editor phải nêu: object tạo ở đâu trong Hierarchy/Project, component thêm vào, field Inspector cần gán, giá trị cần đặt và cách test.

## Chất lượng và portfolio

- Dùng Jobs/Burst chỉ cho bài toán có baseline và benchmark trên workload đủ lớn.
- Không bịa số benchmark, profiler hoặc memory result.
- Các file evidence cuối dự án nằm ở `docs/profiling/`.
- Không sửa `Library/`, `Temp/` hay Unity-generated files.

## Coding instruction
Use action/callback when call from this view to that view, remove cross dependencies serialized fields
Use button listener by addlistener/removelistener instead of Button component
Every generated function should write explanation comments by Vietnamese
Game config and level config should be load text asset by json, then serialize to serialiable class
Text asset can be loaded by addressable by key
Data class has readonly field, set data from constructor only
1 file should define only class/object with the same name as file name

Every singleton instance managed by ServiceLocator
Use Signal instead of placing serialized monobehaviour script object to call methods
When need to use new signal, define a new class inherit from the needed signal

Only use try-catch in functions when:
- has CancellationTokenSource

Use one static class object as service locator to store all singletons
Do not use static class or static instance for singleton items