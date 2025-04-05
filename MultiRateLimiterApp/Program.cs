using MultiRateLimiter.Lib;

namespace MultiRateLimiterApp
{
    internal class Program
    {
        static async Task Main()
        {
            var limiter = new RateLimiter<int>(
                SomeTask,
                new List<RateLimitConfig>
                {
                    new RateLimitConfig { Limit = 1, Window = TimeSpan.FromSeconds(1) },
                    new RateLimitConfig { Limit = 5, Window = TimeSpan.FromSeconds(10) }
                });

            for (int i = 1; i <= 10; i++)
            {
                _ = limiter.Perform(i);
                await Task.Delay(50);
            }


            Console.WriteLine("all tasks added");
            Console.ReadLine();
        }
        static Task SomeTask(int number)
        {
            Console.WriteLine($"[{DateTime.Now.ToString("HH:mm:ss.fff")}] Task {number} executed");
            return Task.CompletedTask;
        }
    }
}
