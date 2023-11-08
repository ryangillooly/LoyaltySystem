using LoyaltySystem.Core.Enums;
using LoyaltySystem.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static LoyaltySystem.Core.Models.Constants;

namespace LoyaltySystem.Core.Convertors;

public class BusinessEmailTokenConvertor : JsonConverter
{
    public override bool CanConvert(Type objectType) => objectType == typeof(BusinessEmailToken);
    public override bool CanWrite => true;

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var item = JObject.Load(reader);
        return new BusinessEmailToken
        {
            Id           = Guid.Parse(item[TokenId].Value<string>()),
            BusinessId   = Guid.Parse(item[BusinessId].Value<string>()),
            Email        = item[Email].Value<string>(),
            Status       = Enum.Parse<EmailTokenStatus>(item[Status].Value<string>()),
            CreationDate = DateTime.Parse(item[CreationDate].Value<string>()),
            ExpiryDate   = DateTime.Parse(item[ExpiryDate].Value<string>())
        };
    }
    // TODO: How do i change the email Token convertor to accept a Business or User? The Token should be sent for a new user, but also a new business. The User token wouldn't contain a businessId, however the business token would
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value is BusinessEmailToken emailToken)
        {
            var obj = new JObject
            {
                { Pk,                BusinessPrefix + emailToken.BusinessId },
                { Sk,                TokenPrefix + emailToken.Id },
                { TokenId,           emailToken.Id },
                { BusinessId,        emailToken.BusinessId },
                { Status,            emailToken.Status.ToString() },
                { EntityTypeAttName, EmailTokenAttName },
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
