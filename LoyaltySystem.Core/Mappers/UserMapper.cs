using LoyaltySystem.Core.Dtos;
using Riok.Mapperly.Abstractions;
using LoyaltySystem.Core.Models;

namespace LoyaltySystem.Core.Mappers;

[Mapper]
public partial class UserMapper
{
    public partial User CreateUserFromCreateUserDto(CreateUserDto dto);
}