using System;
using System.Collections.Generic;
using UnityEngine;

namespace ColorFactory.Levels
{
    /// <summary>
    /// Một entry ánh xạ levelId sang Addressables key của LevelData.
    /// </summary>
    [Serializable]
    public sealed class LevelCatalogEntry
    {
        public int levelId;
        public string levelKey;
    }

    /// <summary>
    /// Danh sách các level có trong game.
    /// Catalog cũng được load bằng Addressables.
    /// </summary>
    [CreateAssetMenu(
        fileName = "ColorFactoryLevelCatalog",
        menuName = "Color Factory/Level Catalog")]
    public sealed class LevelCatalog : ScriptableObject
    {
        [SerializeField] private List<LevelCatalogEntry> levels = new();

        /// <summary>
        /// Tìm Addressables key theo số level.
        /// </summary>
        public bool TryGetLevelKey(int levelId, out string levelKey)
        {
            foreach (LevelCatalogEntry entry in levels)
            {
                if (entry != null && entry.levelId == levelId)
                {
                    levelKey = entry.levelKey;
                    return !string.IsNullOrWhiteSpace(levelKey);
                }
            }

            levelKey = null;
            return false;
        }

        /// <summary>
        /// Tìm level kế tiếp theo thứ tự trong catalog.
        /// </summary>
        public bool TryGetNextLevelId(int currentLevelId, out int nextLevelId)
        {
            nextLevelId = -1;

            foreach (LevelCatalogEntry entry in levels)
            {
                if (entry != null && entry.levelId > currentLevelId &&
                    (nextLevelId == -1 || entry.levelId < nextLevelId))
                {
                    nextLevelId = entry.levelId;
                }
            }

            return nextLevelId > 0;
        }
    }
}