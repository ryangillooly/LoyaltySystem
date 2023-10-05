namespace LoyaltySystem.Core.Settings;

public class DynamoDbSettings
{
    public string TableName { get; set; }
    public string BusinessUserListGsi { get; set; }
    public string EmailGsi { get; set; }
    public string BusinessLoyaltyListGsi { get; set; }
}