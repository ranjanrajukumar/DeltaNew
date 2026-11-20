using System.Collections.Concurrent;

namespace SchoolERP.Application.Modules.Authentication;

/// <summary>
/// Slows password guessing (the Node app's middleware/loginLimiter.js): after too many failed sign-ins for the same email from
/// the same address (or too many from one address overall) further attempts are refused for a while. In memory, so it resets
/// when the server restarts, which is fine for a single-instance school deployment. Register as a singleton.
/// </summary>
public sealed class LoginLimiter
{
    private static readonly TimeSpan Window = TimeSpan.FromMinutes(15);
    private const int MaxPerAccount = 8;
    private const int MaxPerAddress = 40;

    private sealed class Entry
    {
        public int Count;
        public DateTime ResetAt;
    }

    private readonly ConcurrentDictionary<string, Entry> _failures = new();

    private static (string Account, string Address) Keys(string? ip, string? email)
    {
        var address = string.IsNullOrEmpty(ip) ? "unknown" : ip;
        return ($"a|{address}|{(email ?? "").ToLowerInvariant()}", $"i|{address}");
    }

    private int BlockedFor(string key, int max)
    {
        var now = DateTime.UtcNow;
        return _failures.TryGetValue(key, out var e) && e.ResetAt > now && e.Count >= max
            ? (int)Math.Ceiling((e.ResetAt - now).TotalSeconds)
            : 0;
    }

    private void Bump(string key)
    {
        var now = DateTime.UtcNow;
        _failures.AddOrUpdate(key,
            _ => new Entry { Count = 1, ResetAt = now + Window },
            (_, e) =>
            {
                lock (e)
                {
                    if (e.ResetAt <= now) (e.Count, e.ResetAt) = (1, now + Window);
                    else e.Count++;
                }
                return e;
            });
    }

    /// <summary>Seconds to wait before another attempt, or 0 when signing in is allowed.</summary>
    public int SecondsToWait(string? ip, string? email)
    {
        var (account, address) = Keys(ip, email);
        return Math.Max(BlockedFor(account, MaxPerAccount), BlockedFor(address, MaxPerAddress));
    }

    public void RecordFailure(string? ip, string? email)
    {
        var (account, address) = Keys(ip, email);
        Bump(account);
        Bump(address);
        if (_failures.Count > 5000)
        {
            var now = DateTime.UtcNow;
            foreach (var (key, e) in _failures)
                if (e.ResetAt <= now) _failures.TryRemove(key, out _);
        }
    }

    public void RecordSuccess(string? ip, string? email) => _failures.TryRemove(Keys(ip, email).Account, out _);
}
