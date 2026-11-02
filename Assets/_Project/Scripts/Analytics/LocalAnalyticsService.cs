using System;
using System.Collections.Generic;
using UnityEngine;

namespace ColorFactory.Analytics
{
    /// <summary>
    /// Bản mock lưu event trong bộ nhớ, có thể in ra Console khi bật logging.
    /// </summary>
    public sealed class LocalAnalyticsService : IAnalyticsService
    {
        private readonly List<IAnalyticsEvent> buffer = new();
        private readonly bool logToConsole;
        private readonly int maxBufferedEvents;

        /// <summary>
        /// Các event gần nhất, dùng để kiểm tra mock trong test hoặc debug.
        /// </summary>
        public IReadOnlyList<IAnalyticsEvent> BufferedEvents => buffer;

        /// <summary>
        /// Tạo mock với giới hạn bộ nhớ và tùy chọn in event ra Console.
        /// </summary>
        public LocalAnalyticsService(bool logToConsole, int maxBufferedEvents)
        {
            if (maxBufferedEvents < 1)
                throw new ArgumentOutOfRangeException(nameof(maxBufferedEvents));

            this.logToConsole = logToConsole;
            this.maxBufferedEvents = maxBufferedEvents;
        }

        /// <summary>
        /// Lưu event vào buffer có giới hạn và in ra Console nếu được bật.
        /// </summary>
        public void Track<TEvent>(in TEvent analyticsEvent)
            where TEvent : struct, IAnalyticsEvent
        {
            // Boxing chỉ xảy ra khi có event, không xảy ra mỗi frame.
            IAnalyticsEvent boxedEvent = analyticsEvent;

            buffer.Add(boxedEvent);

            // Bỏ event cũ nhất để bộ nhớ không tăng mãi trong phiên chơi dài.
            if (buffer.Count > maxBufferedEvents)
                buffer.RemoveAt(0);

            if (logToConsole)
                Debug.Log($"[Analytics] {boxedEvent.EventName} {boxedEvent}");
        }
    }
}
