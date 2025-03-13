using LoyaltySystem.Domain.Entities;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LoyaltySystem.Infrastructure.Json;

public class OperatingHoursConverter : JsonConverter<OperatingHours>
{
    public override OperatingHours Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Expected start of object");
            
        var hours = new Dictionary<DayOfWeek, TimeRange>();
        bool foundHoursProperty = false;
        
        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            if (reader.TokenType != JsonTokenType.PropertyName)
                continue;
                
            string propertyName = reader.GetString();
            
            // Check if we have a "hours" wrapper
            if (propertyName.Equals("hours", StringComparison.OrdinalIgnoreCase))
            {
                foundHoursProperty = true;
                reader.Read(); // Move to start of hours object
                
                if (reader.TokenType != JsonTokenType.StartObject)
                    throw new JsonException("Expected start of hours object");
                    
                // Process all days inside the hours object
                while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
                {
                    if (reader.TokenType != JsonTokenType.PropertyName)
                        continue;
                        
                    // Parse day name - now supports Pascal case (Monday) and lowercase (monday)
                    string dayName = reader.GetString();
                    if (!Enum.TryParse<DayOfWeek>(dayName, true, out DayOfWeek day))
                    {
                        // Skip unknown days
                        reader.Skip();
                        continue;
                    }
                    
                    reader.Read(); // Move to the value
                    
                    if (reader.TokenType == JsonTokenType.Null)
                    {
                        // Closed day
                        continue;
                    }
                    
                    if (reader.TokenType != JsonTokenType.StartObject)
                        throw new JsonException("Expected start of time range object");
                        
                    TimeSpan? openTime = null;
                    TimeSpan? closeTime = null;
                    
                    // Read time range properties
                    while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
                    {
                        if (reader.TokenType != JsonTokenType.PropertyName)
                            continue;
                            
                        string timePropName = reader.GetString();
                        reader.Read(); // Move to the value
                        
                        if (reader.TokenType == JsonTokenType.Null)
                            continue;
                        
                        if (reader.TokenType != JsonTokenType.String)
                            throw new JsonException($"Expected string value for {timePropName}");
                            
                        string timeStr = reader.GetString();
                        
                        // Support both naming conventions (open/OpenTime)
                        if (timePropName.Equals("open", StringComparison.OrdinalIgnoreCase) ||
                            timePropName.Equals("opentime", StringComparison.OrdinalIgnoreCase))
                        {
                            if (TimeSpan.TryParse(timeStr, out var parsedTime))
                                openTime = parsedTime;
                        }
                        else if (timePropName.Equals("close", StringComparison.OrdinalIgnoreCase) ||
                                 timePropName.Equals("closetime", StringComparison.OrdinalIgnoreCase))
                        {
                            if (TimeSpan.TryParse(timeStr, out var parsedTime))
                                closeTime = parsedTime;
                        }
                    }
                    
                    // Add hours for this day if both open and close times are provided
                    if (openTime.HasValue && closeTime.HasValue)
                        hours[day] = new TimeRange(openTime.Value, closeTime.Value);
                }
            }
            else
            {
                // If we find a day name directly (without hours wrapper)
                if (Enum.TryParse<DayOfWeek>(propertyName, true, out DayOfWeek day))
                {
                    reader.Read(); // Move to the value
                    
                    if (reader.TokenType == JsonTokenType.Null)
                        continue; // Closed day
                        
                    if (reader.TokenType != JsonTokenType.StartObject)
                    {
                        reader.Skip();
                        continue;
                    }
                    
                    TimeSpan? openTime = null;
                    TimeSpan? closeTime = null;
                    
                    // Read time range properties
                    while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
                    {
                        if (reader.TokenType != JsonTokenType.PropertyName)
                            continue;
                            
                        string timePropName = reader.GetString();
                        reader.Read(); // Move to the value
                        
                        if (reader.TokenType == JsonTokenType.Null)
                            continue;
                        
                        if (reader.TokenType != JsonTokenType.String)
                            continue;
                            
                        string timeStr = reader.GetString();
                        
                        // Support both naming conventions (open/OpenTime)
                        if (timePropName.Equals("open", StringComparison.OrdinalIgnoreCase) ||
                            timePropName.Equals("opentime", StringComparison.OrdinalIgnoreCase))
                        {
                            if (TimeSpan.TryParse(timeStr, out var parsedTime))
                                openTime = parsedTime;
                        }
                        else if (timePropName.Equals("close", StringComparison.OrdinalIgnoreCase) ||
                                 timePropName.Equals("closetime", StringComparison.OrdinalIgnoreCase))
                        {
                            if (TimeSpan.TryParse(timeStr, out var parsedTime))
                                closeTime = parsedTime;
                        }
                    }
                    
                    // Add hours for this day if both open and close times are provided
                    if (openTime.HasValue && closeTime.HasValue)
                        hours[day] = new TimeRange(openTime.Value, closeTime.Value);
                }
                else
                {
                    // Skip other properties
                    reader.Skip();
                }
            }
        }
        
        // Use the constructor that takes a dictionary
        return new OperatingHours(hours);
    }

    public override void Write(Utf8JsonWriter writer, OperatingHours value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        
        // Match the expected format with hours wrapper
        writer.WritePropertyName("hours");
        writer.WriteStartObject();
        
        // Write each day and its hours
        foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
        {
            // Use Pascal case (Monday) instead of lowercase
            string dayName = day.ToString();
            writer.WritePropertyName(dayName);
            
            var timeRange = value.GetHoursForDay(day);
            if (timeRange == null)
            {
                writer.WriteNullValue();
                continue;
            }
            
            writer.WriteStartObject();
            // Use OpenTime/CloseTime property names to match expected format
            writer.WriteString("OpenTime", timeRange.OpenTime.ToString(@"hh\:mm"));
            writer.WriteString("CloseTime", timeRange.CloseTime.ToString(@"hh\:mm"));
            writer.WriteEndObject();
        }
        
        writer.WriteEndObject(); // Close hours object
        writer.WriteEndObject(); // Close root object
    }
}