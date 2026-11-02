using ColorFactory.Audio;
using System;
using System.Threading;
using System.Threading.Tasks;
using ColorFactory.Gameplay;
using ColorFactory.Levels;
using UnityEngine;
using UnityEngine.U2D;

namespace ColorFactory.Core
{
    /// <summary>
    /// Điều phối vòng đời gameplay của scene:
    /// load Addressables, tạo pool, tạo BoardModel và cleanup khi rời scene.
    /// </summary>
    public sealed class GameBootstrap : MonoBehaviour
    {
        [Header("Services")]
        [SerializeField] private AddressableService addressableService;
        [SerializeField] private PoolService poolService;

        [Header("Level Selection")]
        [SerializeField] private string levelCatalogKey = "ColorFactoryLevelCatalog";
        [SerializeField, Min(1)] private int fallbackLevelId = 1;

        private LevelCatalog levelCatalog;
        [SerializeField, Min(0)] private int initialTrayPoolSize = 6;
        [SerializeField, Min(0)] private int initialToyPoolSize = 24;

        [Header("Optional Visuals")]
        [SerializeField] private SpriteRenderer backgroundRenderer;

        private readonly LevelRepository levelRepository = new();

        // Token dùng để hủy các load đang chờ khi scene bị hủy.
        private CancellationTokenSource lifetimeCancellation;
        private SceneAssetScope assetScope;
        private Task runTask;
        private Task cleanupTask;

        public LevelData CurrentLevel { get; private set; }
        public BoardModel Board { get; private set; }
        public bool IsReady { get; private set; }

        /// <summary>
        /// Được gọi khi toàn bộ data, prefab template và pool đã sẵn sàng.
        /// </summary>
        public event Action<GameBootstrap> Ready;

        /// <summary>
        /// Khởi tạo một lần khi scene bắt đầu.
        /// </summary>
        private async void Start()
        {
            if (addressableService == null)
                addressableService = Services.Addressables;

            if (addressableService == null || poolService == null)
            {
                Debug.LogError(
                    "GameBootstrap requires AddressableService and PoolService.",
                    this);
                return;
            }

            lifetimeCancellation = new CancellationTokenSource();
            runTask = InitializeAsync(lifetimeCancellation.Token);

            try
            {
                await runTask;
            }
            catch (OperationCanceledException)
            {
                // Đây là trường hợp bình thường khi scene bị đóng lúc đang load.
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
                await CleanupAsync();
            }
        }

        /// <summary>
        /// Load level, tạo model thuần C#, load prefab template và tạo pool.
        /// </summary>
        private async Task InitializeAsync(CancellationToken cancellationToken)
        {
            assetScope = addressableService.CreateSceneScope(gameObject.scene.name);

            // Load catalog trước để chuyển level bằng id thay vì hard-code key.
            levelCatalog = await assetScope.LoadAsync<LevelCatalog>(
                levelCatalogKey,
                cancellationToken);

            int requestedLevelId =
                LevelSession.ConsumeRequestedLevelOrDefault(fallbackLevelId);

            // Không cho load level bị khóa bằng request cũ hoặc thao tác ngoài ý muốn.
            if (!LevelProgressService.IsUnlocked(requestedLevelId))
                requestedLevelId = fallbackLevelId;

            if (!levelCatalog.TryGetLevelKey(requestedLevelId, out string levelKey))
            {
                throw new InvalidOperationException(
                    $"Level {requestedLevelId} is missing from LevelCatalog.");
            }

            CurrentLevel = await levelRepository.LoadAsync(
                levelKey,
                assetScope,
                cancellationToken);

            // BoardModel là nguồn dữ liệu gameplay duy nhất.
            Board = new BoardModel(CurrentLevel);

            GameObject trayTemplate = await assetScope.LoadAsync<GameObject>(
                CurrentLevel.assets.trayPrefabKey,
                cancellationToken);

            GameObject toyTemplate = await assetScope.LoadAsync<GameObject>(
                CurrentLevel.assets.toyPrefabKey,
                cancellationToken);

            if (!string.IsNullOrWhiteSpace(CurrentLevel.assets.backgroundSpriteKey) &&
                backgroundRenderer != null)
            {
                backgroundRenderer.sprite = await assetScope.LoadAsync<Sprite>(
                    CurrentLevel.assets.backgroundSpriteKey,
                    cancellationToken);
            }

            if (!string.IsNullOrWhiteSpace(
                    CurrentLevel.assets.gameplaySpriteAtlasKey))
            {
                await assetScope.LoadAsync<SpriteAtlas>(
                    CurrentLevel.assets.gameplaySpriteAtlasKey,
                    cancellationToken);
            }

            poolService.CreatePool(
                CurrentLevel.assets.trayPrefabKey,
                trayTemplate,
                initialTrayPoolSize);

            poolService.CreatePool(
                CurrentLevel.assets.toyPrefabKey,
                toyTemplate,
                initialToyPoolSize);

            IsReady = true;
            Ready?.Invoke(this);
        }

        /// <summary>
        /// Hủy load đang chạy và dọn pool/scope khi GameObject bị destroy.
        /// </summary>
        private async void OnDestroy()
        {
            lifetimeCancellation?.Cancel();

            if (runTask != null)
            {
                try
                {
                    await runTask;
                }
                catch
                {
                    // Lỗi load đã được xử lý tại Start.
                }
            }

            await CleanupAsync();
        }

        /// <summary>
        /// Đảm bảo cleanup chỉ chạy một lần, kể cả khi có lỗi và scene đóng cùng lúc.
        /// </summary>
        private Task CleanupAsync()
        {
            cleanupTask ??= CleanupInternalAsync();
            return cleanupTask;
        }

        /// <summary>
        /// Bắt buộc clear pool trước rồi mới release Addressables scope.
        /// </summary>
        private async Task CleanupInternalAsync()
        {
            IsReady = false;

            if (poolService != null)
            {
                poolService.BeginShutdown();
                poolService.ClearAll();
            }

            if (assetScope != null)
            {
                await assetScope.ReleaseAllAsync();
                assetScope = null;
            }

            lifetimeCancellation?.Dispose();
            lifetimeCancellation = null;
            Board = null;
            CurrentLevel = null;
        }
        
        /// <summary>
        /// Kiểm tra catalog có level kế tiếp hợp lệ và đã được mở khóa hay không.
        /// </summary>
        public bool CanGoToNextLevel()
        {
            return CurrentLevel != null &&
                   levelCatalog != null &&
                   levelCatalog.TryGetNextLevelId(
                       CurrentLevel.levelId,
                       out int nextLevelId) &&
                   LevelProgressService.IsUnlocked(nextLevelId);
        }

        /// <summary>
        /// Yêu cầu load level kế tiếp rồi tải lại scene Gameplay.
        /// </summary>
        public void GoToNextLevel()
        {
            if (!CanGoToNextLevel())
                return;

            levelCatalog.TryGetNextLevelId(
                CurrentLevel.levelId,
                out int nextLevelId);

            Services.SceneFader?.TryLoadScene(
                gameObject.scene.name,
                () =>
                {
                    Services.Audio?.Play(SfxType.Button);
                    LevelSession.RequestLevel(nextLevelId);
                });
        }
    }
}
