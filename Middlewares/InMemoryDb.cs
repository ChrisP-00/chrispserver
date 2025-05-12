namespace chrispserver.Middlewares;

public class InMemoryDb : IMemoryDb
{
    private readonly Dictionary<string, AuthUser> _userStore = new();
    private readonly HashSet<string> _locks = new();

    public Task<(bool, AuthUser?)> GetUserAsync(string id)
    {
        return Task.FromResult(_userStore.TryGetValue(id, out var user)
            ? (true, user) : (false, null));
    }

    public Task<bool> SetUserReqLockAsync(string token)
    {
        lock(_locks)
        {
            if(_locks.Contains(token))
            {
                return Task.FromResult(false);
            }

            _locks.Add(token);
            return Task.FromResult(true);
        }
    }

    public Task DelUserReqLockAsync(string lockKey)
    {
        lock (_locks)
        {
            _locks.Remove(lockKey);
        }

        return Task.CompletedTask;
    }

    public void AddUser(AuthUser user) => _userStore[user.MemberId] = user;

    public Task<bool> CheckTokenMatchAsync(string id, string token)
    {
        var ok = _userStore.TryGetValue(id, out var user) && user.AuthToken == token;
        return Task.FromResult(ok);
    }

    public Task<AuthUser?> GetUserDataAsync(string memberId, string token)
    {
        var ok = _userStore.TryGetValue(memberId, out var user);
        return Task.FromResult(ok && user != null && user.AuthToken == token ? user : null);
    }

    public Task<AuthUser?> GetGuestDataAsync(string deviceId, string token)
    {
        var ok = _userStore.TryGetValue(deviceId, out var user);
        return Task.FromResult(ok && user != null && user.AuthToken == token ? user : null);
    }

    public Task<bool> CheckUserTokenMatchAsync(string memberId, string token)
    {
        return Task.FromResult(
            _userStore.TryGetValue(memberId, out var user) &&
            user.AuthToken == token
        );
    }

    public Task<bool> CheckGuestTokenMatchAsync(string deviceId, string token)
    {
        return Task.FromResult(
            _userStore.TryGetValue(deviceId, out var user) &&
            user.AuthToken == token
        );
    }
}
