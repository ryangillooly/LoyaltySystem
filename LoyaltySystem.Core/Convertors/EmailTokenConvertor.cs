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

        var tokenId = Guid.Parse(item[TokenId]?.Value<string>()                  ?? throw new InvalidOperationException());
        var email        = item[Email]?.Value<string>()                          ?? throw new InvalidOperationException();
        var status       = Enum.Parse<EmailTokenStatus>(item[Status]?.Value<string>() ?? throw new InvalidOperationException());
        var creationDate = DateTime.Parse(item[CreationDate]?.Value<string>()         ?? throw new InvalidOperationException());
        var expiryDate   = DateTime.Parse(item[ExpiryDate]?.Value<string>()           ?? throw new InvalidOperationException());
        
        if (objectType == typeof(UserEmailToken))
        {
            return new UserEmailToken
            {
                Id = tokenId,
                UserId = Guid.Parse(item[UserId]?.Value<string>() ?? throw new InvalidOperationException()),
                Email = email,
                Status = status,
                CreationDate = creationDate,
                ExpiryDate = expiryDate
            };
        }

        if (objectType == typeof(BusinessEmailToken))
        {
            return new BusinessEmailToken
            {
                Id           = tokenId,
                BusinessId   = Guid.Parse(item[BusinessId]?.Value<string>() ?? throw new InvalidOperationException()),
                Email        = email,
                Status       = status,
                CreationDate = creationDate,
                ExpiryDate   = expiryDate
            };
        }

        throw new JsonSerializationException($"Could not deserialize {nameof(EmailToken)}, as no {nameof(BusinessEmailToken)} or {nameof(UserEmailToken)} could be found. ");
    }
    
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        switch (value)
        {
            case UserEmailToken userEmailToken:
            {
                var obj = new JObject
                {
                    { Pk,                UserPrefix + userEmailToken.UserId },
                    { Sk,                TokenPrefix + userEmailToken.Id },
                    { TokenId,           userEmailToken.Id },
                    { UserId,            userEmailToken.UserId },
                    { Status,            userEmailToken.Status.ToString() },
                    { EntityTypeAttName, EmailTokenAttName },
                    { ExpiryDate,        userEmailToken.ExpiryDate },
                    { CreationDate,      userEmailToken.CreationDate },
                };
            
                // Serialize the JObject to the writer
                obj.WriteTo(writer);
                break;
            }
            
            case BusinessEmailToken businessEmailToken:
            {
                var obj = new JObject
                {
                    { Pk,                BusinessPrefix + businessEmailToken.BusinessId },
                    { Sk,                TokenPrefix + businessEmailToken.Id },
                    { TokenId,           businessEmailToken.Id },
                    { BusinessId,        businessEmailToken.BusinessId },
                    { Status,            businessEmailToken.Status.ToString() },
                    { EntityTypeAttName, EmailTokenAttName },
                    { ExpiryDate,        businessEmailToken.ExpiryDate },
                    { CreationDate,      businessEmailToken.CreationDate },
                };
            
                // Serialize the JObject to the writer
                obj.WriteTo(writer);
                break;
            }

            default:
            {
                throw new JsonSerializationException($"Could not serialize {nameof(EmailToken)}, as no {nameof(BusinessEmailToken)} or {nameof(UserEmailToken)} could be found. ");
            }
        }

    }
}
