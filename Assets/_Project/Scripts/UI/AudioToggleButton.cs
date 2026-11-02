using ColorFactory.Audio;
using ColorFactory.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ColorFactory.UI
{
    /// <summary>
    /// Nút bật/tắt sound, dùng chung ở Level Select và Gameplay.
    /// </summary>
    public sealed class AudioToggleButton : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private TMP_Text label;
        [SerializeField] private string soundOnText = "SOUND: ON";
        [SerializeField] private string soundOffText = "SOUND: OFF";

        private IAudioService audioService;

        /// <summary>
        /// Lấy AudioService đã đăng ký, hiển thị trạng thái hiện tại và lắng nghe thay đổi.
        /// </summary>
        private void Start()
        {
            audioService = Services.Audio;

            if (audioService == null)
            {
                Debug.LogError("AudioToggleButton requires AudioService.", this);
                return;
            }

            button.onClick.AddListener(ToggleMute);
            audioService.MutedChanged += UpdateLabel;
            UpdateLabel(audioService.IsMuted);
        }

        /// <summary>
        /// Gỡ listener và event để không gọi vào UI đã bị hủy.
        /// </summary>
        private void OnDestroy()
        {
            if (button != null)
                button.onClick.RemoveListener(ToggleMute);

            if (audioService != null)
                audioService.MutedChanged -= UpdateLabel;
        }

        /// <summary>
        /// Đảo trạng thái mute của toàn bộ sound effect.
        /// </summary>
        private void ToggleMute()
        {
            audioService.SetMuted(!audioService.IsMuted);
        }

        /// <summary>
        /// Cập nhật nhãn theo trạng thái mute hiện tại.
        /// </summary>
        private void UpdateLabel(bool muted)
        {
            if (label != null)
                label.text = muted ? soundOffText : soundOnText;
        }
    }
}
