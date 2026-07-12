using System.Collections.Concurrent;

namespace SchoolConnect.Shared.Services;

public sealed class LoginAttemptGuard
{
    public const int MaximumFailures = 5;
    private static readonly TimeSpan FailureWindow = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(10);
    private readonly ConcurrentDictionary<string, AttemptState> attempts = new(StringComparer.OrdinalIgnoreCase);

    public bool IsAllowed(string role)
    {
        if (!attempts.TryGetValue(role, out var state))
        {
            return true;
        }

        lock (state)
        {
            var now = DateTimeOffset.UtcNow;
            if (state.LockedUntil > now)
            {
                return false;
            }

            if (now - state.WindowStarted > FailureWindow)
            {
                attempts.TryRemove(role, out _);
            }

            return true;
        }
    }

    public void RecordFailure(string role)
    {
        var now = DateTimeOffset.UtcNow;
        var state = attempts.GetOrAdd(role, _ => new AttemptState(now));
        lock (state)
        {
            if (now - state.WindowStarted > FailureWindow)
            {
                state.WindowStarted = now;
                state.Failures = 0;
                state.LockedUntil = null;
            }

            state.Failures++;
            if (state.Failures >= MaximumFailures)
            {
                state.LockedUntil = now.Add(LockoutDuration);
            }
        }
    }

    public void RecordSuccess(string role) => attempts.TryRemove(role, out _);

    private sealed class AttemptState(DateTimeOffset windowStarted)
    {
        public DateTimeOffset WindowStarted { get; set; } = windowStarted;
        public int Failures { get; set; }
        public DateTimeOffset? LockedUntil { get; set; }
    }
}
