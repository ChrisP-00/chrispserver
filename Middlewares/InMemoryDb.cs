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

    /// <summary>
    /// 테스트 시 유저 추가 용
    /// </summary>
    public void AddUser(AuthUser user) => _userStore[user.MemberId] = user;
}
