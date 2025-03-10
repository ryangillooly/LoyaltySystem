namespace LoyaltySystem.Domain.Enums
{
    /// <summary>
    /// Represents the roles a user can have in the system.
    /// </summary>
    public enum RoleType
    {
        /// <summary>
        /// Regular customer role.
        /// </summary>
        Customer = 1,
        
        /// <summary>
        /// Store staff role.
        /// </summary>
        Staff = 2,
        
        /// <summary>
        /// Store manager role.
        /// </summary>
        StoreManager = 3,
        
        /// <summary>
        /// Brand manager role.
        /// </summary>
        BrandManager = 4,
        
        /// <summary>
        /// System administrator role.
        /// </summary>
        Admin = 5
    }
} 