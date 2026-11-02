using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ColorFactory.Core;
using UnityEngine;
using UnityEngine.Audio;

namespace ColorFactory.Audio
{
    /// <summary>
    /// Service âm thanh dùng chung cho mọi scene: load clip bằng Addressables,
    /// phát qua pool AudioSource cố định và lưu trạng thái mute.
    /// </summary>
    [DefaultExecutionOrder(-900)]
    public sealed class AudioService : MonoBehaviour, IAudioService
    {
        private const string MutePrefsKey = "audio.muted";
        private const string ScopeName = "AudioService";

        [SerializeField] private string audioConfigKey = "AudioConfig";
        [SerializeField, Min(1)] private int sourcePoolSize = 4;

        [Tooltip("Không bắt buộc. AudioMixerGroup SFX để điều chỉnh volume trong Mixer.")]
        [SerializeField] private AudioMixerGroup sfxMixerGroup;

        // Clip được giữ suốt vòng đời app, nên Play không cần load hay GC trong lúc chơi.
        private readonly Dictionary<SfxType, AudioClip> clips = new();

        private AudioSource[] sources;
        private int nextSourceIndex;
        private bool isMuted;

        private AddressableService addressableService;
        private SceneAssetScope assetScope;
        private CancellationTokenSource lifetimeCancellation;

        public bool IsMuted => isMuted;

        public event Action<bool> MutedChanged;

        /// <summary>
        /// Đăng ký service một lần, giữ object sống qua các scene và tạo pool AudioSource.
        /// </summary>
        private void Awake()
        {
            if (Services.Audio != null && !ReferenceEquals(Services.Audio, this))
            {
                Destroy(gameObject);
                return;
            }

            Services.Register(this);
            DontDestroyOnLoad(gameObject);

            isMuted = PlayerPrefs.GetInt(MutePrefsKey, 0) == 1;
            CreateSourcePool();
        }

        /// <summary>
        /// Load config và toàn bộ clip sau khi AddressableService đã sẵn sàng.
        /// </summary>
        private async void Start()
        {
            addressableService = Services.Addressables;

            if (addressableService == null)
            {
                Debug.LogError("AudioService requires AddressableService.", this);
                return;
            }

            lifetimeCancellation = new CancellationTokenSource();

            try
            {
                await LoadClipsAsync(lifetimeCancellation.Token);
            }
            catch (OperationCanceledException)
            {
                // Đây là trường hợp bình thường khi app đóng lúc đang load.
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
        }

        /// <summary>
        /// Dừng các clip đang phát, trả lại scope rồi mới kết thúc service.
        /// </summary>
        private async void OnDestroy()
        {
            // Bản sao trùng lặp bị hủy trong Awake không sở hữu service.
            if (!ReferenceEquals(Services.Audio, this))
                return;

            Services.Unregister(this);

            StopAllSources();
            clips.Clear();

            lifetimeCancellation?.Cancel();

            if (assetScope != null)
                await assetScope.ReleaseAllAsync();

            lifetimeCancellation?.Dispose();
            lifetimeCancellation = null;
        }

        /// <summary>
        /// Phát clip theo SfxType, bỏ qua khi đang mute hoặc clip chưa được load.
        /// </summary>
        public void Play(SfxType type)
        {
            if (isMuted ||
                sources == null ||
                !clips.TryGetValue(type, out AudioClip clip) ||
                clip == null)
            {
                return;
            }

            AudioSource source = sources[nextSourceIndex];
            nextSourceIndex = (nextSourceIndex + 1) % sources.Length;

            source.PlayOneShot(clip);
        }

        /// <summary>
        /// Đổi trạng thái mute, dừng âm đang phát khi tắt tiếng và lưu lựa chọn.
        /// </summary>
        public void SetMuted(bool muted)
        {
            if (isMuted == muted)
                return;

            isMuted = muted;

            PlayerPrefs.SetInt(MutePrefsKey, muted ? 1 : 0);
            PlayerPrefs.Save();

            if (muted)
                StopAllSources();

            MutedChanged?.Invoke(isMuted);
        }

        /// <summary>
        /// Đọc config JSON rồi load từng clip theo key, mọi lỗi key đều dừng ngay.
        /// </summary>
        private async Task LoadClipsAsync(CancellationToken cancellationToken)
        {
            assetScope = addressableService.CreateSceneScope(ScopeName);

            TextAsset configAsset = await assetScope.LoadAsync<TextAsset>(
                audioConfigKey,
                cancellationToken);

            SfxKeyMap keyMap = SfxKeyMap.FromJson(configAsset.text);

            foreach (SfxType type in Enum.GetValues(typeof(SfxType)))
            {
                if (!keyMap.TryGetKey(type, out string key))
                    throw new InvalidOperationException($"Audio config is missing {type}.");

                clips[type] = await assetScope.LoadAsync<AudioClip>(key, cancellationToken);
            }
        }

        /// <summary>
        /// Tạo AudioSource một lần; các sound dùng lại pool này suốt vòng đời app.
        /// </summary>
        private void CreateSourcePool()
        {
            sources = new AudioSource[sourcePoolSize];

            for (int index = 0; index < sources.Length; index++)
            {
                AudioSource source = gameObject.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.loop = false;
                source.outputAudioMixerGroup = sfxMixerGroup;

                sources[index] = source;
            }
        }

        /// <summary>
        /// Dừng toàn bộ sound đang phát trong pool.
        /// </summary>
        private void StopAllSources()
        {
            if (sources == null)
                return;

            foreach (AudioSource source in sources)
                source.Stop();
        }
    }
}
