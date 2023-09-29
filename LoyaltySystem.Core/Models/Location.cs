namespace LoyaltySystem.Core.Models;

public class Location
{
    public double Latitude { get; set; }  = double.MinValue;
    public double Longitude { get; set; } = double.MinValue;
    public string Address { get; set; }   = string.Empty;
}