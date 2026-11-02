namespace ColorFactory.Economy
{
    /// <summary>
    /// Kết quả bất biến của một giao dịch coin để caller biết số dư sau giao dịch.
    /// </summary>
    public readonly struct WalletTransactionResult
    {
        public bool IsSuccessful { get; }
        public int Balance { get; }
        public int Delta { get; }

        /// <summary>
        /// Khởi tạo kết quả giao dịch với trạng thái, số dư cuối và số coin thay đổi.
        /// </summary>
        public WalletTransactionResult(bool isSuccessful, int balance, int delta)
        {
            IsSuccessful = isSuccessful;
            Balance = balance;
            Delta = delta;
        }
    }
}
