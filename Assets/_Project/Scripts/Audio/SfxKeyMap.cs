using System;
using System.Collections.Generic;
using UnityEngine;

namespace ColorFactory.Audio
{
    /// <summary>
    /// Bảng ánh xạ từ mỗi SfxType sang Addressables key, đọc từ text asset JSON.
    /// </summary>
    public sealed class SfxKeyMap
    {
        /// <summary>
        /// Hình dạng JSON của file cấu hình audio, chỉ dùng để JsonUtility đọc dữ liệu.
        /// </summary>
        [Serializable]
        public sealed class ConfigDto
        {
            public string tap;
            public string move;
            public string undo;
            public string hint;
            public string win;
            public string button;
        }

        private readonly IReadOnlyDictionary<SfxType, string> keys;

        /// <summary>
        /// Tạo bảng key từ dữ liệu đã được kiểm tra trước đó.
        /// </summary>
        public SfxKeyMap(IReadOnlyDictionary<SfxType, string> keys)
        {
            this.keys = keys ?? throw new ArgumentNullException(nameof(keys));
        }

        /// <summary>
        /// Lấy Addressables key của một sound effect.
        /// </summary>
        public bool TryGetKey(SfxType type, out string key)
        {
            return keys.TryGetValue(type, out key);
        }

        /// <summary>
        /// Đọc JSON và kiểm tra mọi sound effect đều có key hợp lệ.
        /// </summary>
        public static SfxKeyMap FromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                throw new InvalidOperationException("Audio config is empty.");

            ConfigDto dto = JsonUtility.FromJson<ConfigDto>(json);

            if (dto == null)
                throw new InvalidOperationException("Audio config cannot be parsed.");

            var keys = new Dictionary<SfxType, string>
            {
                [SfxType.Tap] = dto.tap,
                [SfxType.Move] = dto.move,
                [SfxType.Undo] = dto.undo,
                [SfxType.Hint] = dto.hint,
                [SfxType.Win] = dto.win,
                [SfxType.Button] = dto.button
            };

            foreach ((SfxType type, string key) in keys)
                ValidateKey(type, key);

            return new SfxKeyMap(keys);
        }

        /// <summary>
        /// Kiểm tra key không rỗng và không có khoảng trắng thừa.
        /// </summary>
        private static void ValidateKey(SfxType type, string key)
        {
            if (string.IsNullOrWhiteSpace(key) ||
                !string.Equals(key, key.Trim(), StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Audio config key for {type} must be a non-empty exact Addressables name.");
            }
        }
    }
}
