namespace MultiRateLimiter.Lib
{
    public class RateLimiter<TArg>
    {
        private readonly Func<TArg, Task> _action;
        private readonly List<RateLimitRule> _rules;
        private readonly object _lockObj = new();
        private readonly Queue<TArg> _pendingQueue = new();
        private bool _isProcessing = false;

        public RateLimiter(Func<TArg, Task> action, List<RateLimitConfig> configs)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action));
            _rules = new List<RateLimitRule>();

            foreach (var config in configs)
            {
                _rules.Add(new RateLimitRule() { Limit = config.Limit, Window = config.Window, TimeStamps = new Queue<DateTime>() } );
            }
        }

        public Task Perform(TArg argument)
        {
            // perform calls now starts the queue procecsssing
            lock (_lockObj)
            {
                _pendingQueue.Enqueue(argument);

                if (!_isProcessing)
                {
                    _isProcessing = true;
                    _ = Task.Run(ProcessQueue);
                }
            }

            return Task.CompletedTask;
        }
        private async Task ProcessQueue()
        {
            //run only while there are items in the queue
            while (true)
            {
                TArg nextItem;
                lock (_lockObj)
                {
                    if (_pendingQueue.Count == 0)
                    {
                        _isProcessing = false;
                        return;
                    }

                    nextItem = _pendingQueue.Dequeue(); 
                }

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
                    if (allowed)
                        break;

                    await Task.Delay(10);
                }

                await _action(nextItem);
            }
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
