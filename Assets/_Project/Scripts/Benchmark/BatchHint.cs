namespace ColorFactory.Benchmark
{
    /// <summary>
    /// Nước đi tốt nhất của một bàn trong batch, kèm điểm để so sánh baseline và job.
    /// </summary>
    public readonly struct BatchHint
    {
        public bool Found { get; }
        public int From { get; }
        public int To { get; }
        public int Score { get; }

        /// <summary>
        /// Khởi tạo kết quả; khi không tìm thấy nước đi thì Found là false.
        /// </summary>
        public BatchHint(bool found, int from, int to, int score)
        {
            Found = found;
            From = from;
            To = to;
            Score = score;
        }
    }
}
