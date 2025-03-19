using System;

namespace LoyaltySystem.Domain.ValueObjects
{
    /// <summary>
    /// Value object representing contact information.
    /// </summary>
    public class ContactInfo
    {
        public string Email { get; private set; }
        public string Phone { get; private set; }
        public string Website { get; private set; }

        // Private constructor for EF Core
        private ContactInfo() { }

        public ContactInfo(string email, string phone, string website)
        {
            // Regex email validation
            if (!string.IsNullOrEmpty(email))
            {
                var emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
                if (!System.Text.RegularExpressions.Regex.IsMatch(email, emailPattern))
                    throw new ArgumentException("Invalid email format", nameof(email));
            }

            Email = email;
            Phone = phone;
            Website = website;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            var other = (ContactInfo)obj;
            return Email == other.Email && 
                   Phone == other.Phone && 
                   Website == other.Website;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Email, Phone, Website);
        }
    }
} 