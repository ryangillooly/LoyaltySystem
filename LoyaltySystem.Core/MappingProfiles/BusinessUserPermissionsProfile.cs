using AutoMapper;
using LoyaltySystem.Core.Dtos;
using LoyaltySystem.Core.Models;
using LoyaltySystem.Core.Models.Persistance;
using LoyaltySystem.Core.Utilities;
using static LoyaltySystem.Core.Models.Constants;

namespace LoyaltySystem.Core.MappingProfiles;

public class BusinessUserPermissionsProfile : Profile
{
    public BusinessUserPermissionsProfile()
    {
        CreateMap<BusinessUserPermissionsDynamoModel, BusinessUserPermissions>();
    }
}