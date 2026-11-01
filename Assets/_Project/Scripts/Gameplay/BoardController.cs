using ColorFactory.Core;
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
            if (!initialized || levelCompleted || gameBootstrap.Board == null)
                return;

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
                // Model thay đổi trước, sau đó View render lại từ model.
                ClearSelection();
                boardView.Render(gameBootstrap.Board);

                if (gameBootstrap.Board.IsCompleted())
                {
                    // Khóa input trước khi hiện panel hoàn thành.
                    levelCompleted = true;
                    Debug.Log("Level completed!");

                    if (levelCompleteView != null)
                        levelCompleteView.Show();
                }

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
    }
}