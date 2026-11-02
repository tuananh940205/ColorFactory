using System;
using ColorFactory.Audio;
using ColorFactory.Core;
using ColorFactory.Levels;
using ColorFactory.UI;
using UnityEngine;

namespace ColorFactory.Gameplay
{
    /// <summary>
    /// Điều khiển lựa chọn khay và áp dụng nước đi vào BoardModel.
    /// </summary>
    public sealed class BoardController : MonoBehaviour
    {
        [SerializeField] private GameBootstrap gameBootstrap;
        [SerializeField] private BoardView boardView;
        [SerializeField] private LevelCompleteView levelCompleteView;

        // Ngăn người chơi tiếp tục di chuyển sau khi đã thắng.
        private bool levelCompleted;

        // -1 nghĩa là hiện chưa có khay nguồn nào được chọn.
        private int selectedTrayIndex = -1;
        private bool initialized;
        // Chặn input trong lúc toy đang bay giữa hai khay.
        private bool isAnimatingMove;
        
        /// <summary>
        /// Tổng số nước đi hợp lệ của level hiện tại.
        /// </summary>
        public int MoveCount { get; private set; }

        /// <summary>
        /// UI đăng ký event này để cập nhật số nước đi.
        /// </summary>
        public event Action<int> MoveCountChanged;

        /// <summary>
        /// Chờ BoardView dựng tray xong rồi mới nhận input.
        /// </summary>
        private void Start()
        {
            if (gameBootstrap == null)
                gameBootstrap = FindFirstObjectByType<GameBootstrap>();

            if (boardView == null)
                boardView = FindFirstObjectByType<BoardView>();

            if (gameBootstrap == null || boardView == null)
            {
                Debug.LogError("BoardController requires GameBootstrap and BoardView.", this);
                return;
            }

            if (boardView.IsBuilt)
                Initialize();
            else
                boardView.Built += HandleBoardBuilt;
        }

        /// <summary>
        /// Hủy đăng ký event để tránh callback vào object đã bị destroy.
        /// </summary>
        private void OnDestroy()
        {
            if (boardView == null)
                return;

            boardView.Built -= HandleBoardBuilt;
            boardView.TrayClicked -= HandleTrayClicked;
        }

        /// <summary>
        /// Nhận tín hiệu BoardView đã tạo đủ tray.
        /// </summary>
        private void HandleBoardBuilt(BoardView _)
        {
            Initialize();
        }

        /// <summary>
        /// Bắt đầu nhận click sau khi gameplay data và visual đều sẵn sàng.
        /// </summary>
        private void Initialize()
        {
            if (initialized)
                return;

            initialized = true;
            boardView.TrayClicked += HandleTrayClicked;
        }

        /// <summary>
        /// Xử lý quy tắc chọn nguồn, chọn đích, đổi nguồn và di chuyển hợp lệ.
        /// </summary>
        private void HandleTrayClicked(int clickedTrayIndex)
        {
            if (!initialized ||
                levelCompleted ||
                isAnimatingMove ||
                gameBootstrap.Board == null)
            {
                return;
            }

            if (selectedTrayIndex == -1)
            {
                // Không thể chọn một khay trống làm nguồn.
                if (gameBootstrap.Board.GetTray(clickedTrayIndex).Count > 0)
                    SelectTray(clickedTrayIndex);

                return;
            }

            if (selectedTrayIndex == clickedTrayIndex)
            {
                // Chạm lại vào khay đang chọn để bỏ chọn.
                ClearSelection();
                return;
            }

            if (gameBootstrap.Board.TryMove(selectedTrayIndex, clickedTrayIndex))
            {
                int fromTrayIndex = selectedTrayIndex;

                // Xóa highlight trước khi toy bắt đầu di chuyển.
                ClearSelection();

                MoveCount++;
                MoveCountChanged?.Invoke(MoveCount);

                // BoardModel đã đổi state, còn BoardView giờ animate instance hiện có.
                isAnimatingMove = true;

                bool levelIsCompleted = gameBootstrap.Board.IsCompleted();

                // Khi thắng, âm thanh win được phát sau khi toy đáp để không trùng với move.
                if (!levelIsCompleted)
                    Services.Audio?.Play(SfxType.Move);

                boardView.AnimateMove(
                    fromTrayIndex,
                    clickedTrayIndex,
                    () => HandleMoveAnimationCompleted(levelIsCompleted));
                return;
            }

            // Khi nước đi thất bại, cho phép đổi sang khay nguồn khác nếu nó không rỗng.
            if (gameBootstrap.Board.GetTray(clickedTrayIndex).Count > 0)
                SelectTray(clickedTrayIndex);
        }

