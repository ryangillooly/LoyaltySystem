namespace LoyaltySystem.Domain.Common;

public class BusinessId : EntityId
{
    public override string Prefix => "bus";
    /// <summary>
/// Initializes a new <see cref="BusinessId"/> with a unique identifier.
/// </summary>
public BusinessId() : base(Guid.NewGuid()) { }
    /// <summary>
/// Initializes a new BusinessId with the specified GUID value.
/// </summary>
public BusinessId(Guid value) : base(value) { }
    /// <summary>
/// Parses a prefixed string to create a <see cref="BusinessId"/> instance.
/// </summary>
/// <param name="prefixed">The string containing the "bus" prefix and the identifier value.</param>
/// <returns>A <see cref="BusinessId"/> represented by the input string.</returns>
public static BusinessId FromString(string prefixed) => Parse<BusinessId>(prefixed);
}
    
public class StaffId : EntityId
{
    public override string Prefix => "sta";
    /// <summary>
/// Initializes a new StaffId with a unique identifier.
/// </summary>
public StaffId() : base(Guid.NewGuid()) { }
    /// <summary>
/// Initializes a new instance of the StaffId class with the specified GUID value.
/// </summary>
public StaffId(Guid value) : base(value) { }
    /// <summary>
/// Creates a <see cref="StaffId"/> instance from its prefixed string representation.
/// </summary>
/// <param name="prefixed">The string containing the prefix and GUID for a staff identifier.</param>
/// <returns>A <see cref="StaffId"/> parsed from the provided string.</returns>
public static StaffId FromString(string prefixed) => Parse<StaffId>(prefixed);
}
    
public class BrandId : EntityId
{
    public override string Prefix => "bra";
    /// <summary>
/// Initializes a new <see cref="BrandId"/> with a unique identifier.
/// </summary>
public BrandId() : base(Guid.NewGuid()) { }
    /// <summary>
/// Initializes a new instance of the BrandId class with the specified GUID value.
/// </summary>
public BrandId(Guid value) : base(value) { }
    /// <summary>
/// Creates a <see cref="BrandId"/> instance from its prefixed string representation.
/// </summary>
/// <param name="prefixed">The prefixed string to parse.</param>
/// <returns>A <see cref="BrandId"/> corresponding to the provided string.</returns>
public static BrandId FromString(string prefixed) => Parse<BrandId>(prefixed);
}

public class StoreId : EntityId
{
    public override string Prefix => "sto";
    /// <summary>
/// Initializes a new StoreId with a unique identifier.
/// </summary>
public StoreId() : base(Guid.NewGuid()) { }
    /// <summary>
/// Initializes a new instance of the <see cref="StoreId"/> class with the specified GUID value.
/// </summary>
/// <param name="value">The GUID representing the store identifier.</param>
public StoreId(Guid value) : base(value) { }
    /// <summary>
/// Parses a prefixed string to create a <see cref="StoreId"/> instance.
/// </summary>
/// <param name="prefixed">The prefixed string representation of the store identifier.</param>
/// <returns>A <see cref="StoreId"/> corresponding to the provided string.</returns>
public static StoreId FromString(string prefixed) => Parse<StoreId>(prefixed);
}
    
public class CustomerId : EntityId
{
    public override string Prefix => "cus";
    /// <summary>
/// Initializes a new CustomerId with a unique identifier.
/// </summary>
public CustomerId() : base(Guid.NewGuid()) { }
    /// <summary>
/// Initializes a new instance of the <see cref="CustomerId"/> class with the specified GUID value.
/// </summary>
/// <param name="value">The GUID representing the customer identifier.</param>
public CustomerId(Guid value) : base(value) { }
    /// <summary>
/// Creates a <see cref="CustomerId"/> instance from its prefixed string representation.
/// </summary>
/// <param name="prefixed">The string containing the prefix and GUID for a customer identifier.</param>
/// <returns>A <see cref="CustomerId"/> parsed from the provided string.</returns>
public static CustomerId FromString(string prefixed) => Parse<CustomerId>(prefixed);
}
    
public class LoyaltyProgramId : EntityId
{
    public override string Prefix => "pro";
    /// <summary>
/// Initializes a new LoyaltyProgramId with a unique identifier.
/// </summary>
public LoyaltyProgramId() : base(Guid.NewGuid()) { }
    /// <summary>
/// Initializes a new LoyaltyProgramId with the specified GUID value.
/// </summary>
public LoyaltyProgramId(Guid value) : base(value) { }
    /// <summary>
/// Parses a prefixed string to create a <see cref="LoyaltyProgramId"/> instance.
/// </summary>
/// <param name="prefixed">The string representation of the loyalty program identifier, including its prefix.</param>
/// <returns>A <see cref="LoyaltyProgramId"/> corresponding to the provided string.</returns>
public static LoyaltyProgramId FromString(string prefixed) => Parse<LoyaltyProgramId>(prefixed);
}
    
public class LoyaltyCardId : EntityId
{
    public override string Prefix => "loy";
    /// <summary>
/// Initializes a new LoyaltyCardId with a unique identifier.
/// </summary>
public LoyaltyCardId() : base(Guid.NewGuid()) { }
    /// <summary>
/// Initializes a new instance of the LoyaltyCardId class with the specified GUID value.
/// </summary>
/// <param name="value">The GUID value representing the loyalty card identifier.</param>
public LoyaltyCardId(Guid value) : base(value) { }
    /// <summary>
/// Parses a prefixed string to create a <see cref="LoyaltyCardId"/> instance.
/// </summary>
/// <param name="prefixed">The string containing the prefix and GUID for a loyalty card identifier.</param>
/// <returns>A <see cref="LoyaltyCardId"/> represented by the input string.</returns>
public static LoyaltyCardId FromString(string prefixed) => Parse<LoyaltyCardId>(prefixed);
}
    
