using System;
using System.Data;
using Dapper;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Infrastructure.Json;
using System.Text.Json;

namespace LoyaltySystem.Infrastructure.Data.TypeHandlers
{
    /// <summary>
    /// TypeHandler for mapping OperatingHours to and from JSON in the database
    /// </summary>
    public class OperatingHoursTypeHandler : SqlMapper.TypeHandler<OperatingHours>
    {
        private readonly JsonSerializerOptions _jsonOptions;
        
        public OperatingHoursTypeHandler()
        {
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            _jsonOptions.Converters.Add(new OperatingHoursConverter());
        }
        
        public override void SetValue(IDbDataParameter parameter, OperatingHours value)
        {
            parameter.Value = value != null 
                ? JsonSerializer.Serialize(value, _jsonOptions) 
                : DBNull.Value;
        }

        public override OperatingHours Parse(object value)
        {
            if (value == null || value is DBNull)
                return new OperatingHours();
                
            if (value is string json && !string.IsNullOrEmpty(json))
            {
                try
                {
                    return JsonSerializer.Deserialize<OperatingHours>(json, _jsonOptions);
                }
                catch (Exception ex)
                {
                    throw new InvalidCastException($"Failed to parse OperatingHours from JSON: {ex.Message}", ex);
                }
            }
            
            throw new InvalidCastException($"Cannot convert {value.GetType().Name} to OperatingHours");
        }
    }
} 