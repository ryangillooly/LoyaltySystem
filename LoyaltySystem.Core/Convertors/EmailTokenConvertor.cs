using LoyaltySystem.Core.Enums;
using LoyaltySystem.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static LoyaltySystem.Core.Models.Constants;

namespace LoyaltySystem.Core.Convertors;

public class EmailTokenConvertor : JsonConverter
{
    public override bool CanConvert(Type objectType) => objectType == typeof(EmailToken);
    public override bool CanWrite => true;

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var item = JObject.Load(reader);
        return new EmailToken
        {
            Id           = Guid.Parse(item["TokenId"].Value<string>()),
            UserId       = Guid.Parse(item["UserId"].Value<string>()),
            Status       = Enum.Parse<EmailTokenStatus>(item["Status"].Value<string>()),
            CreationDate = DateTime.Parse(item["CreationDate"].Value<string>()),
            ExpiryDate   = DateTime.Parse(item["ExpiryDate"].Value<string>())
        };
    }
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value is EmailToken emailToken)
        {
            var obj = new JObject
            {
                { Pk,                UserPrefix + emailToken.UserId },
                { Sk,                TokenPrefix + emailToken.Id },
                { TokenId,           emailToken.Id },
                { UserId,            emailToken.UserId },
                { Status,            emailToken.Status.ToString() },
                { EntityTypeAttName, emailToken.GetType().Name },
                { ExpiryDate,        emailToken.ExpiryDate },
                { CreationDate,      emailToken.CreationDate },
            };
            
            // Serialize the JObject to the writer
            obj.WriteTo(writer);
        }
        else
        {
            throw new JsonSerializationException("Expected EmailToken object value");
        }
    }

}
