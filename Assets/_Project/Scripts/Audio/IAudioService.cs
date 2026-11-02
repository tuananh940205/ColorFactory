using System;

namespace ColorFactory.Audio
{
    /// <summary>
    /// Giao diện phát sound effect, để gameplay và UI không phụ thuộc vào AudioSource cụ thể.
    /// </summary>
    public interface IAudioService
    {
        bool IsMuted { get; }

        /// <summary>
        /// Được gọi khi trạng thái mute thay đổi, UI dùng để cập nhật nút bật/tắt.
        /// </summary>
        event Action<bool> MutedChanged;

        /// <summary>
        /// Phát một sound effect nếu đang không mute và clip đã được load.
        /// </summary>
        void Play(SfxType type);

        /// <summary>
        /// Bật hoặc tắt toàn bộ sound effect và lưu lựa chọn cho lần chơi sau.
        /// </summary>
        void SetMuted(bool muted);
    }
}
