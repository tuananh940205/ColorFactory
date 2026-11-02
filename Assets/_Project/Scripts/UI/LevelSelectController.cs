using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ColorFactory.Core;
using ColorFactory.Levels;
using UnityEngine;

namespace ColorFactory.UI
{
    /// <summary>
    /// Load LevelCatalog bằng Addressables và tạo các button level theo progress.
    /// </summary>
    public sealed class LevelSelectController : MonoBehaviour
    {
        [Header("Addressables")]
        [SerializeField] private string levelCatalogKey =
            "ColorFactoryLevelCatalog";

        [SerializeField] private string levelButtonPrefabKey =
            "LevelSelectButton";

        [Header("Scene References")]
        [SerializeField] private Transform buttonContainer;
        [SerializeField] private string gameplaySceneName = "Gameplay";

        private readonly List<GameObject> spawnedButtons = new();

        private AddressableService addressableService;
        private SceneAssetScope assetScope;
        private CancellationTokenSource cancellationSource;

        /// <summary>
        /// Load catalog và tạo button sau khi scene Level Select mở.
        /// </summary>
        private async void Start()
        {
            addressableService = Services.Addressables;

            if (addressableService == null)
            {
                Debug.LogError(
                    "LevelSelectController requires AddressableService.",
                    this);
                return;
            }

            if (buttonContainer == null)
            {
                Debug.LogError(
                    "LevelSelectController requires Button Container.",
                    this);
                return;
            }

            cancellationSource = new CancellationTokenSource();
            assetScope = addressableService.CreateSceneScope(
                gameObject.scene.name);

            try
            {
                LevelCatalog catalog =
                    await assetScope.LoadAsync<LevelCatalog>(
                        levelCatalogKey,
                        cancellationSource.Token);

                GameObject buttonTemplate =
                    await assetScope.LoadAsync<GameObject>(
                        levelButtonPrefabKey,
                        cancellationSource.Token);

                CreateLevelButtons(catalog, buttonTemplate);
            }
            catch (OperationCanceledException)
            {
                // Scene bị đóng khi Addressables đang load là trường hợp bình thường.
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
        }

        /// <summary>
        /// Hủy các button trước rồi mới release template Addressables.
        /// </summary>
        private async void OnDestroy()
        {
            cancellationSource?.Cancel();

            foreach (GameObject buttonObject in spawnedButtons)
            {
                if (buttonObject != null)
                    Destroy(buttonObject);
            }

            spawnedButtons.Clear();

            if (assetScope != null)
                await assetScope.ReleaseAllAsync();

            cancellationSource?.Dispose();
        }

        /// <summary>
        /// Tạo một UI button cho từng entry trong LevelCatalog.
        /// </summary>
        private void CreateLevelButtons(
            LevelCatalog catalog,
            GameObject buttonTemplate)
        {
            foreach (LevelCatalogEntry entry in catalog.Levels)
            {
                if (entry == null || entry.levelId <= 0)
                    continue;

                GameObject buttonObject = Instantiate(
                    buttonTemplate,
                    buttonContainer,
                    false);

                LevelSelectButton levelButton =
                    buttonObject.GetComponent<LevelSelectButton>();

                if (levelButton == null)
                {
                    Debug.LogError(
                        "LevelSelectButton prefab needs LevelSelectButton component.",
                        buttonObject);

                    Destroy(buttonObject);
                    continue;
                }

                bool isUnlocked =
                    LevelProgressService.IsUnlocked(entry.levelId);

                int bestStars =
                    LevelProgressService.GetBestStars(entry.levelId);
                
                int bestMoves =
                    LevelProgressService.GetBestMoves(entry.levelId);

                levelButton.Setup(
                    entry.levelId,
                    isUnlocked,
                    bestStars,
                    bestMoves,
                    OpenLevel);

                spawnedButtons.Add(buttonObject);
            }
        }

        /// <summary>
        /// Lưu level người chơi chọn rồi chuyển sang scene Gameplay.
        /// </summary>
        private void OpenLevel(int levelId)
        {
            Services.SceneFader?.TryLoadScene(
                gameplaySceneName,
                () => LevelSession.RequestLevel(levelId));
        }
    }
}
