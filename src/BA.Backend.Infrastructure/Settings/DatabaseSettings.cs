namespace BA.Backend.Infrastructure.Settings;

public class DatabaseSettings
{
    public string ConnectionString { get; set; } = null!;
    public string Provider { get; set; } = "SqlServer";
}
