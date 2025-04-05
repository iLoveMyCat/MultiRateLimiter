namespace MultiRateLimiter.Lib
{
    public class RateLimitConfig
    {
        public int Limit { get; set; }
        public TimeSpan Window { get; set; }
    }
}
