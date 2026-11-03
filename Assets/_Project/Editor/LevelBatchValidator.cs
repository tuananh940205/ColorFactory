using System.Collections.Generic;
using ColorFactory.Core;
using ColorFactory.Levels;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace ColorFactory.EditorTools
{
    /// <summary>
    /// Menu Editor kiểm tra toàn bộ catalog và LevelData trước khi build.
    /// Chỉ đọc asset, không sửa dữ liệu; mọi lỗi được in ra Console.
    /// </summary>
    public static class LevelBatchValidator
    {
        /// <summary>
        /// Chạy kiểm tra catalog, từng LevelData và Addressables entry, rồi in tổng kết.
        /// </summary>
        [MenuItem("Color Factory/Validate Level Catalog")]
        public static void ValidateCatalog()
        {
            string[] catalogGuids = AssetDatabase.FindAssets("t:LevelCatalog");
            if (catalogGuids.Length != 1)
            {
                Debug.LogError(
                    $"[LevelBatchValidator] Cần đúng 1 LevelCatalog, tìm thấy {catalogGuids.Length}.");
                return;
            }

            string catalogPath = AssetDatabase.GUIDToAssetPath(catalogGuids[0]);
            var catalog = AssetDatabase.LoadAssetAtPath<LevelCatalog>(catalogPath);

            var errors = new List<string>(
                LevelBatchRules.CheckCatalog(catalog.Levels));

            Dictionary<string, string> levelPaths = IndexLevelAssets(errors);
            AddressableAssetSettings settings =
                AddressableAssetSettingsDefaultObject.GetSettings(false);

            if (settings == null)
                errors.Add("Không tìm thấy AddressableAssetSettings.");

            foreach (LevelCatalogEntry entry in catalog.Levels)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.levelKey))
                    continue;

                if (!levelPaths.TryGetValue(entry.levelKey, out string path))
                {
                    errors.Add($"Không tìm thấy LevelData tên '{entry.levelKey}'.");
                    continue;
                }

                CheckLevelAsset(entry, path, settings, errors);
            }

            PrintSummary(catalog.Levels.Count, errors);
        }

        /// <summary>
        /// Lập chỉ mục LevelData theo tên asset, báo lỗi nếu hai asset trùng tên.
        /// </summary>
        private static Dictionary<string, string> IndexLevelAssets(List<string> errors)
        {
            var paths = new Dictionary<string, string>();

            foreach (string guid in AssetDatabase.FindAssets("t:LevelData"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string name = System.IO.Path.GetFileNameWithoutExtension(path);

                if (paths.ContainsKey(name))
                {
                    errors.Add($"Có hai LevelData cùng tên '{name}': {paths[name]} và {path}.");
                    continue;
                }

                paths.Add(name, path);
            }

            return paths;
        }

        /// <summary>
        /// Kiểm tra một LevelData: dữ liệu khay, levelId và Addressables address khớp key trong catalog.
        /// </summary>
        private static void CheckLevelAsset(
            LevelCatalogEntry entry,
            string path,
            AddressableAssetSettings settings,
            List<string> errors)
        {
            var level = AssetDatabase.LoadAssetAtPath<LevelData>(path);
            if (level == null)
            {
                errors.Add($"'{path}' không phải LevelData.");
                return;
            }

            var trays = new List<IReadOnlyList<ToyColor>>();
            foreach (TrayData tray in level.trays)
            {
                trays.Add(tray?.colors ?? new List<ToyColor>());
            }

            errors.AddRange(LevelBatchRules.CheckLevel(
                entry.levelId,
                level.levelId,
                level.trayCapacity,
                level.threeStarMoveTarget,
                level.twoStarMoveTarget,
                trays));

            if (settings == null)
                return;

            AddressableAssetEntry addressable =
                settings.FindAssetEntry(AssetDatabase.AssetPathToGUID(path));

            if (addressable == null)
            {
                errors.Add($"'{entry.levelKey}' chưa nằm trong Addressables group.");
            }
            else if (addressable.address != entry.levelKey)
            {
                errors.Add(
                    $"Addressables address '{addressable.address}' khác key '{entry.levelKey}'.");
            }
        }

        /// <summary>
        /// In kết quả: một dòng tổng kết và từng lỗi ra Console.
        /// </summary>
        private static void PrintSummary(int levelCount, List<string> errors)
        {
            if (errors.Count == 0)
            {
                Debug.Log($"[LevelBatchValidator] PASS: {levelCount} level hợp lệ.");
                return;
            }

            foreach (string error in errors)
                Debug.LogError($"[LevelBatchValidator] {error}");

            Debug.LogError(
                $"[LevelBatchValidator] FAIL: {errors.Count} lỗi trong {levelCount} level.");
        }
    }
}
