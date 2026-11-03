using ColorFactory.Levels;
using NUnit.Framework;

namespace ColorFactory.Tests
{
    /// <summary>
    /// Kiểm thử ngưỡng sao của LevelResultCalculator, gồm các giá trị biên.
    /// </summary>
    public sealed class LevelResultCalculatorTests
    {
        [Test]
        /// <summary>
        /// Số nước đi tại và dưới ngưỡng 3 sao phải được 3 sao; vượt ngưỡng thì giảm dần.
        /// </summary>
        public void CalculateStars_BoundaryValues()
        {
            LevelData level = TestLevelFactory.CreateValid();
            level.threeStarMoveTarget = 8;
            level.twoStarMoveTarget = 12;

            Assert.AreEqual(3, LevelResultCalculator.CalculateStars(level, 1));
            Assert.AreEqual(3, LevelResultCalculator.CalculateStars(level, 8));
            Assert.AreEqual(2, LevelResultCalculator.CalculateStars(level, 9));
            Assert.AreEqual(2, LevelResultCalculator.CalculateStars(level, 12));
            Assert.AreEqual(1, LevelResultCalculator.CalculateStars(level, 13));

            TestLevelFactory.Cleanup();
        }
    }
}
