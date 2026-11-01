using System;
using System.Threading;
using System.Threading.Tasks;
using ColorFactory.Levels;
using UnityEngine;
using UnityEngine.U2D;

namespace ColorFactory.Core
{
    public sealed class GameBootstrap : MonoBehaviour
    {
        [Header("Services")]
        [SerializeField] private AddressableService addressableService;
        [SerializeField] private PoolService poolService;

        [Header("Level")]
        [Tooltip("Exact Addressables address and LevelData asset name.")]
        [SerializeField] private string levelKey = "Level_001";
        [SerializeField, Min(0)] private int initialTrayPoolSize = 6;
        [SerializeField, Min(0)] private int initialToyPoolSize = 24;

        [Header("Optional Visuals")]
        [SerializeField] private SpriteRenderer backgroundRenderer;

        private readonly LevelRepository levelRepository = new();

        private CancellationTokenSource lifetimeCancellation;
        private SceneAssetScope assetScope;
        private Task runTask;
        private Task cleanupTask;

        public LevelData CurrentLevel { get; private set; }
        public bool IsReady { get; private set; }
        public event Action<GameBootstrap> Ready;

        private async void Start()
        {
            if (addressableService == null)
                addressableService = AddressableService.Instance;

            if (addressableService == null)
            {
                Debug.LogError("GameBootstrap requires AddressableService.", this);
                return;
            }

            if (poolService == null)
            {
                Debug.LogError("GameBootstrap requires PoolService.", this);
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
                // Scene is leaving; cleanup follows in OnDestroy.
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
                await CleanupAsync();
            }
        }

        private async Task InitializeAsync(CancellationToken cancellationToken)
        {
            assetScope = addressableService.CreateSceneScope(gameObject.scene.name);

            CurrentLevel = await levelRepository.LoadAsync(
                levelKey,
                assetScope,
                cancellationToken);

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

            Debug.Log(
                $"Color Factory ready: Level {CurrentLevel.levelId}, " +
                $"Addressable keys held: {assetScope.LeaseCount}.",
                this);

            // Gameplay controller can now request:
            // poolService.Get(CurrentLevel.assets.trayPrefabKey)
            // poolService.Get(CurrentLevel.assets.toyPrefabKey)
        }

        private async void OnDestroy()
        {
            if (lifetimeCancellation != null)
                lifetimeCancellation.Cancel();

            if (runTask != null)
            {
                try
                {
                    await runTask;
                }
                catch
                {
                    // Initialization errors were already handled or cancellation is expected.
                }
            }

            await CleanupAsync();
        }

        private Task CleanupAsync()
        {
            if (cleanupTask != null)
                return cleanupTask;

            cleanupTask = CleanupInternalAsync();
            return cleanupTask;
        }

        private async Task CleanupInternalAsync()
        {
            IsReady = false;

            // Required ordering: instances first, Addressables templates second.
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
            CurrentLevel = null;
        }
    }
}