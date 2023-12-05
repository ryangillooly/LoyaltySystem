using System.Globalization;
using LoyaltySystem.Core.Enums;
using LoyaltySystem.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static LoyaltySystem.Core.Models.Constants;

namespace LoyaltySystem.Core.Convertors;

public class UserConverter : JsonConverter
{
    public override bool CanConvert(Type objectType) => objectType == typeof(User);
    public override bool CanWrite => true;

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var item = JObject.Load(reader);
        var user = new User
        {
            Id          = Guid.Parse(item[UserId].Value<string>()),
            FirstName   = item[FirstName]?.Value<string>(),
            LastName    = item[LastName]?.Value<string>(),
            Status      = Enum.Parse<UserStatus>(item[Status].Value<string>()),
            ContactInfo = new ContactInfo { Email = item[Email]?.Value<string>() }
        };

        var phoneNumber = item[PhoneNumber]?.Value<string>();
        if (!string.IsNullOrWhiteSpace(phoneNumber)) user.ContactInfo.PhoneNumber = phoneNumber;
        
        var dob = item[DateOfBirth]?.Value<string>();
        //if (!string.IsNullOrWhiteSpace(dob)) user.DateOfBirth = DateTime.ParseExact(dob, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        
        return user;
    }
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value is User userValue)
        {
            var obj = new JObject
            {
                { Pk,                UserPrefix + userValue.Id },
                { Sk,                MetaUserInfo },
                { UserId,            userValue.Id.ToString() },
                { Email,             userValue.ContactInfo.Email },
                { FirstName,         userValue.FirstName },
                { LastName,          userValue.LastName },
                { Status,            userValue.Status.ToString() },
                { EntityTypeAttName, userValue.GetType().Name }
            };

            // Optional properties are only added if they are not null or empty.
            if (!string.IsNullOrWhiteSpace(userValue.ContactInfo.PhoneNumber))
                obj.Add(PhoneNumber, userValue.ContactInfo.PhoneNumber);

            if (userValue.DateOfBirth.HasValue)
                obj.Add(DateOfBirth, userValue.DateOfBirth.Value.ToString("yyyy-MM-dd"));

            // Serialize the JObject to the writer
            obj.WriteTo(writer);
        }
        else
        {
            throw new JsonSerializationException("Expected User object value");
        }
    }

}
