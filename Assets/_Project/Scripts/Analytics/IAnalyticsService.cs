namespace ColorFactory.Analytics
{
    /// <summary>
    /// Giao diện ghi event gameplay, để gameplay không phụ thuộc vào SDK analytics thật.
    /// </summary>
    public interface IAnalyticsService
    {
        /// <summary>
        /// Ghi nhận một event đã được kiểu hóa; không được ảnh hưởng luật gameplay.
        /// </summary>
        void Track<TEvent>(in TEvent analyticsEvent)
            where TEvent : struct, IAnalyticsEvent;
    }
}
