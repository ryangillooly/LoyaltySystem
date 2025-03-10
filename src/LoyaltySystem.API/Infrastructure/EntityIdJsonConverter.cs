using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using LoyaltySystem.Domain.Common;

namespace LoyaltySystem.API.Infrastructure
{
    /// <summary>
    /// JSON converter for EntityId types
    /// </summary>
    public class EntityIdJsonConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert.IsSubclassOf(typeof(EntityId));
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var converterType = typeof(EntityIdJsonConverter<>).MakeGenericType(typeToConvert);
            return (JsonConverter)Activator.CreateInstance(converterType);
        }
    }

    /// <summary>
    /// JSON converter for specific EntityId types
    /// </summary>
    public class EntityIdJsonConverter<T> : JsonConverter<T> where T : EntityId, new()
    {
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;

            if (reader.TokenType != JsonTokenType.String)
                throw new JsonException($"Expected string value for {typeToConvert.Name}");

            var value = reader.GetString();
            if (string.IsNullOrEmpty(value))
                return null;

            try
            {
                return EntityId.Parse<T>(value);
            }
            catch (Exception ex)
            {
                throw new JsonException($"Error parsing {typeToConvert.Name}: {ex.Message}", ex);
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