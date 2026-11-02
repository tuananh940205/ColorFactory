using System;
using ColorFactory.Core;
using ColorFactory.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ColorFactory.UI
{
    /// <summary>
    /// Hiển thị thông tin gameplay ở đầu màn hình:
    /// số level, số nước đi và nút chơi lại.
    /// </summary>
    public sealed class GameplayHeaderView : MonoBehaviour
    {
        [SerializeField] private GameBootstrap gameBootstrap;
        [SerializeField] private BoardController boardController;

        [Header("UI References")]
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text movesText;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button undoButton;
        [SerializeField] private Button hintButton;

        private bool isBound;

        /// <summary>
        /// Tìm các service cần thiết, đăng ký nút restart
        /// và chờ GameBootstrap load level xong.
        /// </summary>
        private void Start()
        {
            gameBootstrap ??= FindFirstObjectByType<GameBootstrap>();
            boardController ??= FindFirstObjectByType<BoardController>();

            if (gameBootstrap == null || boardController == null)
            {
                Debug.LogError(
                    "GameplayHeaderView requires GameBootstrap and BoardController.",
                    this);
                return;
            }

            restartButton.onClick.AddListener(RestartCurrentLevel);
            undoButton.onClick.AddListener(UndoLastMove);
            hintButton.onClick.AddListener(RequestHint);

            if (gameBootstrap.IsReady)
                BindToGameplay();
            else
                gameBootstrap.Ready += HandleBootstrapReady;
        }

        /// <summary>
        /// Hủy event và listener để object không nhận callback sau khi bị destroy.
        /// </summary>
        private void OnDestroy()
        {
            if (gameBootstrap != null)
                gameBootstrap.Ready -= HandleBootstrapReady;

            if (boardController != null)
                boardController.MoveCountChanged -= UpdateMovesText;

            if (restartButton != null)
                restartButton.onClick.RemoveListener(RestartCurrentLevel);
            
            if (undoButton != null)
                undoButton.onClick.RemoveListener(UndoLastMove);
            
            if (hintButton != null)
                hintButton.onClick.RemoveListener(RequestHint);
        }

        /// <summary>
        /// Nhận thông báo Addressables và BoardModel đã sẵn sàng.
        /// </summary>
        private void HandleBootstrapReady(GameBootstrap _)
        {
            BindToGameplay();
        }

        /// <summary>
        /// Gán dữ liệu level hiện tại và lắng nghe số nước đi thay đổi.
        /// </summary>
        private void BindToGameplay()
        {
            if (isBound || gameBootstrap.CurrentLevel == null)
                return;

            isBound = true;

            levelText.text = $"LEVEL {gameBootstrap.CurrentLevel.levelId:00}";
            boardController.MoveCountChanged += UpdateMovesText;
            UpdateMovesText(boardController.MoveCount);
        }

        /// <summary>
        /// Cập nhật text sau mỗi nước đi hợp lệ.
        /// </summary>
        private void UpdateMovesText(int moveCount)
        {
            movesText.text = $"MOVES: {moveCount}";
            // Undo chỉ bật khi có ít nhất một nước đi hợp lệ trong lịch sử.
            undoButton.interactable = boardController.CanUndo;
        }

        /// <summary>
        /// Tải lại scene hiện tại để người chơi bắt đầu level từ đầu.
        /// </summary>
        private void RestartCurrentLevel()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        
        /// <summary>
        /// Gọi BoardController hoàn tác nước đi gần nhất.
        /// UI tự cập nhật qua MoveCountChanged.
        /// </summary>
        private void UndoLastMove()
        {
            boardController.TryUndoLastMove();
        }
        
        /// <summary>
        /// Yêu cầu BoardController hiển thị nước đi được HintSolver đề xuất.
        /// </summary>
        private void RequestHint()
        {
            boardController.RequestHint();
        }
    }
}