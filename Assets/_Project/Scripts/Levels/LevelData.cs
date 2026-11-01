using System;
using System.Collections.Generic;
using ColorFactory.Core;
using UnityEngine;

namespace ColorFactory.Levels
{
    [Serializable]
    public sealed class TrayData
    {
        // Bottom to top.
        public List<ToyColor> colors = new();
    }

    [Serializable]
    public sealed class LevelAssetConfig
    {
        [Tooltip("Address equals the root GameObject name of the tray prefab.")]
        public string trayPrefabKey;

        [Tooltip("Address equals the root GameObject name of the toy prefab.")]
        public string toyPrefabKey;

        [Tooltip("Optional. Address equals the Sprite asset name.")]
        public string backgroundSpriteKey;

        [Tooltip("Optional. Address equals the SpriteAtlas asset name.")]
        public string gameplaySpriteAtlasKey;
    }

    [CreateAssetMenu(
        fileName = "Level_001",
        menuName = "Color Factory/Level Data")]
    public sealed class LevelData : ScriptableObject
    {
        [Min(1)] public int levelId = 1;
        [Min(1)] public int trayCapacity = 4;

        public LevelAssetConfig assets = new();
        public List<TrayData> trays = new();

        public void ValidateOrThrow()
        {
            if (trayCapacity < 1)
                throw new InvalidOperationException(
                    $"{name}: trayCapacity must be at least 1.");

            if (assets == null)
                throw new InvalidOperationException(
                    $"{name}: asset configuration is missing.");

            ValidateRequiredKey(assets.trayPrefabKey, "Tray Prefab Key");
            ValidateRequiredKey(assets.toyPrefabKey, "Toy Prefab Key");

            foreach (TrayData tray in trays)
            {
                if (tray?.colors == null || tray.colors.Count > trayCapacity)
                {
                    throw new InvalidOperationException(
                        $"{name}: a tray has invalid color data.");
                }
            }
        }

        private static void ValidateRequiredKey(string key, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(key) ||
                !string.Equals(key, key.Trim(), StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"{key}-{fieldName} must be a non-empty exact Addressables name.");
            }
        }
    }
}