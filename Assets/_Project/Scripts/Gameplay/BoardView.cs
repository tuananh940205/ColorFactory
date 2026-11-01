using System.Collections.Generic;
using ColorFactory.Core;
using ColorFactory.Levels;
using UnityEngine;

namespace ColorFactory.Gameplay
{
    public sealed class BoardView : MonoBehaviour
    {
        [SerializeField] private GameBootstrap gameBootstrap;
        [SerializeField] private Transform boardRoot;
        [SerializeField] private float traySpacing = 3f;

        private readonly List<GameObject> spawnedInstances = new();
        private PoolService poolService;
        private bool built;

        private void Start()
        {
            if (gameBootstrap == null)
            {
                Debug.LogError("BoardView requires GameBootstrap.", this);
                return;
            }

            if (gameBootstrap.IsReady)
                Build(gameBootstrap);
            else
                gameBootstrap.Ready += Build;
        }

        private void OnDestroy()
        {
            if (gameBootstrap != null)
                gameBootstrap.Ready -= Build;

            Clear();
        }

        private void Build(GameBootstrap bootstrap)
        {
            if (built)
                return;

            if (boardRoot == null)
                boardRoot = transform;

            poolService = FindFirstObjectByType<PoolService>();

            if (poolService == null)
            {
                Debug.LogError("BoardView could not find PoolService.", this);
                return;
            }

            LevelData level = bootstrap.CurrentLevel;
            if (level == null)
            {
                Debug.LogError("GameBootstrap has no loaded LevelData.", this);
                return;
            }

            float firstTrayX =
                -((level.trays.Count - 1) * traySpacing) * 0.5f;

            for (int trayIndex = 0; trayIndex < level.trays.Count; trayIndex++)
            {
                GameObject trayObject =
                    poolService.Get(level.assets.trayPrefabKey);

                spawnedInstances.Add(trayObject);

                trayObject.transform.SetParent(boardRoot, false);
                trayObject.transform.localPosition =
                    new Vector3(firstTrayX + trayIndex * traySpacing, 0f, 0f);

                TrayView trayView =
                    trayObject.GetComponent<TrayView>();

                if (trayView == null)
                {
                    Debug.LogError(
                        "TrayPrefab requires a TrayView component on its root.",
                        trayObject);
                    continue;
                }

                trayView.Setup(trayIndex);

                TrayData trayData = level.trays[trayIndex];

                for (int toyIndex = 0; toyIndex < trayData.colors.Count; toyIndex++)
                {
                    GameObject toyObject =
                        poolService.Get(level.assets.toyPrefabKey);

                    spawnedInstances.Add(toyObject);

                    toyObject.transform.SetParent(trayView.ToyContainer, false);
                    toyObject.transform.localPosition =
                        trayView.GetToyLocalPosition(toyIndex);

                    ToyView toyView = toyObject.GetComponent<ToyView>();

                    if (toyView == null)
                    {
                        Debug.LogError(
                            "ToyPrefab requires a ToyView component on its root.",
                            toyObject);
                        continue;
                    }

                    toyView.SetColor(trayData.colors[toyIndex]);
                }
            }

            built = true;
        }

        private void Clear()
        {
            if (poolService == null || poolService.IsShuttingDown)
                return;

            for (int index = spawnedInstances.Count - 1; index >= 0; index--)
                poolService.Return(spawnedInstances[index]);

            spawnedInstances.Clear();
            built = false;
        }
    }
}