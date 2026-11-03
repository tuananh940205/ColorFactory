using System;
using System.Collections.Generic;
using ColorFactory.Core;

namespace ColorFactory.Levels
{
    /// <summary>
    /// Luật kiểm tra hàng loạt cho catalog và từng level.
    /// Viết bằng C# thuần, nhận dữ liệu phẳng, nên không phụ thuộc Unity Editor.
    /// Trả về danh sách lỗi thay vì ném exception để batch check báo đầy đủ mọi level.
    /// </summary>
    public static class LevelBatchRules
    {
        /// <summary>
        /// Kiểm tra catalog: không rỗng, levelId không trùng, levelKey không rỗng và không trùng.
        /// </summary>
        public static List<string> CheckCatalog(IReadOnlyList<LevelCatalogEntry> entries)
        {
            var errors = new List<string>();

            if (entries == null || entries.Count == 0)
            {
                errors.Add("Catalog không có level nào.");
                return errors;
            }

            var seenIds = new HashSet<int>();
            var seenKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (LevelCatalogEntry entry in entries)
            {
                if (entry == null)
                {
                    errors.Add("Catalog có entry null.");
                    continue;
                }

                if (!seenIds.Add(entry.levelId))
                    errors.Add($"levelId {entry.levelId} bị trùng trong catalog.");

                if (string.IsNullOrWhiteSpace(entry.levelKey) ||
                    !string.Equals(entry.levelKey, entry.levelKey.Trim(), StringComparison.Ordinal))
                {
                    errors.Add($"levelId {entry.levelId} có levelKey rỗng hoặc có khoảng trắng thừa.");
                    continue;
                }

                if (!seenKeys.Add(entry.levelKey))
                    errors.Add($"Addressables key '{entry.levelKey}' bị trùng trong catalog.");
            }

            return errors;
        }

        /// <summary>
        /// Kiểm tra một level theo luật acceptance của Bài 27:
        /// levelId khớp catalog, sức chứa hợp lệ, mỗi màu đủ đúng một khay,
        /// có ít nhất một khay trống và star target hợp lệ.
        /// </summary>
        public static List<string> CheckLevel(
            int expectedLevelId,
            int actualLevelId,
            int trayCapacity,
            int threeStarMoveTarget,
            int twoStarMoveTarget,
            IReadOnlyList<IReadOnlyList<ToyColor>> trays)
        {
            var errors = new List<string>();

            if (expectedLevelId != actualLevelId)
                errors.Add($"levelId trong asset là {actualLevelId}, catalog ghi {expectedLevelId}.");

            if (trayCapacity < 1)
            {
                errors.Add("trayCapacity phải ít nhất là 1.");
                return errors;
            }

            if (threeStarMoveTarget < 1 || threeStarMoveTarget > twoStarMoveTarget)
                errors.Add("Star target phải thỏa 1 <= 3 sao <= 2 sao.");

            if (trays == null || trays.Count == 0)
            {
                errors.Add("Level không có khay nào.");
                return errors;
            }

            var colorCounts = new Dictionary<ToyColor, int>();
            bool hasEmptyTray = false;

            foreach (IReadOnlyList<ToyColor> tray in trays)
            {
                if (tray == null || tray.Count > trayCapacity)
                {
                    errors.Add("Có khay null hoặc chứa nhiều toy hơn sức chứa.");
                    continue;
                }

                if (tray.Count == 0)
                    hasEmptyTray = true;

                foreach (ToyColor color in tray)
                {
                    colorCounts.TryGetValue(color, out int current);
                    colorCounts[color] = current + 1;
                }
            }

            if (colorCounts.Count == 0)
                errors.Add("Level không có toy nào.");

            foreach (KeyValuePair<ToyColor, int> pair in colorCounts)
            {
                if (pair.Value != trayCapacity)
                {
                    errors.Add(
                        $"Màu {pair.Key} có {pair.Value} toy, phải đúng {trayCapacity}.");
                }
            }

            if (!hasEmptyTray)
                errors.Add($"Level {actualLevelId} không có khay trống nào.");

            if (trays.Count < colorCounts.Count + 1)
                errors.Add($"Level {actualLevelId} Cần ít nhất một khay thừa ngoài số màu.");

            return errors;
        }
    }
}
