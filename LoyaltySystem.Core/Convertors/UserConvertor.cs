using System.Globalization;
using LoyaltySystem.Core.Enums;
using LoyaltySystem.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LoyaltySystem.Core.Convertors;

public class UserConverter : JsonConverter
{
    public override bool CanConvert(Type objectType) => objectType == typeof(User);

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var item = JObject.Load(reader);
        
        var user = new User
        {
            Id = item["UserId"]?.Value<string>() != null ? Guid.Parse(item["UserId"].Value<string>()) : Guid.NewGuid(),
            FirstName = item["FirstName"]?.Value<string>(),
            LastName = item["LastName"]?.Value<string>(),
            Status = item["Status"]?.Value<string>() != null ? Enum.Parse<UserStatus>(item["Status"].Value<string>()) : UserStatus.Pending,
            ContactInfo = new ContactInfo { Email = item["Email"]?.Value<string>() }
        };

        var phoneNumber = item["PhoneNumber"]?.Value<string>();
        if (!string.IsNullOrWhiteSpace(phoneNumber)) user.ContactInfo.PhoneNumber = phoneNumber;
        
        var dob = item["DateOfBirth"]?.Value<string>();
        if (!string.IsNullOrWhiteSpace(dob)) user.DateOfBirth = DateTime.ParseExact(dob, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        
        return user;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) =>
        throw new NotImplementedException("Unnecessary because CanWrite is false. The type will skip the converter.");

    public override bool CanWrite => false;
}
