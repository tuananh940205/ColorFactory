using ColorFactory.Levels;
using NUnit.Framework;

namespace ColorFactory.Tests
{
    /// <summary>
    /// Kiểm thử LevelSession: yêu cầu level chỉ được dùng một lần.
    /// </summary>
    public sealed class LevelSessionTests
    {
        [TearDown]
        /// <summary>
        /// Tiêu thụ request còn sót để static state không ảnh hưởng test khác.
        /// </summary>
        public void TearDown()
        {
            LevelSession.ConsumeRequestedLevelOrDefault(-1);
        }

        [Test]
        /// <summary>
        /// Request level phải được trả đúng một lần, các lần sau dùng fallback.
        /// </summary>
        public void ConsumeRequestedLevel_UsesRequestOnlyOnce()
        {
            LevelSession.RequestLevel(5);

            Assert.AreEqual(5, LevelSession.ConsumeRequestedLevelOrDefault(1));
            Assert.AreEqual(1, LevelSession.ConsumeRequestedLevelOrDefault(1));
        }

        [Test]
        /// <summary>
        /// Không có request thì phải dùng fallback.
        /// </summary>
        public void ConsumeRequestedLevel_WithoutRequest_ReturnsFallback()
        {
            Assert.AreEqual(3, LevelSession.ConsumeRequestedLevelOrDefault(3));
        }
    }
}
