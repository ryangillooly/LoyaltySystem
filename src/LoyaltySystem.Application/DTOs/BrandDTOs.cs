using LoyaltySystem.Domain.Common;
using System;

namespace LoyaltySystem.Application.DTOs
{
    /// <summary>
    /// Data Transfer Object for Brand entity
    /// </summary>
    public class BrandDto
    {
        /// <summary>
        /// The unique identifier for the brand
        /// </summary>
        public string Id { get; set; } = string.Empty;
        
        /// <summary>
        /// The name of the brand
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// The category of the brand
        /// </summary>
        public string Category { get; set; } = string.Empty;
        
        /// <summary>
        /// The logo URL for the brand
        /// </summary>
        public string Logo { get; set; } = string.Empty;
        
        /// <summary>
        /// The description of the brand
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// The contact information for the brand
        /// </summary>
        public ContactInfoDto ContactInfo { get; set; } = new ContactInfoDto();
        
        /// <summary>
        /// The address of the brand headquarters
        /// </summary>
        public AddressDto Address { get; set; } = new AddressDto();
        
        /// <summary>
        /// The date and time when the brand was created
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// The date and time when the brand was created
        /// </summary>
        public DateTime UpdatedAt { get; set; }
        
        /// <summary>
        /// The business ID that owns this brand
        /// </summary>
        public string BusinessId { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for creating a new brand
    /// </summary>
    public class CreateBrandDto
    {
        /// <summary>
        /// The name of the brand
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// The category of the brand
        /// </summary>
        public string Category { get; set; } = string.Empty;
        
        /// <summary>
        /// The logo URL for the brand
        /// </summary>
        public string Logo { get; set; } = string.Empty;
        
        /// <summary>
        /// The description of the brand
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// The contact information for the brand
        /// </summary>
        public ContactInfoDto ContactInfo { get; set; } = new ();
        
        /// <summary>
        /// The address of the brand headquarters
        /// </summary>
        public AddressDto Address { get; set; } = new ();
        
        /// <summary>
        /// The business ID that owns this brand
        /// </summary>
        public string BusinessId { get; set; }
    }

    /// <summary>
    /// DTO for updating an existing brand
    /// </summary>
    public class UpdateBrandDto
    {
        /// <summary>
        /// The name of the brand
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// The category of the brand
        /// </summary>
        public string Category { get; set; } = string.Empty;
        
        /// <summary>
        /// The logo URL for the brand
        /// </summary>
        public string Logo { get; set; } = string.Empty;
        
        /// <summary>
        /// The description of the brand
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// The contact information for the brand
        /// </summary>
        public ContactInfoDto ContactInfo { get; set; } = new ContactInfoDto();
        
        /// <summary>
        /// The address of the brand headquarters
        /// </summary>
        public AddressDto Address { get; set; } = new AddressDto();
    }
} 