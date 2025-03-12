namespace LoyaltySystem.Admin.API.Settings;

public class PostgresqlSettings 
{
    public string ConnectionString { get; set; } = string.Empty;
    public bool EnableSensitiveDataLogging { get; set; } = false;
    public int CommandTimeout { get; set; } = 30;
}
