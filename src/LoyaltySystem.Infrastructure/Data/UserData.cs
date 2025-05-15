using LoyaltySystem.Domain.Entities;

namespace LoyaltySystem.Infrastructure.Data;

public class UserData 
{
    public Guid id { get; init; }
    public string prefixed_id { get; init; }
    public string first_name { get; init; }
    public string last_name { get; init; }
    public string username { get; init; }
    public string email { get; init; }
    public string password_hash { get; init; }
    public int status { get; init; }
    public DateTime created_at { get; init; }
    public DateTime updated_at { get; init; }
    public DateTime? last_login_at { get; init; }
    
    public bool email_confirmed { get; init; }
}
