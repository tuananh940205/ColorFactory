using System;
using System.Collections.Generic;
using ColorFactory.Core;
using ColorFactory.Gameplay;
using ColorFactory.Levels;
using NUnit.Framework;

namespace ColorFactory.Tests
{
    /// <summary>
    /// Kiểm thử luật di chuyển, Undo và điều kiện hoàn thành của BoardModel,
    /// gồm các failure scenario: nước đi sai, undo khi chưa có lịch sử, dữ liệu khay lỗi.
    /// </summary>
    public sealed class BoardModelTests
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
        /// Chuyển toy vào cùng khay phải bị từ chối và board không đổi.
        /// </summary>
        public void TryMove_SameTray_ReturnsFalse()
        {
            var board = new BoardModel(TestLevelFactory.CreateValid());

            Assert.IsFalse(board.TryMove(0, 0));
            Assert.AreEqual(2, board.GetTray(0).Count);
            Assert.IsFalse(board.CanUndo);
        }

        [Test]
        /// <summary>
        /// Chỉ số khay ngoài phạm vi phải bị từ chối mà không ném exception.
        /// </summary>
        public void TryMove_OutOfRangeIndex_ReturnsFalse()
        {
            var board = new BoardModel(TestLevelFactory.CreateValid());

            Assert.IsFalse(board.CanMove(-1, 0));
            Assert.IsFalse(board.CanMove(0, 99));
            Assert.IsFalse(board.TryMove(99, 0));
        }

        [Test]
        /// <summary>
        /// Lấy toy từ khay trống phải bị từ chối.
        /// </summary>
        public void TryMove_FromEmptyTray_ReturnsFalse()
        {
            var board = new BoardModel(TestLevelFactory.CreateValid());

            Assert.IsFalse(board.TryMove(2, 0));
        }

        [Test]
        /// <summary>
        /// Đặt toy vào khay đã đầy phải bị từ chối.
        /// </summary>
        public void TryMove_OntoFullTray_ReturnsFalse()
        {
            LevelData level = TestLevelFactory.Create(
                1,
                2,
                new[] { ToyColor.Red, ToyColor.Red },
                new[] { ToyColor.Red, ToyColor.Red });
            var board = new BoardModel(level);

            Assert.IsFalse(board.TryMove(0, 1));
        }

        [Test]
        /// <summary>
        /// Màu đỉnh khác nhau phải bị từ chối và cả hai khay giữ nguyên.
        /// </summary>
        public void TryMove_ColorMismatch_ReturnsFalse_AndLeavesBoardUnchanged()
        {
            var board = new BoardModel(TestLevelFactory.CreateValid());

            // Đỉnh khay 0 là Blue, đỉnh khay 1 là Red.
            Assert.IsFalse(board.TryMove(0, 1));

            CollectionAssert.AreEqual(
                new[] { ToyColor.Red, ToyColor.Blue },
                board.GetTray(0));
            CollectionAssert.AreEqual(
                new[] { ToyColor.Blue, ToyColor.Red },
                board.GetTray(1));
            Assert.IsFalse(board.CanUndo);
        }

        [Test]
        /// <summary>
        /// Undo khi chưa có nước đi nào phải trả false và không ném exception.
        /// </summary>
        public void TryUndo_EmptyHistory_ReturnsFalse()
        {
            var board = new BoardModel(TestLevelFactory.CreateValid());

            Assert.IsFalse(board.TryUndo());
            Assert.IsFalse(board.TryUndo(out BoardMove move));
            Assert.AreEqual(default(BoardMove), move);
        }

        [Test]
        /// <summary>
        /// Một nước đi hợp lệ rồi Undo phải khôi phục đúng board và lịch sử.
        /// </summary>
        public void TryMove_ThenUndo_RestoresBoardAndHistory()
        {
            var board = new BoardModel(TestLevelFactory.CreateValid());

            Assert.IsTrue(board.TryMove(0, 2));
            CollectionAssert.AreEqual(new[] { ToyColor.Red }, board.GetTray(0));
            CollectionAssert.AreEqual(new[] { ToyColor.Blue }, board.GetTray(2));
            Assert.IsTrue(board.CanUndo);

            Assert.IsTrue(board.TryUndo(out BoardMove undone));
            Assert.AreEqual(0, undone.From);
            Assert.AreEqual(2, undone.To);
            Assert.AreEqual(ToyColor.Blue, undone.Color);

            CollectionAssert.AreEqual(
                new[] { ToyColor.Red, ToyColor.Blue },
                board.GetTray(0));
            Assert.AreEqual(0, board.GetTray(2).Count);
            Assert.IsFalse(board.CanUndo);
            Assert.IsFalse(board.TryUndo());
        }

        [Test]
        /// <summary>
        /// Board đã sắp xếp đúng phải được nhận là hoàn thành, board chưa xong thì không.
        /// </summary>
        public void IsCompleted_SolvedAndUnsolvedBoards()
        {
            LevelData solved = TestLevelFactory.Create(
                1,
                2,
                new[] { ToyColor.Red, ToyColor.Red },
                new[] { ToyColor.Blue, ToyColor.Blue },
                new ToyColor[0]);

            Assert.IsTrue(new BoardModel(solved).IsCompleted());
            Assert.IsFalse(new BoardModel(TestLevelFactory.CreateValid()).IsCompleted());
        }

        [Test]
        /// <summary>
        /// Khay chứa nhiều toy hơn sức chứa phải làm BoardModel từ chối khởi tạo.
        /// </summary>
        public void Constructor_TrayOverCapacity_Throws()
        {
            LevelData level = TestLevelFactory.Create(
                1,
                2,
                new[] { ToyColor.Red, ToyColor.Red, ToyColor.Red });

            Assert.Throws<ArgumentException>(() => new BoardModel(level));
        }

        [Test]
        /// <summary>
        /// Level null phải bị từ chối bằng ArgumentNullException.
        /// </summary>
        public void Constructor_NullLevel_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new BoardModel(null));
        }

        [Test]
        /// <summary>
        /// Sức chứa khay không dương phải bị từ chối khi khởi tạo.
        /// </summary>
        public void Constructor_NonPositiveCapacity_Throws()
        {
            LevelData level = TestLevelFactory.Create(1, 0);

            Assert.Throws<ArgumentException>(() => new BoardModel(level));
        }
    }
}
