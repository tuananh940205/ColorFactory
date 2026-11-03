using System;
using ColorFactory.Core;
using ColorFactory.Levels;
using NUnit.Framework;

namespace ColorFactory.Tests
{
    /// <summary>
    /// Kiểm thử LevelData.ValidateOrThrow với level hợp lệ và các level dữ liệu lỗi (malformed level).
    /// </summary>
    public sealed class LevelDataValidationTests
    {
        [TearDown]
        /// <summary>
        /// Dọn LevelData đã tạo sau mỗi test.
        /// </summary>
        public void TearDown()
        {
            TestLevelFactory.Cleanup();
        }

        [Test]
        /// <summary>
        /// Level hợp lệ không được ném exception.
        /// </summary>
        public void ValidateOrThrow_ValidLevel_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => TestLevelFactory.CreateValid().ValidateOrThrow());
        }

        [Test]
        /// <summary>
        /// Một màu có số toy khác sức chứa khay phải bị từ chối.
        /// </summary>
        public void ValidateOrThrow_ColorCountMismatch_Throws()
        {
            LevelData level = TestLevelFactory.Create(
                1,
                2,
                new[] { ToyColor.Red },
                new[] { ToyColor.Blue, ToyColor.Blue },
                new ToyColor[0]);

            Assert.Throws<InvalidOperationException>(() => level.ValidateOrThrow());
        }

        [Test]
        /// <summary>
        /// Khay chứa quá sức chứa phải bị từ chối.
        /// </summary>
        public void ValidateOrThrow_TrayOverCapacity_Throws()
        {
            LevelData level = TestLevelFactory.Create(
                1,
                2,
                new[] { ToyColor.Red, ToyColor.Red, ToyColor.Red },
                new[] { ToyColor.Blue, ToyColor.Blue },
                new ToyColor[0]);

            Assert.Throws<InvalidOperationException>(() => level.ValidateOrThrow());
        }

        [Test]
        /// <summary>
        /// Không có khay thừa ngoài số màu phải bị từ chối.
        /// </summary>
        public void ValidateOrThrow_NoSpareTray_Throws()
        {
            LevelData level = TestLevelFactory.Create(
                1,
                2,
                new[] { ToyColor.Red, ToyColor.Blue },
                new[] { ToyColor.Blue, ToyColor.Red });

            Assert.Throws<InvalidOperationException>(() => level.ValidateOrThrow());
        }

        [Test]
        /// <summary>
        /// Level không có toy nào phải bị từ chối.
        /// </summary>
        public void ValidateOrThrow_NoToys_Throws()
        {
            LevelData level = TestLevelFactory.Create(
                1,
                2,
                new ToyColor[0],
                new ToyColor[0]);

            Assert.Throws<InvalidOperationException>(() => level.ValidateOrThrow());
        }

        [Test]
        /// <summary>
        /// Ngưỡng 3 sao lớn hơn ngưỡng 2 sao phải bị từ chối.
        /// </summary>
        public void ValidateOrThrow_ThreeStarTargetAboveTwoStar_Throws()
        {
            LevelData level = TestLevelFactory.CreateValid();
            level.threeStarMoveTarget = 13;
            level.twoStarMoveTarget = 12;

            Assert.Throws<InvalidOperationException>(() => level.ValidateOrThrow());
        }

        [Test]
        /// <summary>
        /// Thiếu Addressables key của tray prefab phải bị từ chối.
        /// </summary>
        public void ValidateOrThrow_MissingTrayPrefabKey_Throws()
        {
            LevelData level = TestLevelFactory.CreateValid();
            level.assets.trayPrefabKey = "";

            Assert.Throws<InvalidOperationException>(() => level.ValidateOrThrow());
        }

        [Test]
        /// <summary>
        /// Key có khoảng trắng thừa không khớp Addressables name nên phải bị từ chối.
        /// </summary>
        public void ValidateOrThrow_KeyWithWhitespace_Throws()
        {
            LevelData level = TestLevelFactory.CreateValid();
            level.assets.toyPrefabKey = " ToyPrefab";

            Assert.Throws<InvalidOperationException>(() => level.ValidateOrThrow());
        }
    }
}
