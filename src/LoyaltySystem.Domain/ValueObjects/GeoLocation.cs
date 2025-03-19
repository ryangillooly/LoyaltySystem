using System;

namespace LoyaltySystem.Domain.ValueObjects
{
    /// <summary>
    /// Value object representing geographical coordinates.
    /// </summary>
    public class GeoLocation
    {
        public double Latitude { get; private set; }
        public double Longitude { get; private set; }

        // Private constructor for EF Core
        private GeoLocation() { }

        public GeoLocation(double latitude, double longitude)
        {
            // Basic validation for latitude (-90 to 90)
            if (latitude < -90 || latitude > 90)
                throw new ArgumentOutOfRangeException(nameof(latitude), "Latitude must be between -90 and 90 degrees.");

            // Basic validation for longitude (-180 to 180)
            if (longitude < -180 || longitude > 180)
                throw new ArgumentOutOfRangeException(nameof(longitude), "Longitude must be between -180 and 180 degrees.");

            Latitude = latitude;
            Longitude = longitude;
        }

        /// <summary>
        /// Calculates the distance in kilometers between this location and another location
        /// using the Haversine formula.
        /// </summary>
        public double DistanceToInKilometers(GeoLocation other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            const double EarthRadiusKm = 6371.0;
            
            var dLat = DegreesToRadians(other.Latitude - Latitude);
            var dLon = DegreesToRadians(other.Longitude - Longitude);
            
            var lat1 = DegreesToRadians(Latitude);
            var lat2 = DegreesToRadians(other.Latitude);
            
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2) * 
                    Math.Cos(lat1) * Math.Cos(lat2);
            
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            
            return EarthRadiusKm * c;
        }

        private double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            var other = (GeoLocation)obj;
            return Latitude == other.Latitude && Longitude == other.Longitude;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Latitude, Longitude);
        }

        public override string ToString()
        {
            return $"{Latitude}, {Longitude}";
        }
    }
} 