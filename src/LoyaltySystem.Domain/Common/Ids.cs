using System;

namespace LoyaltySystem.Domain.Common
{
    /// <summary>
    /// Brand identifier (bnd_xxxxxxxx)
    /// </summary>
    public class BrandId : EntityId
    {
        public override string Prefix => "bnd_";
        
        public BrandId() { }
        
        public BrandId(Guid value) : base(value) { }
        
        // Create from string representation
        public static BrandId FromString(string prefixed) => Parse<BrandId>(prefixed);
    }
    
    /// <summary>
    /// Store identifier (sto_xxxxxxxx)
    /// </summary>
    public class StoreId : EntityId
    {
        public override string Prefix => "sto_";
        
        public StoreId() { }
        
        public StoreId(Guid value) : base(value) { }
        
        // Create from string representation
        public static StoreId FromString(string prefixed) => Parse<StoreId>(prefixed);
    }
    
    /// <summary>
    /// Customer identifier (cus_xxxxxxxx)
    /// </summary>
    public class CustomerId : EntityId
    {
        public override string Prefix => "cus_";
        
        public CustomerId() { }
        
        public CustomerId(Guid value) : base(value) { }
        
        // Create from string representation
        public static CustomerId FromString(string prefixed) => Parse<CustomerId>(prefixed);
    }
    
    /// <summary>
    /// Loyalty Program identifier (prg_xxxxxxxx)
    /// </summary>
    public class LoyaltyProgramId : EntityId
    {
        public override string Prefix => "prg_";
        
        public LoyaltyProgramId() { }
        
        public LoyaltyProgramId(Guid value) : base(value) { }
        
        // Create from string representation
        public static LoyaltyProgramId FromString(string prefixed) => Parse<LoyaltyProgramId>(prefixed);
    }
    
    /// <summary>
    /// Loyalty Card identifier (lcy_xxxxxxxx)
    /// </summary>
    public class LoyaltyCardId : EntityId
    {
        public override string Prefix => "lcy_";
        
        public LoyaltyCardId() { }
        
        public LoyaltyCardId(Guid value) : base(value) { }
        
        // Create from string representation
        public static LoyaltyCardId FromString(string prefixed) => Parse<LoyaltyCardId>(prefixed);
    }
    
    /// <summary>
    /// Reward identifier (rwd_xxxxxxxx)
    /// </summary>
    public class RewardId : EntityId
    {
        public override string Prefix => "rwd_";
        
        public RewardId() { }
        
        public RewardId(Guid value) : base(value) { }
        
        // Create from string representation
        public static RewardId FromString(string prefixed) => Parse<RewardId>(prefixed);
    }
    
    /// <summary>
    /// Transaction identifier (txn_xxxxxxxx)
    /// </summary>
    public class TransactionId : EntityId
    {
        public override string Prefix => "txn_";
        
        public TransactionId() { }
        
        public TransactionId(Guid value) : base(value) { }
        
        // Create from string representation
        public static TransactionId FromString(string prefixed) => Parse<TransactionId>(prefixed);
    }
    
    /// <summary>
    /// User identifier (usr_xxxxxxxx)
    /// </summary>
    public class UserId : EntityId
    {
        public override string Prefix => "usr_";
        
        public UserId() { }
        
        public UserId(Guid value) : base(value) { }
        
        // Create from string representation
        public static UserId FromString(string prefixed) => Parse<UserId>(prefixed);
    }
} 