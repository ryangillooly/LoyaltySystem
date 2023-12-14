namespace LoyaltySystem.Core.Models.Persistance;

public class EmailTokenDynamoModel
{
    public string PK { get; set; } = string.Empty;
    public string SK { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;

    public Guid TokenId { get; set; } = Guid.NewGuid();
    public string Email { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreationDate { get; set; }
    public DateTime ExpiryDate { get; set; }
}

public class UserEmailTokenDynamoModel : EmailTokenDynamoModel
{
    public Guid UserId = Guid.Empty;
}

public class BusinessEmailTokenDynamoModel : EmailTokenDynamoModel
{
    public Guid BusinessId = Guid.Empty;
}

