using System.Collections.Generic;
using ColorFactory.Core;
using ColorFactory.Levels;
using NUnit.Framework;

namespace ColorFactory.Tests
{
    /// <summary>
    /// Kiểm thử luật batch check của catalog và level (LevelBatchRules), gồm các level dữ liệu lỗi.
    /// </summary>
    public sealed class LevelBatchRulesTests
    {
        [Test]
        /// <summary>
        /// Catalog hợp lệ với levelId và key duy nhất không sinh lỗi.
        /// </summary>
        public void CheckCatalog_ValidEntries_NoErrors()
        {
            var entries = new List<LevelCatalogEntry>
            {
                new LevelCatalogEntry { levelId = 1, levelKey = "Level_001" },
                new LevelCatalogEntry { levelId = 2, levelKey = "Level_002" }
            };

            CollectionAssert.IsEmpty(LevelBatchRules.CheckCatalog(entries));
        }

        [Test]
        /// <summary>
        /// Catalog rỗng hoặc null phải báo lỗi.
        /// </summary>
        public void CheckCatalog_EmptyOrNull_ReportsError()
        {
            Assert.IsNotEmpty(LevelBatchRules.CheckCatalog(new List<LevelCatalogEntry>()));
            Assert.IsNotEmpty(LevelBatchRules.CheckCatalog(null));
        }

        [Test]
        /// <summary>
        /// levelId trùng và Addressables key trùng (không phân biệt hoa thường) phải bị báo.
        /// </summary>
        public void CheckCatalog_DuplicateIdOrKey_ReportsError()
        {
            var duplicateId = new List<LevelCatalogEntry>
            {
                new LevelCatalogEntry { levelId = 1, levelKey = "Level_001" },
                new LevelCatalogEntry { levelId = 1, levelKey = "Level_01b" }
            };
            var duplicateKey = new List<LevelCatalogEntry>
            {
                new LevelCatalogEntry { levelId = 1, levelKey = "Level_001" },
                new LevelCatalogEntry { levelId = 2, levelKey = "level_001" }
            };

            Assert.AreEqual(1, LevelBatchRules.CheckCatalog(duplicateId).Count);
            Assert.AreEqual(1, LevelBatchRules.CheckCatalog(duplicateKey).Count);
        }

        [Test]
        /// <summary>
        /// Key rỗng hoặc có khoảng trắng thừa phải bị báo.
        /// </summary>
        public void CheckCatalog_EmptyOrPaddedKey_ReportsError()
        {
            var entries = new List<LevelCatalogEntry>
            {
                new LevelCatalogEntry { levelId = 1, levelKey = "" },
                new LevelCatalogEntry { levelId = 2, levelKey = " Level_002" }
            };

            Assert.AreEqual(2, LevelBatchRules.CheckCatalog(entries).Count);
        }

        [Test]
        /// <summary>
        /// Level hợp lệ: levelId khớp, mỗi màu đủ sức chứa, có khay trống, không lỗi.
        /// </summary>
        public void CheckLevel_ValidLevel_NoErrors()
        {
            IReadOnlyList<IReadOnlyList<ToyColor>> trays = Trays(
                new[] { ToyColor.Red, ToyColor.Blue },
                new[] { ToyColor.Blue, ToyColor.Red },
                new ToyColor[0]);

            CollectionAssert.IsEmpty(
                LevelBatchRules.CheckLevel(1, 1, 2, 3, 5, trays));
        }

        [Test]
        /// <summary>
        /// Level dữ liệu lỗi: levelId lệch catalog, màu thiếu toy, không có khay trống,
        /// khay vượt sức chứa và sao target sai đều phải được báo.
        /// </summary>
        public void CheckLevel_MalformedLevel_ReportsEachProblem()
        {
            IReadOnlyList<IReadOnlyList<ToyColor>> wrongCount = Trays(
                new[] { ToyColor.Red },
                new[] { ToyColor.Blue, ToyColor.Blue });
            IReadOnlyList<IReadOnlyList<ToyColor>> noEmpty = Trays(
                new[] { ToyColor.Red, ToyColor.Blue },
                new[] { ToyColor.Blue, ToyColor.Red });
            IReadOnlyList<IReadOnlyList<ToyColor>> overCapacity = Trays(
                new[] { ToyColor.Red, ToyColor.Red, ToyColor.Red },
                new ToyColor[0]);

            Assert.IsNotEmpty(LevelBatchRules.CheckLevel(2, 1, 2, 3, 5, Trays(new ToyColor[0])));
            Assert.IsNotEmpty(LevelBatchRules.CheckLevel(1, 1, 2, 3, 5, wrongCount));
            Assert.IsNotEmpty(LevelBatchRules.CheckLevel(1, 1, 2, 3, 5, noEmpty));
            Assert.IsNotEmpty(LevelBatchRules.CheckLevel(1, 1, 2, 3, 5, overCapacity));
            Assert.IsNotEmpty(LevelBatchRules.CheckLevel(1, 1, 2, 6, 5, Trays(new ToyColor[0])));
        }

        /// <summary>
        /// Tạo danh sách khay chỉ-đọc từ các mảng màu, dùng cho test.
        /// </summary>
        private static IReadOnlyList<IReadOnlyList<ToyColor>> Trays(params ToyColor[][] trays)
        {
            return trays;
        }
    }
}
