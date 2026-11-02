using System;
using System.Collections.Generic;
using ColorFactory.Core;
using UnityEngine;

namespace ColorFactory.Levels
{
    /// <summary>
    /// Dữ liệu một khay, sắp xếp màu từ đáy lên đỉnh.
    /// </summary>
    [Serializable]
    public sealed class TrayData
    {
        public List<ToyColor> colors = new();
    }

    /// <summary>
    /// Các Addressables key cần cho một level.
    /// </summary>
    [Serializable]
    public sealed class LevelAssetConfig
    {
        [Tooltip("Address phải bằng tên root GameObject của tray prefab.")]
        public string trayPrefabKey;

        [Tooltip("Address phải bằng tên root GameObject của toy prefab.")]
        public string toyPrefabKey;

        [Tooltip("Không bắt buộc. Address của sprite background.")]
        public string backgroundSpriteKey;

        [Tooltip("Không bắt buộc. Address của SpriteAtlas gameplay.")]
        public string gameplaySpriteAtlasKey;
    }

    /// <summary>
    /// ScriptableObject chứa cấu hình gameplay và asset key của một level.
    /// </summary>
    [CreateAssetMenu(
        fileName = "Level_001",
        menuName = "Color Factory/Level Data")]
    public sealed class LevelData : ScriptableObject
    {
        [Min(1)] public int levelId = 1;
        [Min(1)] public int trayCapacity = 4;
        
        [Header("Star Rating")]
        [Min(1)] public int threeStarMoveTarget = 8;
        [Min(1)] public int twoStarMoveTarget = 12;

        public LevelAssetConfig assets = new();
        public List<TrayData> trays = new();

        /// <summary>
        /// Kiểm tra toàn bộ dữ liệu trước khi tạo BoardModel.
        /// </summary>
        public void ValidateOrThrow()
        {
            if (trayCapacity < 1)
                throw new InvalidOperationException(
                    $"{name}: trayCapacity must be at least 1.");
            
            if (threeStarMoveTarget > twoStarMoveTarget)
            {
                throw new InvalidOperationException(
                    $"{name}: 3-star target must be less than or equal to 2-star target.");
            }

            if (assets == null)
                throw new InvalidOperationException(
                    $"{name}: asset configuration is missing.");

            ValidateRequiredKey(assets.trayPrefabKey, "Tray Prefab Key");
            ValidateRequiredKey(assets.toyPrefabKey, "Toy Prefab Key");

            var colorCounts = new Dictionary<ToyColor, int>();

            foreach (TrayData tray in trays)
            {
                if (tray?.colors == null || tray.colors.Count > trayCapacity)
                {
                    throw new InvalidOperationException(
                        $"{name}: a tray has invalid color data.");
                }

                foreach (ToyColor color in tray.colors)
                {
                    colorCounts.TryGetValue(color, out int currentCount);
                    colorCounts[color] = currentCount + 1;
                }
            }

            ValidateColorCountsOrThrow(colorCounts);
        }

        /// <summary>
        /// Bảo đảm mỗi màu có đúng số toy bằng sức chứa một khay.
        /// </summary>
        private void ValidateColorCountsOrThrow(
            IReadOnlyDictionary<ToyColor, int> colorCounts)
        {
            if (colorCounts.Count == 0)
            {
                throw new InvalidOperationException(
                    $"{name}: level must contain at least one toy color.");
            }

            foreach ((ToyColor color, int count) in colorCounts)
            {
                if (count != trayCapacity)
                {
                    throw new InvalidOperationException(
                        $"{name}: {color} has {count} toys; " +
                        $"it must have exactly {trayCapacity}.");
                }
            }

            // Ít nhất một khay trống giúp level chưa hoàn thành có không gian di chuyển.
            if (trays.Count < colorCounts.Count + 1)
            {
                throw new InvalidOperationException(
                    $"{name}: add at least one empty tray.");
            }
        }

        /// <summary>
        /// Kiểm tra key bắt buộc không rỗng và không có khoảng trắng thừa.
        /// </summary>
        private static void ValidateRequiredKey(
            string key,
            string fieldName)
        {
            if (string.IsNullOrWhiteSpace(key) ||
                !string.Equals(key, key.Trim(), StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"{fieldName} must be a non-empty exact Addressables name.");
            }
        }
    }
}