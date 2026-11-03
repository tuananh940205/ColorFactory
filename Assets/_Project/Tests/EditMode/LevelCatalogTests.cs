using System.Collections.Generic;
using System.Reflection;
using ColorFactory.Levels;
using NUnit.Framework;
using UnityEngine;

namespace ColorFactory.Tests
{
    /// <summary>
    /// Kiểm thử tra cứu key và level kế tiếp của LevelCatalog.
    /// </summary>
    public sealed class LevelCatalogTests
    {
        private readonly List<Object> created = new();

        [TearDown]
        /// <summary>
        /// Hủy catalog đã tạo trong test.
        /// </summary>
        public void TearDown()
        {
            foreach (Object asset in created)
            {
                if (asset != null)
                    Object.DestroyImmediate(asset);
            }

            created.Clear();
        }

        [Test]
        /// <summary>
        /// Tra key theo levelId và tìm level kế tiếp theo thứ tự catalog.
        /// </summary>
        public void Lookup_FindsKeyAndNextLevel()
        {
            LevelCatalog catalog = CreateCatalog(
                Entry(1, "Level_001"),
                Entry(2, "Level_002"),
                Entry(3, "Level_003"));

            Assert.IsTrue(catalog.TryGetLevelKey(2, out string key));
            Assert.AreEqual("Level_002", key);
            Assert.IsTrue(catalog.TryGetNextLevelId(1, out int next));
            Assert.AreEqual(2, next);
        }

        [Test]
        /// <summary>
        /// levelId không có trong catalog và level cuối phải trả false.
        /// </summary>
        public void Lookup_MissingLevelAndLastLevel_ReturnFalse()
        {
            LevelCatalog catalog = CreateCatalog(Entry(1, "Level_001"));

            Assert.IsFalse(catalog.TryGetLevelKey(99, out _));
            Assert.IsFalse(catalog.TryGetNextLevelId(1, out _));
        }

        [Test]
        /// <summary>
        /// Entry có key rỗng không được coi là tra cứu thành công.
        /// </summary>
        public void Lookup_EmptyKey_ReturnsFalse()
        {
            LevelCatalog catalog = CreateCatalog(Entry(1, ""));

            Assert.IsFalse(catalog.TryGetLevelKey(1, out _));
        }

        /// <summary>
        /// Tạo entry catalog cho test.
        /// </summary>
        private static LevelCatalogEntry Entry(int levelId, string key)
        {
            return new LevelCatalogEntry { levelId = levelId, levelKey = key };
        }

        /// <summary>
        /// Tạo LevelCatalog và gán danh sách entry qua field serialize private, giống cách Inspector gán dữ liệu.
        /// </summary>
        private LevelCatalog CreateCatalog(params LevelCatalogEntry[] entries)
        {
            var catalog = ScriptableObject.CreateInstance<LevelCatalog>();
            created.Add(catalog);

            FieldInfo field = typeof(LevelCatalog).GetField(
                "levels",
                BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(catalog, new List<LevelCatalogEntry>(entries));

            return catalog;
        }
    }
}
