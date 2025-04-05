# MultiRateLimiter

## TODOs

- folder structure is flat, could be cleaner (e.g. RateLimiting/, Config/, etc.)
- delay while waiting is a fixed 50ms, could calculate exact wait time instead
- sliding window works fine, but hybrid (sliding + fixed) would be more efficient for long windows like 24h
- no unit tests, just used a console app — can add tests if needed
