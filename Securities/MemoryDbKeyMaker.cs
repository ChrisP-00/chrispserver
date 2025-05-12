namespace chrispserver.Securities;

public class MemoryDbKeyMaker
{
    public static string MakeUserLockKey(string userId) => $"lock:{userId}";

}
