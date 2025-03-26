namespace Tickette.Infrastructure.Persistence;

public class RedisSettings
{
    // Connection properties
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 6379;
    public string ConnectionString { get; set; } = string.Empty;
    public string InstanceName { get; set; } = "Tickette:";

    // Timeout settings
    public int ConnectTimeout { get; set; } = 5000;
    public int SyncTimeout { get; set; } = 5000;
    public int ResponseTimeout { get; set; } = 10000;

    // Retry settings
    public int ConnectRetry { get; set; } = 3;
    public bool AbortOnConnectFail { get; set; } = false;

    // Security settings
    public bool Ssl { get; set; } = false;
    public string Password { get; set; } = string.Empty;
    public string User { get; set; } = string.Empty;
    public bool AllowAdmin { get; set; } = false;

    // Database settings
    public int DefaultDatabase { get; set; } = 0;
}