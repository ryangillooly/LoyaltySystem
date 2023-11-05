using System.Globalization;
using LoyaltySystem.Core.Enums;
using LoyaltySystem.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static LoyaltySystem.Core.Models.Constants;

namespace LoyaltySystem.Core.Convertors;

public class BusinessConvertor : JsonConverter
{
    public override bool CanConvert(Type objectType) => objectType == typeof(Business);
    public override bool CanWrite => true;

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var item = JObject.Load(reader);

        var business = new Business
        {
            Id           = Guid.Parse(item[BusinessId].Value<string>()),
            OwnerId      = Guid.Parse(item[OwnerId].Value<string>()),
            Name         = item[Name].Value<string>(),
            Location     = JsonConvert.DeserializeObject<Location>(item[LocationAttributeName].Value<string>())   ?? throw new NullReferenceException(),
            OpeningHours = JsonConvert.DeserializeObject<OpeningHours>(item[OpeningHoursAttName].Value<string>()) ?? throw new NullReferenceException(),
            ContactInfo  = new ContactInfo { Email = item[Email].Value<string>() },
            Status       = Enum.Parse<BusinessStatus>(item[Status].Value<string>()),
        };

        var description = item[Description]?.Value<string>();
        if (!string.IsNullOrWhiteSpace(description)) business.Description = description;
        
        var phoneNumber = item[PhoneNumber]?.Value<string>();
        if (!string.IsNullOrWhiteSpace(phoneNumber)) business.ContactInfo.PhoneNumber = phoneNumber;
        
        return business;
    }
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value is Business business)
        {
            var openingHoursJson = JsonConvert.SerializeObject(business.OpeningHours);
            var locationJson     = JsonConvert.SerializeObject(business.Location);
            
            var obj = new JObject
            {
                { Pk,                   BusinessPrefix + business.Id },
                { Sk,                   MetaBusinessInfo },
                { BusinessId,           business.Id},
                { OwnerId,              business.OwnerId},
                { Name,                 business.Name },
                { OpeningHoursAttName,  openingHoursJson},
                { LocationAttributeName,locationJson },
                { Email,                business.ContactInfo.Email },
                { Status,               business.Status.ToString()},
                { EntityTypeAttName,    business.GetType().Name }
            };

            if (!string.IsNullOrWhiteSpace(business.Description))             obj.Add(Description, business.Description);
            if (!string.IsNullOrWhiteSpace(business.ContactInfo.PhoneNumber)) obj.Add(PhoneNumber, business.ContactInfo.PhoneNumber);

            // Serialize the JObject to the writer
            obj.WriteTo(writer);
        }
        else
        {
            throw new JsonSerializationException("Expected Business object value");
        }
    }

}
