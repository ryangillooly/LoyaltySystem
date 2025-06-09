using LoyaltySystem.Domain.Common;

namespace LoyaltySystem.Domain.ValueObjects;

/// <summary>
/// Represents metadata associated with loyalty transactions
/// Encapsulates source information, device details, and contextual data
/// </summary>
public record TransactionMetadata
{
    public string Source { get; init; }
    public string? DeviceId { get; init; }
    public string? DeviceType { get; init; }
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
    public string? Location { get; init; }
    public string? Channel { get; init; }
    public string? CampaignId { get; init; }
    public string? ReferenceId { get; init; }
    public Dictionary<string, string>? AdditionalData { get; init; }

    public TransactionMetadata(
        string source,
        string? deviceId = null,
        string? deviceType = null,
        string? ipAddress = null,
        string? userAgent = null,
        string? location = null,
        string? channel = null,
        string? campaignId = null,
        string? referenceId = null,
        Dictionary<string, string>? additionalData = null)
    {
        // Validate required fields
        if (string.IsNullOrWhiteSpace(source))
            throw new DomainException("Transaction source is required");

        // Validate source format
        if (!IsValidSource(source))
            throw new DomainException("Invalid transaction source format");

        // Validate device type if provided
        if (!string.IsNullOrEmpty(deviceType) && !IsValidDeviceType(deviceType))
            throw new DomainException("Invalid device type");

        // Validate IP address format if provided
        if (!string.IsNullOrEmpty(ipAddress) && !IsValidIpAddress(ipAddress))
            throw new DomainException("Invalid IP address format");

        // Validate channel if provided
        if (!string.IsNullOrEmpty(channel) && !IsValidChannel(channel))
            throw new DomainException("Invalid channel");

        // Validate additional data size
        if (additionalData != null && additionalData.Count > 20)
            throw new DomainException("Additional data cannot contain more than 20 key-value pairs");

        // Validate additional data key/value lengths
        if (additionalData != null)
        {
            foreach (var kvp in additionalData)
            {
                if (string.IsNullOrEmpty(kvp.Key) || kvp.Key.Length > 50)
                    throw new DomainException("Additional data keys must be between 1 and 50 characters");
                if (kvp.Value != null && kvp.Value.Length > 500)
                    throw new DomainException("Additional data values cannot exceed 500 characters");
            }
        }

        Source = source.Trim().ToUpperInvariant();
        DeviceId = deviceId?.Trim();
        DeviceType = deviceType?.Trim().ToLowerInvariant();
        IpAddress = ipAddress?.Trim();
        UserAgent = userAgent?.Trim();
        Location = location?.Trim();
        Channel = channel?.Trim().ToUpperInvariant();
        CampaignId = campaignId?.Trim();
        ReferenceId = referenceId?.Trim();
        AdditionalData = additionalData != null ? new Dictionary<string, string>(additionalData) : null;
    }

    /// <summary>
    /// Checks if the transaction originated from a mobile device
    /// </summary>
    public bool IsMobileTransaction => 
        DeviceType?.ToLowerInvariant() is "mobile" or "tablet" or "smartphone";

    /// <summary>
    /// Checks if the transaction has location information
    /// </summary>
    public bool HasLocationData => !string.IsNullOrEmpty(Location);

    /// <summary>
    /// Checks if the transaction is part of a campaign
    /// </summary>
    public bool IsCampaignTransaction => !string.IsNullOrEmpty(CampaignId);

    /// <summary>
    /// Gets a specific additional data value by key
    /// </summary>
    public string? GetAdditionalData(string key)
    {
        return AdditionalData?.TryGetValue(key, out var value) == true ? value : null;
    }

    /// <summary>
    /// Validates transaction source format
    /// </summary>
    private static bool IsValidSource(string source)
    {
        var validSources = new HashSet<string>
        {
            "POS", "MOBILE_APP", "WEB_APP", "KIOSK", "API", "ADMIN", "IMPORT", "SYSTEM"
        };

        return validSources.Contains(source.ToUpperInvariant());
    }

    /// <summary>
    /// Validates device type
    /// </summary>
    private static bool IsValidDeviceType(string deviceType)
    {
        var validTypes = new HashSet<string>
        {
            "desktop", "mobile", "tablet", "smartphone", "kiosk", "pos", "unknown"
        };

        return validTypes.Contains(deviceType.ToLowerInvariant());
    }

    /// <summary>
    /// Validates IP address format (basic validation)
    /// </summary>
    private static bool IsValidIpAddress(string ipAddress)
    {
        return System.Net.IPAddress.TryParse(ipAddress, out _);
    }

    /// <summary>
    /// Validates channel format
    /// </summary>
    private static bool IsValidChannel(string channel)
    {
        var validChannels = new HashSet<string>
        {
            "ONLINE", "INSTORE", "MOBILE", "PHONE", "EMAIL", "SMS", "SOCIAL", "PARTNER"
        };

        return validChannels.Contains(channel.ToUpperInvariant());
    }

    /// <summary>
    /// Creates a new TransactionMetadata with additional data
    /// </summary>
    public TransactionMetadata WithAdditionalData(string key, string value)
    {
        var newAdditionalData = AdditionalData != null 
            ? new Dictionary<string, string>(AdditionalData) 
            : new Dictionary<string, string>();
        
        newAdditionalData[key] = value;

        return new TransactionMetadata(
            Source, DeviceId, DeviceType, IpAddress, UserAgent, 
            Location, Channel, CampaignId, ReferenceId, newAdditionalData);
    }

    /// <summary>
    /// Creates a new TransactionMetadata with updated location
    /// </summary>
    public TransactionMetadata WithLocation(string location)
    {
        return new TransactionMetadata(
            Source, DeviceId, DeviceType, IpAddress, UserAgent, 
            location, Channel, CampaignId, ReferenceId, AdditionalData);
    }

    /// <summary>
    /// Creates a new TransactionMetadata with campaign information
    /// </summary>
    public TransactionMetadata WithCampaign(string campaignId)
    {
        return new TransactionMetadata(
            Source, DeviceId, DeviceType, IpAddress, UserAgent, 
            Location, Channel, campaignId, ReferenceId, AdditionalData);
    }

    /// <summary>
    /// Creates a minimal metadata for system-generated transactions
    /// </summary>
    public static TransactionMetadata CreateSystemMetadata(string? referenceId = null)
    {
        return new TransactionMetadata("SYSTEM", referenceId: referenceId);
    }

    /// <summary>
    /// Creates metadata for POS transactions
    /// </summary>
    public static TransactionMetadata CreatePosMetadata(string deviceId, string? location = null)
    {
        return new TransactionMetadata("POS", deviceId, "pos", location: location, channel: "INSTORE");
    }

    /// <summary>
    /// Creates metadata for mobile app transactions
    /// </summary>
    public static TransactionMetadata CreateMobileMetadata(string deviceId, string? ipAddress = null, string? userAgent = null)
    {
        return new TransactionMetadata("MOBILE_APP", deviceId, "mobile", ipAddress, userAgent, channel: "MOBILE");
    }
} 