namespace MultiRateLimiter.Lib
{
    public class RateLimitRule
    {
        public int Limit { get; set; }
        public TimeSpan Window { get; set; }
        public Queue<DateTime> TimeStamps { get; set; }
    }
}
