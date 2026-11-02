using ColorFactory.Analytics;
using ColorFactory.Audio;
using System;
using ColorFactory.Core;
using ColorFactory.Economy;
using ColorFactory.Gameplay;
using TMPro;
using UnityEngine;
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
        private WalletService walletService;

        /// <summary>
        /// Tìm các service cần thiết, đăng ký nút restart
        /// và chờ GameBootstrap load level xong.
        /// </summary>
        private void Start()
        {
            gameBootstrap ??= FindFirstObjectByType<GameBootstrap>();
            boardController ??= FindFirstObjectByType<BoardController>();
            walletService = Services.Wallet;

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

            if (walletService != null)
                walletService.BalanceChanged -= UpdateWalletText;

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

            if (walletService != null)
                walletService.BalanceChanged += UpdateWalletText;
            UpdateMovesText(boardController.MoveCount);
            UpdateWalletText(walletService?.Coins ?? 0);
            UpdateBoosterLabels();
        }

        /// <summary>
        /// Cập nhật text sau mỗi nước đi hợp lệ.
        /// </summary>
        private void UpdateMovesText(int moveCount)
        {
            movesText.text = $"MOVES: {moveCount}\nCOINS: {walletService?.Coins ?? 0}";
            // Undo chỉ bật khi có ít nhất một nước đi hợp lệ trong lịch sử.
            undoButton.interactable = boardController.CanUndo &&
                                      walletService != null &&
                                      walletService.CanAfford(walletService.UndoCost);
        }

        /// <summary>
        /// Cập nhật coin HUD và trạng thái booster sau mỗi giao dịch ví.
        /// </summary>
        private void UpdateWalletText(int coins)
        {
            movesText.text = $"MOVES: {boardController.MoveCount}\nCOINS: {coins}";
            undoButton.interactable = boardController.CanUndo &&
                                      walletService != null &&
                                      walletService.CanAfford(walletService.UndoCost);
            hintButton.interactable = boardController.CanRequestHint &&
                                      walletService != null &&
                                      walletService.CanAfford(walletService.HintCost);
        }

        /// <summary>
        /// Hiển thị coin cost ngay trên nhãn button để người chơi biết trước khi dùng.
        /// </summary>
        private void UpdateBoosterLabels()
        {
            if (walletService == null)
                return;

            TMP_Text undoLabel = undoButton.GetComponentInChildren<TMP_Text>();
            TMP_Text hintLabel = hintButton.GetComponentInChildren<TMP_Text>();

            if (undoLabel != null)
                undoLabel.text = $"UNDO\n{walletService.UndoCost} COINS";

            if (hintLabel != null)
                hintLabel.text = $"HINT\n{walletService.HintCost} COINS";
        }

        /// <summary>
        /// Tải lại scene hiện tại để người chơi bắt đầu level từ đầu.
        /// </summary>
        private void RestartCurrentLevel()
        {
            int levelId = gameBootstrap.CurrentLevelIdOrDefault;

            Services.SceneFader?.TryReloadActiveScene(() =>
            {
                Services.Audio?.Play(SfxType.Button);
                Services.Analytics?.Track(new RestartRequestedEvent(levelId));
            });
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
