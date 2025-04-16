using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using LoyaltySystem.Domain.Common;

namespace LoyaltySystem.Shared.API.Serialization
{
    /// <summary>
    /// JSON converter for EntityId types to ensure they are always serialized as their prefixed string representation
    /// </summary>
    public class EntityIdJsonConverter : JsonConverter<EntityId>
    {
        public override EntityId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String)
            {
                throw new JsonException($"Expected string value for {typeToConvert.Name}");
            }

            string stringValue = reader.GetString();
            
            // Use reflection to call Parse method on the specific EntityId type
            var parseMethod = typeToConvert.GetMethod("Parse", new[] { typeof(string) });
            if (parseMethod == null)
            {
                throw new JsonException($"Type {typeToConvert.Name} does not have a Parse method");
            }
            
            try
            {
                return (EntityId)parseMethod.Invoke(null, new object[] { stringValue });
            }
            catch (Exception ex)
            {
                throw new JsonException($"Failed to parse {typeToConvert.Name} from '{stringValue}'", ex);
            }
        }

        public override void Write(Utf8JsonWriter writer, EntityId value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }
            
            writer.WriteStringValue(value.ToString());
        }
    }
    
    /// <summary>
    /// Generic JSON converter for specific EntityId types
    /// </summary>
    public class EntityIdJsonConverter<T> : JsonConverter<T> where T : EntityId, new()
    {
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;
                
            if (reader.TokenType != JsonTokenType.String)
                throw new JsonException($"Expected string value for {typeToConvert.Name}");

            string stringValue = reader.GetString();
            
            try
            {
                if (EntityId.TryParse<T>(stringValue, out var result))
                    return result;
                    
                throw new JsonException($"Failed to parse {typeToConvert.Name} from '{stringValue}'");
            }
            catch (Exception ex)
            {
                throw new JsonException($"Error parsing {typeToConvert.Name} from '{stringValue}'", ex);
            }
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }
            
            writer.WriteStringValue(value.ToString());
        }
    }
} 