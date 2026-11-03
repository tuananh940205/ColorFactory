using System.Collections.Generic;
using ColorFactory.Core;
using ColorFactory.Levels;
using UnityEngine;

namespace ColorFactory.Tests
{
    /// <summary>
    /// Tạo LevelData trong bộ test và theo dõi các ScriptableObject đã tạo để dọn sau mỗi test.
    /// </summary>
    public static class TestLevelFactory
    {
        private static readonly List<Object> created = new();

        /// <summary>
        /// Tạo LevelData với key Addressables hợp lệ, sao mặc định 8/12 và các khay theo dữ liệu truyền vào.
        /// </summary>
        public static LevelData Create(
            int levelId,
            int trayCapacity,
            params ToyColor[][] trays)
        {
            var level = ScriptableObject.CreateInstance<LevelData>();
            created.Add(level);

            level.levelId = levelId;
            level.trayCapacity = trayCapacity;
            level.threeStarMoveTarget = 8;
            level.twoStarMoveTarget = 12;
            level.assets = new LevelAssetConfig
            {
                trayPrefabKey = "TrayPrefab",
                toyPrefabKey = "ToyPrefab"
            };

            foreach (ToyColor[] colors in trays)
            {
                var tray = new TrayData();
                tray.colors.AddRange(colors);
                level.trays.Add(tray);
            }

            return level;
        }

        /// <summary>
        /// Tạo level hợp lệ nhỏ: hai màu, sức chứa 2, một khay trống.
        /// </summary>
        public static LevelData CreateValid()
        {
            return Create(
                1,
                2,
                new[] { ToyColor.Red, ToyColor.Blue },
                new[] { ToyColor.Blue, ToyColor.Red },
                new ToyColor[0]);
        }

        /// <summary>
        /// Hủy toàn bộ ScriptableObject đã tạo trong test hiện tại.
        /// </summary>
        public static void Cleanup()
        {
            foreach (Object asset in created)
            {
                if (asset != null)
                    Object.DestroyImmediate(asset);
            }

            created.Clear();
        }
    }
}
