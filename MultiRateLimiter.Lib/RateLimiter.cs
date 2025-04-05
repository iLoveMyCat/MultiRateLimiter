namespace MultiRateLimiter.Lib
{
    public class RateLimiter<TArg>
    {
        private readonly Func<TArg, Task> _action;
        private readonly List<RateLimitRule> _rules;
        private readonly object _lockObj = new();

        public RateLimiter(Func<TArg, Task> action, List<RateLimitConfig> configs)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action));
            _rules = new List<RateLimitRule>();

            foreach (var config in configs)
            {
                _rules.Add(new RateLimitRule() { Limit = config.Limit, Window = config.Window, TimeStamps = new Queue<DateTime>() } );
            }
        }

        public async Task Perform(TArg argument)
        {
            // delays execution until it can honor the rate limits
            while (true)
            {
                DateTime now = DateTime.UtcNow;
                bool allowed = false;

                lock (_lockObj)
                {
                    SlideWindow(now);
                    
                    if (ComplyWithAllRules(now))
                    {
                        foreach (var rule in _rules)
                        {
                            rule.TimeStamps.Enqueue(now);
                        }
                        allowed = true;
                    }
                }

                // it can honor the rate limits
                if (allowed)
                    break;

                //Console.WriteLine($"Task {argument} delayed");
                await Task.Delay(50); 
            }

            await _action(argument);
        }


        private void SlideWindow(DateTime now)
        {
            // remove irelevant timestamps for each rule
            foreach (var rule in _rules)
            {
                while (rule.TimeStamps.Count > 0 && rule.TimeStamps.Peek() <= now - rule.Window)
                {
                    rule.TimeStamps.Dequeue();
                }
            }
        }

        private bool ComplyWithAllRules(DateTime now)
        {
            foreach (var rule in _rules)
            {
                if (rule.TimeStamps.Count >= rule.Limit)
                    return false;
            }
            return true;
        }

    }
}
