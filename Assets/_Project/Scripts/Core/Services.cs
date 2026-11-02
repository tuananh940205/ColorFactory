using ColorFactory.Analytics;
using ColorFactory.Audio;
using ColorFactory.Economy;

namespace ColorFactory.Core
{
    /// <summary>
    /// Lưu các service dùng xuyên scene tại một điểm truy cập duy nhất.
    /// </summary>
    public static class Services
    {
        public static AddressableService Addressables { get; private set; }
        public static SceneFader SceneFader { get; private set; }
        public static IAudioService Audio { get; private set; }
        public static WalletService Wallet { get; private set; }
        public static IAnalyticsService Analytics { get; private set; }

        /// <summary>
        /// Đăng ký service analytics khi AnalyticsService persistent được tạo.
        /// </summary>
        public static void Register(IAnalyticsService service)
        {
            Analytics = service;
        }

        /// <summary>
        /// Gỡ service analytics khi AnalyticsService bị hủy.
        /// </summary>
        public static void Unregister(IAnalyticsService service)
        {
            if (Analytics == service)
                Analytics = null;
        }

        /// <summary>
        /// Đăng ký ví coin khi WalletService persistent được tạo.
        /// </summary>
        public static void Register(WalletService service)
        {
            Wallet = service;
        }

        /// <summary>
        /// Gỡ ví coin khi WalletService persistent bị hủy.
        /// </summary>
        public static void Unregister(WalletService service)
        {
            if (Wallet == service)
                Wallet = null;
        }

        /// <summary>
        /// Đăng ký service âm thanh khi AudioService được tạo.
        /// </summary>
        public static void Register(IAudioService service)
        {
            Audio = service;
        }

        /// <summary>
        /// Gỡ service âm thanh khi AudioService bị hủy.
        /// </summary>
        public static void Unregister(IAudioService service)
        {
            if (Audio == service)
                Audio = null;
        }

        /// <summary>
        /// Đăng ký service Addressables khi bootstrap được tạo.
        /// </summary>
        public static void Register(AddressableService service)
        {
            Addressables = service;
        }

        /// <summary>
        /// Gỡ service Addressables khi bootstrap gốc bị hủy.
        /// </summary>
        public static void Unregister(AddressableService service)
        {
            if (Addressables == service)
                Addressables = null;
        }

        /// <summary>
        /// Đăng ký service fade để các view không phụ thuộc trực tiếp lẫn nhau.
        /// </summary>
        public static void Register(SceneFader service)
        {
            SceneFader = service;
        }

        /// <summary>
        /// Gỡ service fade khi bootstrap gốc bị hủy.
        /// </summary>
        public static void Unregister(SceneFader service)
        {
            if (SceneFader == service)
                SceneFader = null;
        }
    }
}
