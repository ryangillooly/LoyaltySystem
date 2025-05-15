
namespace LoyaltySystem.Domain.Common;

public class BusinessId : EntityId
{
    public override string Prefix => "bus";
    public BusinessId() : base(Guid.NewGuid()) { }
    public BusinessId(Guid value) : base(value) { }
    public static BusinessId FromString(string prefixed) => Parse<BusinessId>(prefixed);
}
    
public class StaffId : EntityId
{
    public override string Prefix => "sta";
    public StaffId() : base(Guid.NewGuid()) { }
    public StaffId(Guid value) : base(value) { }
    public static StaffId FromString(string prefixed) => Parse<StaffId>(prefixed);
}
    
public class BrandId : EntityId
{
    public override string Prefix => "bra";
    public BrandId() : base(Guid.NewGuid()) { }
    public BrandId(Guid value) : base(value) { }
    public static BrandId FromString(string prefixed) => Parse<BrandId>(prefixed);
}

public class StoreId : EntityId
{
    public override string Prefix => "sto";
    public StoreId() : base(Guid.NewGuid()) { }
    public StoreId(Guid value) : base(value) { }
    public static StoreId FromString(string prefixed) => Parse<StoreId>(prefixed);
}
    
public class CustomerId : EntityId
{
    public override string Prefix => "cus";
    public CustomerId() : base(Guid.NewGuid()) { }
    public CustomerId(Guid value) : base(value) { }
    public static CustomerId FromString(string prefixed) => Parse<CustomerId>(prefixed);
}
    
public class LoyaltyProgramId : EntityId
{
    public override string Prefix => "pro";
    public LoyaltyProgramId() : base(Guid.NewGuid()) { }
    public LoyaltyProgramId(Guid value) : base(value) { }
    public static LoyaltyProgramId FromString(string prefixed) => Parse<LoyaltyProgramId>(prefixed);
}
    
public class LoyaltyCardId : EntityId
{
    public override string Prefix => "loy";
    public LoyaltyCardId() : base(Guid.NewGuid()) { }
    public LoyaltyCardId(Guid value) : base(value) { }
    public static LoyaltyCardId FromString(string prefixed) => Parse<LoyaltyCardId>(prefixed);
}
    
public class RewardId : EntityId
{
    public override string Prefix => "rew";
    public RewardId() : base(Guid.NewGuid()) { }
    public RewardId(Guid value) : base(value) { }
    public static RewardId FromString(string prefixed) => Parse<RewardId>(prefixed);
}
    
public class TransactionId : EntityId
{
    public override string Prefix => "trx";
    public TransactionId() : base(Guid.NewGuid()) { }
    public TransactionId(Guid value) : base(value) { }
    public static TransactionId FromString(string prefixed) => Parse<TransactionId>(prefixed);
}
    
public class UserId : EntityId
{
    public override string Prefix => "usr";
    public UserId() : base(Guid.NewGuid()) { }
    public UserId(Guid value) : base(value) { }
    public static UserId FromString(string prefixed) => Parse<UserId>(prefixed);
}
    
public class UserRoleId : EntityId
{
    public override string Prefix => "role";
    public UserRoleId() : base(Guid.NewGuid()) { }
    public UserRoleId(Guid value) : base(value) { }
    public static UserRoleId FromString(string prefixed) => Parse<UserRoleId>(prefixed);
}

public class VerificationTokenId : EntityId
{
    public override string Prefix => "vto";
    public VerificationTokenId() : base(Guid.NewGuid()) { }
    public VerificationTokenId(Guid value) : base(value) { }
    public static VerificationTokenId FromString(string prefixed) => Parse<VerificationTokenId>(prefixed);
}

public class LoyaltyTierId : EntityId
{
    public override string Prefix => "ltid";
    public LoyaltyTierId() : base(Guid.NewGuid()) { }
    public LoyaltyTierId(Guid value) : base(value) { }
    public static LoyaltyTierId FromString(string prefixed) => Parse<LoyaltyTierId>(prefixed);
}