using System;

namespace LoyaltySystem.Domain.ValueObjects
{
    /// <summary>
    /// Value object representing a physical address.
    /// </summary>
    public class Address
    {
        public string Line1 { get; private set; }
        public string Line2 { get; private set; }
        public string City { get; private set; }
        public string State { get; private set; }
        public string PostalCode { get; private set; }
        public string Country { get; private set; }

        // Private constructor for EF Core
        private Address() { }

        public Address(
            string line1,
            string line2,
            string city,
            string state,
            string postalCode,
            string country)
        {
            if (string.IsNullOrWhiteSpace(line1))
                throw new ArgumentException("Address line 1 cannot be empty", nameof(line1));
            
            if (string.IsNullOrWhiteSpace(city))
                throw new ArgumentException("City cannot be empty", nameof(city));

            if (string.IsNullOrWhiteSpace(country))
                throw new ArgumentException("Country cannot be empty", nameof(country));

            Line1 = line1;
            Line2 = line2;
            City = city;
            State = state;
            PostalCode = postalCode;
            Country = country;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            var other = (Address)obj;
            return Line1 == other.Line1 &&
                   Line2 == other.Line2 &&
                   City == other.City &&
                   State == other.State &&
                   PostalCode == other.PostalCode &&
                   Country == other.Country;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Line1, Line2, City, State, PostalCode, Country);
        }

        public override string ToString()
        {
            var address = Line1;
            
            if (!string.IsNullOrWhiteSpace(Line2))
                address += ", " + Line2;
                
            address += ", " + City;
            
            if (!string.IsNullOrWhiteSpace(State))
                address += ", " + State;
                
            if (!string.IsNullOrWhiteSpace(PostalCode))
                address += " " + PostalCode;
                
            address += ", " + Country;
            
            return address;
        }
    }
} 