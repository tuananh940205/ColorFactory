namespace ColorFactory.Core
{
    /// <summary>
    /// Lưu các service dùng xuyên scene tại một điểm truy cập duy nhất.
    /// </summary>
    public static class Services
    {
        public static AddressableService Addressables { get; private set; }
        public static SceneFader SceneFader { get; private set; }

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
