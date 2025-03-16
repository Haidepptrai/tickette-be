namespace Tickette.Infrastructure.Persistence;

public class RedisSettings
{
    // Connection properties
    public string ConnectionString { get; set; } = string.Empty;
    public string InstanceName { get; set; } = "Tickette:";

    // Timeout settings
    public int ConnectTimeout { get; set; } = 5000;
    public int SyncTimeout { get; set; } = 5000;
    public int ResponseTimeout { get; set; } = 10000;

    // Retry settings
    public int ConnectRetry { get; set; } = 3;
    public bool AbortOnConnectFail { get; set; } = false;

    // Cache durations
    public int DefaultExpirationMinutes { get; set; } = 15;
    public int TicketCacheExpirationMinutes { get; set; } = 15;
    public int AgentCacheExpirationMinutes { get; set; } = 15;

    // Security settings
    public bool Ssl { get; set; } = false;
    public string Password { get; set; } = string.Empty;
    public string User { get; set; } = string.Empty;
    public bool AllowAdmin { get; set; } = false;

    // Database settings
    public int DefaultDatabase { get; set; } = 0;

    // Computed properties for convenience
    public TimeSpan DefaultExpiration => TimeSpan.FromMinutes(DefaultExpirationMinutes);
    public TimeSpan TicketCacheExpiration => TimeSpan.FromMinutes(TicketCacheExpirationMinutes);
    public TimeSpan AgentCacheExpiration => TimeSpan.FromMinutes(AgentCacheExpirationMinutes);
}