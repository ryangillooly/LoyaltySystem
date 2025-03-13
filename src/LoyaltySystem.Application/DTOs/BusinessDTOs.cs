using System;
using System.Collections.Generic;

namespace LoyaltySystem.Application.DTOs
{
    /// <summary>
    /// Data Transfer Object for Business entity
    /// </summary>
    public class BusinessDto
    {
        /// <summary>
        /// The unique identifier for the business
        /// </summary>
        public string Id { get; set; } = string.Empty;
        
        /// <summary>
        /// The name of the business
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// The business description
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// The tax identification number for the business
        /// </summary>
        public string TaxId { get; set; } = string.Empty;
        
        /// <summary>
        /// The primary contact information for the business
        /// </summary>
        public ContactInfoDto Contact { get; set; } = new ContactInfoDto();
        
        /// <summary>
        /// The primary address of the business headquarters
        /// </summary>
        public AddressDto HeadquartersAddress { get; set; } = new AddressDto();
        
        /// <summary>
        /// The logo URL for the business
        /// </summary>
        public string Logo { get; set; } = string.Empty;
        
        /// <summary>
        /// The website for the business
        /// </summary>
        public string Website { get; set; } = string.Empty;
        
        /// <summary>
        /// The date the business was established
        /// </summary>
        public DateTime? FoundedDate { get; set; }
        
        /// <summary>
        /// The date and time when the business was created in the system
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// The date and time when the business was last updated
        /// </summary>
        public DateTime UpdatedAt { get; set; }
        
        /// <summary>
        /// Whether the business is currently active
        /// </summary>
        public bool IsActive { get; set; }
        
        /// <summary>
        /// The brands owned by this business
        /// </summary>
        public List<BrandDto> Brands { get; set; } = new List<BrandDto>();
    }

    /// <summary>
    /// DTO for creating a new business
    /// </summary>
    public class CreateBusinessDto
    {
        /// <summary>
        /// The name of the business
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// The business description
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// The tax identification number for the business
        /// </summary>
        public string TaxId { get; set; } = string.Empty;
        
        /// <summary>
        /// The primary contact information for the business
        /// </summary>
        public ContactInfoDto Contact { get; set; } = new ContactInfoDto();
        
        /// <summary>
        /// The primary address of the business headquarters
        /// </summary>
        public AddressDto HeadquartersAddress { get; set; } = new AddressDto();
        
        /// <summary>
        /// The logo URL for the business
        /// </summary>
        public string Logo { get; set; } = string.Empty;
        
        /// <summary>
        /// The website for the business
        /// </summary>
        public string Website { get; set; } = string.Empty;
        
        /// <summary>
        /// The date the business was established
        /// </summary>
        public DateTime? FoundedDate { get; set; }
    }

    /// <summary>
    /// DTO for updating an existing business
    /// </summary>
    public class UpdateBusinessDto
    {
        /// <summary>
        /// The name of the business
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// The business description
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// The tax identification number for the business
        /// </summary>
        public string TaxId { get; set; } = string.Empty;
        
        /// <summary>
        /// The primary contact information for the business
        /// </summary>
        public ContactInfoDto Contact { get; set; } = new ContactInfoDto();
        
        /// <summary>
        /// The primary address of the business headquarters
        /// </summary>
        public AddressDto HeadquartersAddress { get; set; } = new AddressDto();
        
        /// <summary>
        /// The logo URL for the business
        /// </summary>
        public string Logo { get; set; } = string.Empty;
        
        /// <summary>
        /// The website for the business
        /// </summary>
        public string Website { get; set; } = string.Empty;
        
        /// <summary>
        /// The date the business was established
        /// </summary>
        public DateTime? FoundedDate { get; set; }
        
        /// <summary>
        /// Whether the business is currently active
        /// </summary>
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// DTO for representing business summary information
    /// </summary>
    public class BusinessSummaryDto
    {
        /// <summary>
        /// The unique identifier for the business
        /// </summary>
        public string Id { get; set; } = string.Empty;
        
        /// <summary>
        /// The name of the business
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// The logo URL for the business
        /// </summary>
        public string Logo { get; set; } = string.Empty;
        
        /// <summary>
        /// The number of brands owned by this business
        /// </summary>
        public int BrandCount { get; set; }
        
        /// <summary>
        /// The date and time when the business was created in the system
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
} 