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
                    new RateLimitConfig { Limit = 5, Window = TimeSpan.FromSeconds(11) }
                });

            var tasks = new List<Task>();
            for (int i = 1; i <= 100; i++)
            {
                tasks.Add(limiter.Perform(i));
            }

            //run all at once to try and find the race visually
            Console.WriteLine("all tasks called");
            await Task.WhenAll(tasks);

            Console.ReadLine();
        }
        static async Task SomeTask(int number)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Task {number} started");
            Random rdn = new Random();
            await Task.Delay(rdn.Next(100,600));
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Task {number} finished");
        }
    }
}
