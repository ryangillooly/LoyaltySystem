using LoyaltySystem.Core.Dtos;
using LoyaltySystem.Core.DTOs;
using Riok.Mapperly.Abstractions;
using LoyaltySystem.Core.Models;

namespace LoyaltySystem.Core.Mappers;

[Mapper]
public partial class BusinessMapper
{
    public partial Business CreateBusinessFromCreateBusinessDto(CreateBusinessDto dto);
}