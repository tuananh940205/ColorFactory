using ColorFactory.Core;
using UnityEngine;

namespace ColorFactory.Analytics
{
    /// <summary>
    /// Service analytics persistent: đăng ký bản mock làm IAnalyticsService dùng chung.
    /// </summary>
    [DefaultExecutionOrder(-960)]
    public sealed class AnalyticsService : MonoBehaviour, IAnalyticsService
    {
        [Tooltip("Chỉ bật khi phát triển, để xem event trong Console.")]
        [SerializeField] private bool logToConsole;

        [SerializeField, Min(1)] private int maxBufferedEvents = 200;

        private LocalAnalyticsService mock;

        public LocalAnalyticsService Mock => mock;

        /// <summary>
        /// Đăng ký service một lần và giữ object sống qua các scene.
        /// </summary>
        private void Awake()
        {
            if (Services.Analytics != null && !ReferenceEquals(Services.Analytics, this))
            {
                Destroy(gameObject);
                return;
            }

            Services.Register(this);
            DontDestroyOnLoad(gameObject);

            mock = new LocalAnalyticsService(logToConsole, maxBufferedEvents);
        }

        /// <summary>
        /// Gỡ service khi object persistent bị hủy.
        /// </summary>
        private void OnDestroy()
        {
            Services.Unregister(this);
        }

        /// <summary>
        /// Chuyển event sang bản mock hiện tại.
        /// </summary>
        public void Track<TEvent>(in TEvent analyticsEvent)
            where TEvent : struct, IAnalyticsEvent
        {
            mock?.Track(analyticsEvent);
        }
    }
}
