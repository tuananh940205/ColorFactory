using System;
using ColorFactory.Core;
using UnityEngine;

namespace ColorFactory.Economy
{
    /// <summary>
    /// Lưu số coin local, xử lý giao dịch an toàn và phát event khi số dư đổi.
    /// </summary>
    [DefaultExecutionOrder(-950)]
    public sealed class WalletService : MonoBehaviour
    {
        private const string CoinPrefsKey = "wallet.coins";

        [SerializeField, Min(0)] private int initialCoins = 30;
        [SerializeField, Min(1)] private int hintCost = 3;
        [SerializeField, Min(1)] private int undoCost = 2;
        [SerializeField, Min(0)] private int completionReward = 10;

        private int coins;

        public int Coins => coins;
        public int HintCost => hintCost;
        public int UndoCost => undoCost;
        public int CompletionReward => completionReward;

        /// <summary>
        /// Báo số dư mới để HUD đồng bộ mà không phụ thuộc trực tiếp WalletService.
        /// </summary>
        public event Action<int> BalanceChanged;

        /// <summary>
        /// Đăng ký ví duy nhất, đọc số dư cũ hoặc tạo số dư mock lần chạy đầu.
        /// </summary>
        private void Awake()
        {
            if (Services.Wallet != null && Services.Wallet != this)
            {
                Destroy(gameObject);
                return;
            }

            Services.Register(this);
            DontDestroyOnLoad(gameObject);

            coins = Mathf.Max(
                0,
                PlayerPrefs.GetInt(CoinPrefsKey, initialCoins));
            SaveCoins();
        }

        /// <summary>
        /// Gỡ ví khỏi service locator khi object persistent gốc bị hủy.
        /// </summary>
        private void OnDestroy()
        {
            Services.Unregister(this);
        }

        /// <summary>
        /// Kiểm tra trước giao dịch để UI có thể khóa booster khi thiếu coin.
        /// </summary>
        public bool CanAfford(int cost)
        {
            return cost >= 0 && coins >= cost;
        }

        /// <summary>
        /// Trừ coin nếu đủ tiền; không bao giờ cho phép số dư âm.
        /// </summary>
        public WalletTransactionResult TrySpend(int cost)
        {
            if (cost <= 0 || !CanAfford(cost))
                return new WalletTransactionResult(false, coins, 0);

            coins -= cost;
            SaveCoins();
            BalanceChanged?.Invoke(coins);

            return new WalletTransactionResult(true, coins, -cost);
        }

        /// <summary>
        /// Cộng reward dương vào ví và lưu ngay để số dư còn sau khi restart.
        /// </summary>
        public WalletTransactionResult Reward(int reward)
        {
            if (reward <= 0)
                return new WalletTransactionResult(false, coins, 0);

            coins += reward;
            SaveCoins();
            BalanceChanged?.Invoke(coins);

            return new WalletTransactionResult(true, coins, reward);
        }

        /// <summary>
        /// Ghi số dư đã được clamp vào PlayerPrefs và flush xuống thiết bị.
        /// </summary>
        private void SaveCoins()
        {
            PlayerPrefs.SetInt(CoinPrefsKey, Mathf.Max(0, coins));
            PlayerPrefs.Save();
        }
    }
}