public class RewardId : EntityId
{
    public override string Prefix => "rew";
    /// <summary>
/// Initializes a new <see cref="RewardId"/> with a unique identifier.
/// </summary>
public RewardId() : base(Guid.NewGuid()) { }
    /// <summary>
/// Initializes a new instance of the RewardId class with the specified GUID value.
/// </summary>
/// <param name="value">The GUID representing the reward identifier.</param>
public RewardId(Guid value) : base(value) { }
    /// <summary>
/// Creates a <see cref="RewardId"/> instance from its prefixed string representation.
/// </summary>
/// <param name="prefixed">The prefixed string to parse.</param>
/// <returns>A <see cref="RewardId"/> corresponding to the provided string.</returns>
public static RewardId FromString(string prefixed) => Parse<RewardId>(prefixed);
}
    
public class TransactionId : EntityId
{
    public override string Prefix => "trx";
    /// <summary>
/// Initializes a new TransactionId with a unique identifier.
/// </summary>
public TransactionId() : base(Guid.NewGuid()) { }
    /// <summary>
/// Initializes a new TransactionId with the specified GUID value.
/// </summary>
public TransactionId(Guid value) : base(value) { }
    /// <summary>
/// Creates a <see cref="TransactionId"/> from its prefixed string representation.
/// </summary>
/// <param name="prefixed">The prefixed string to parse.</param>
/// <returns>A <see cref="TransactionId"/> instance corresponding to the provided string.</returns>
public static TransactionId FromString(string prefixed) => Parse<TransactionId>(prefixed);
}
    
public class UserId : EntityId
{
    public override string Prefix => "usr";
    /// <summary>
/// Initializes a new UserId with a unique identifier.
/// </summary>
public UserId() : base(Guid.NewGuid()) { }
    /// <summary>
/// Initializes a new instance of the UserId class with the specified GUID value.
/// </summary>
public UserId(Guid value) : base(value) { }
    /// <summary>
/// Creates a <see cref="UserId"/> instance from its prefixed string representation.
/// </summary>
/// <param name="prefixed">The string containing the "usr" prefix followed by the identifier value.</param>
/// <returns>A <see cref="UserId"/> parsed from the provided string.</returns>
public static UserId FromString(string prefixed) => Parse<UserId>(prefixed);
}
    
public class UserRoleId : EntityId
{
    public override string Prefix => "role";
    /// <summary>
/// Initializes a new UserRoleId with a unique identifier.
/// </summary>
public UserRoleId() : base(Guid.NewGuid()) { }
    /// <summary>
/// Initializes a new instance of the UserRoleId class with the specified GUID value.
/// </summary>
/// <param name="value">The GUID representing the unique identifier for a user role.</param>
public UserRoleId(Guid value) : base(value) { }
    /// <summary>
/// Creates a <see cref="UserRoleId"/> instance from its prefixed string representation.
/// </summary>
/// <param name="prefixed">The string containing the prefix and GUID for the user role identifier.</param>
/// <returns>A <see cref="UserRoleId"/> parsed from the provided string.</returns>
public static UserRoleId FromString(string prefixed) => Parse<UserRoleId>(prefixed);
}

public class VerificationTokenId : EntityId
{
    public override string Prefix => "vto";
    /// <summary>
/// Initializes a new <see cref="VerificationTokenId"/> with a unique identifier.
/// </summary>
public VerificationTokenId() : base(Guid.NewGuid()) { }
    /// <summary>
/// Initializes a new instance of the <see cref="VerificationTokenId"/> class with the specified GUID value.
/// </summary>
/// <param name="value">The GUID value for the verification token identifier.</param>
public VerificationTokenId(Guid value) : base(value) { }
    /// <summary>
/// Parses a prefixed string to create a <see cref="VerificationTokenId"/> instance.
/// </summary>
/// <param name="prefixed">The string containing the prefix and identifier value.</param>
/// <returns>A <see cref="VerificationTokenId"/> represented by the input string.</returns>
public static VerificationTokenId FromString(string prefixed) => Parse<VerificationTokenId>(prefixed);
}

public class LoyaltyTierId : EntityId
{
    public override string Prefix => "ltid";
    /// <summary>
/// Initializes a new LoyaltyTierId with a unique identifier.
/// </summary>
public LoyaltyTierId() : base(Guid.NewGuid()) { }
    /// <summary>
/// Initializes a new LoyaltyTierId with the specified GUID value.
/// </summary>
/// <param name="value">The GUID representing the loyalty tier identifier.</param>
public LoyaltyTierId(Guid value) : base(value) { }
    /// <summary>
/// Parses a prefixed string to create a <see cref="LoyaltyTierId"/> instance.
/// </summary>
/// <param name="prefixed">The string containing the prefix and GUID for a loyalty tier identifier.</param>
/// <returns>A <see cref="LoyaltyTierId"/> represented by the input string.</returns>
public static LoyaltyTierId FromString(string prefixed) => Parse<LoyaltyTierId>(prefixed);
}