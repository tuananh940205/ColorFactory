namespace ColorFactory.Analytics
{
    /// <summary>
    /// Hợp đồng chung của mọi event analytics, mỗi event có một tên ổn định.
    /// </summary>
    public interface IAnalyticsEvent
    {
        string EventName { get; }
    }
}
