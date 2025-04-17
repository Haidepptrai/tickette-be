namespace Tickette.Infrastructure.Persistence.Redis;

public static class RedisLuaScripts
{
    public static readonly string ReserveTicket = Load("reserve_ticket.lua");
    public static readonly string ReserveTicketWithSeats = Load("reserve_ticket_with_seats.lua");

    private static string Load(string fileName)
    {
        var basePath = AppContext.BaseDirectory;
        var scriptPath = Path.Combine(AppContext.BaseDirectory, "Persistence", "Redis", "Scripts", fileName);

        if (!File.Exists(scriptPath))
            throw new FileNotFoundException($"Lua script not found at path: {scriptPath}");

        return File.ReadAllText(scriptPath);
    }
}
