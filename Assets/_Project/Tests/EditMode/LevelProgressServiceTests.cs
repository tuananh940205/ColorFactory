using ColorFactory.Levels;
using NUnit.Framework;
using UnityEngine;

namespace ColorFactory.Tests
{
    /// <summary>
    /// Kiểm thử mở khóa level, lưu best result và độ bền dữ liệu trong PlayerPrefs.
    /// Chỉ xóa đúng các key của test, không gọi PlayerPrefs.DeleteAll để không ảnh hưởng dữ liệu Editor.
    /// </summary>
    public sealed class LevelProgressServiceTests
    {
        private const string HighestUnlockedKey = "ColorFactory.HighestUnlockedLevel";

        [SetUp]
        /// <summary>
        /// Xóa tiến độ của các level dùng trong test trước khi chạy.
        /// </summary>
        public void SetUp()
        {
            ClearTestKeys();
        }

        [TearDown]
        /// <summary>
        /// Xóa tiến độ test sau khi chạy để không để lại dữ liệu.
        /// </summary>
        public void TearDown()
        {
            ClearTestKeys();
        }

        [Test]
        /// <summary>
        /// Game mới chỉ mở level 1; level 2 là level bị khóa.
        /// </summary>
        public void NewGame_OnlyLevelOneUnlocked_LevelTwoIsLocked()
        {
            Assert.AreEqual(1, LevelProgressService.GetHighestUnlockedLevel());
            Assert.IsTrue(LevelProgressService.IsUnlocked(1));
            Assert.IsFalse(LevelProgressService.IsUnlocked(2));
        }

        [Test]
        /// <summary>
        /// Hoàn thành level 1 phải mở khóa level 2 và không mở level 3.
        /// </summary>
        public void RecordCompletion_UnlocksNextLevelOnly()
        {
            LevelProgressService.RecordCompletion(1, 10, 2);

            Assert.IsTrue(LevelProgressService.IsUnlocked(2));
            Assert.IsFalse(LevelProgressService.IsUnlocked(3));
        }

        [Test]
        /// <summary>
        /// Lần chơi tệ hơn không được ghi đè best stars và best moves.
        /// </summary>
        public void RecordCompletion_KeepsBestStarsAndBestMoves()
        {
            LevelProgressService.RecordCompletion(1, 10, 2);
            LevelProgressService.RecordCompletion(1, 12, 1);

            Assert.AreEqual(2, LevelProgressService.GetBestStars(1));
            Assert.AreEqual(10, LevelProgressService.GetBestMoves(1));

            LevelProgressService.RecordCompletion(1, 9, 3);

            Assert.AreEqual(3, LevelProgressService.GetBestStars(1));
            Assert.AreEqual(9, LevelProgressService.GetBestMoves(1));
        }

        [Test]
        /// <summary>
        /// Số sao ngoài khoảng 1–3 phải được kẹp về khoảng hợp lệ.
        /// </summary>
        public void RecordCompletion_ClampsStarsToValidRange()
        {
            LevelProgressService.RecordCompletion(1, 5, 7);

            Assert.AreEqual(3, LevelProgressService.GetBestStars(1));
        }

        [Test]
        /// <summary>
        /// Hoàn thành level cũ không được làm giảm mức mở khóa đã có.
        /// </summary>
        public void RecordCompletion_ReplayingEarlierLevel_DoesNotLockProgress()
        {
            LevelProgressService.RecordCompletion(2, 10, 1);
            LevelProgressService.RecordCompletion(1, 10, 1);

            Assert.AreEqual(3, LevelProgressService.GetHighestUnlockedLevel());
        }

        [Test]
        /// <summary>
        /// Kết quả và mở khóa phải nằm trong PlayerPrefs để còn sau khi khởi động lại.
        /// Test đọc trực tiếp kho PlayerPrefs; kiểm tra restart thật nằm ở bước Play Mode thủ công.
        /// </summary>
        public void RecordCompletion_ValuesAreStoredInPlayerPrefs()
        {
            LevelProgressService.RecordCompletion(1, 7, 3);

            Assert.AreEqual(2, PlayerPrefs.GetInt(HighestUnlockedKey));
            Assert.AreEqual(3, PlayerPrefs.GetInt("ColorFactory.Level.1.BestStars"));
            Assert.AreEqual(7, PlayerPrefs.GetInt("ColorFactory.Level.1.BestMoves"));
        }

        /// <summary>
        /// Xóa đúng các key tiến độ của level 1–3 dùng trong test.
        /// </summary>
        private static void ClearTestKeys()
        {
            PlayerPrefs.DeleteKey(HighestUnlockedKey);

            for (int levelId = 1; levelId <= 3; levelId++)
            {
                PlayerPrefs.DeleteKey($"ColorFactory.Level.{levelId}.BestStars");
                PlayerPrefs.DeleteKey($"ColorFactory.Level.{levelId}.BestMoves");
            }

            PlayerPrefs.Save();
        }
    }
}
