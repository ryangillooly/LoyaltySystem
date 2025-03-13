using System;

namespace LoyaltySystem.Application.DTOs
{
    /// <summary>
    /// Data Transfer Object for address information
    /// </summary>
    public partial class AddressDto
    {
        /// <summary>
        /// First line of address
        /// </summary>
        public string Line1 { get; set; } = string.Empty;
        
        /// <summary>
        /// Second line of address (optional)
        /// </summary>
        public string Line2 { get; set; } = string.Empty;
        
        /// <summary>
        /// City
        /// </summary>
        public string City { get; set; } = string.Empty;
        
        /// <summary>
        /// State or province
        /// </summary>
        public string State { get; set; } = string.Empty;
        
        /// <summary>
        /// Postal code or ZIP code
        /// </summary>
        public string PostalCode { get; set; } = string.Empty;
        
        /// <summary>
        /// Country
        /// </summary>
        public string Country { get; set; } = string.Empty;
    }

    /// <summary>
    /// Data Transfer Object for contact information
    /// </summary>
    public partial class ContactInfoDto
    {
        /// <summary>
        /// Email address
        /// </summary>
        public string Email { get; set; } = string.Empty;
        
        /// <summary>
        /// Phone number
        /// </summary>
        public string Phone { get; set; } = string.Empty;
        
        /// <summary>
        /// Website URL
        /// </summary>
        public string Website { get; set; } = string.Empty;
    }

    /// <summary>
    /// Data Transfer Object for geographic location
    /// </summary>
    public partial class GeoLocationDto
    {
        /// <summary>
        /// Latitude coordinate
        /// </summary>
        public double Latitude { get; set; }
        
        /// <summary>
        /// Longitude coordinate
        /// </summary>
        public double Longitude { get; set; }
    }
} 