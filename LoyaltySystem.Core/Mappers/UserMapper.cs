using LoyaltySystem.Core.Dtos;
using Riok.Mapperly.Abstractions;
using LoyaltySystem.Core.Models;
using LoyaltySystem.Core.Models.Persistance;

namespace LoyaltySystem.Core.Mappers;

public interface IUserMapper
{
    User CreateUserFromCreateUserDto(CreateUserDto dto);
    UserDynamoModel MapToDynamoModel(User user);
}

[Mapper]
public partial class UserMapper : IUserMapper
{
    public partial User CreateUserFromCreateUserDto(CreateUserDto dto);
    
    public partial UserDynamoModel MapToDynamoModel(User user);
}