        /// <summary>
        /// Hiển thị highlight cho khay nguồn hiện tại.
        /// </summary>
        private void SelectTray(int trayIndex)
        {
            ClearSelection();

            selectedTrayIndex = trayIndex;

            Services.Audio?.Play(SfxType.Tap);

            if (boardView.TryGetTrayView(trayIndex, out TrayView trayView))
                trayView.SetSelected(true);
        }

        /// <summary>
        /// Xóa highlight và trạng thái chọn hiện có.
        /// </summary>
        private void ClearSelection()
        {
            if (selectedTrayIndex >= 0 &&
                boardView.TryGetTrayView(selectedTrayIndex, out TrayView trayView))
            {
                trayView.SetSelected(false);
            }

            selectedTrayIndex = -1;
        }
        
        /// <summary>
        /// Chỉ cho Undo khi board có lịch sử và không có animation đang chạy.
        /// </summary>
        public bool CanUndo =>
            !levelCompleted &&
            !isAnimatingMove &&
            gameBootstrap != null &&
            gameBootstrap.Board != null &&
            gameBootstrap.Board.CanUndo;

        /// <summary>
        /// Hoàn tác nước đi gần nhất và animate toy bay ngược về khay nguồn cũ.
        /// </summary>
        public bool TryUndoLastMove()
        {
            if (!initialized || !CanUndo)
                return false;

            ClearSelection();

            if (!gameBootstrap.Board.TryUndo(out BoardMove undoneMove))
                return false;

            MoveCount = Mathf.Max(0, MoveCount - 1);
            MoveCountChanged?.Invoke(MoveCount);

            // Khi Undo, toy di chuyển ngược từ khay đích cũ về khay nguồn cũ.
            isAnimatingMove = true;

            Services.Audio?.Play(SfxType.Undo);

            boardView.AnimateMove(
                undoneMove.To,
                undoneMove.From,
                HandleUndoAnimationCompleted);

            return true;
        }
        
        /// <summary>
        /// Tìm và hiển thị một nước đi hợp lệ mà không tự di chuyển toy.
        /// </summary>
        public void RequestHint()
        {
            if (!initialized || levelCompleted || gameBootstrap.Board == null)
                return;

            ClearSelection();

            if (HintSolver.TryFindHint(gameBootstrap.Board, out MoveHint hint))
            {
                Services.Audio?.Play(SfxType.Hint);
                boardView.ShowHint(hint.From, hint.To);
            }
        }
        
        /// <summary>
        /// Mở lại input sau animation và hiện completion panel nếu level đã hoàn thành.
        /// </summary>
        private void HandleMoveAnimationCompleted(bool levelIsCompleted)
        {
            isAnimatingMove = false;

            // Chỉ hiện kết quả khi trạng thái board sau nước đi thật sự là hoàn thành.
            if (!levelIsCompleted)
                return;

            levelCompleted = true;

            Services.Audio?.Play(SfxType.Win);
            boardView.PlayCompletionFeedback();
            
            int stars = LevelResultCalculator.CalculateStars(
                gameBootstrap.CurrentLevel,
                MoveCount);

            LevelProgressService.RecordCompletion(
                gameBootstrap.CurrentLevel.levelId,
                MoveCount,
                stars);

            levelCompleteView.Show(MoveCount, stars);
        }
        
        /// <summary>
        /// Mở lại input sau khi animation Undo kết thúc.
        /// </summary>
        private void HandleUndoAnimationCompleted()
        {
            isAnimatingMove = false;
        }
    }
}
