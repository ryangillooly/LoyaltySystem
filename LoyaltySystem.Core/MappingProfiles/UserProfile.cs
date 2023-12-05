using AutoMapper;
using LoyaltySystem.Core.Dtos;
using LoyaltySystem.Core.Models;
using LoyaltySystem.Core.Models.Persistance;
using LoyaltySystem.Core.Utilities;
using static LoyaltySystem.Core.Models.Constants;

namespace LoyaltySystem.Core.MappingProfiles;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<User, UserDynamoModel>()
            .ForMember(dest => dest.PK,          opt => opt.MapFrom(src => UserPrefix + src.Id))
            .ForMember(dest => dest.SK,          opt => opt.MapFrom(src => MetaUserInfo))
            .ForMember(dest => dest.Email,       opt => opt.MapFrom(src => src.ContactInfo.Email))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.ContactInfo.PhoneNumber))
            .ForMember(dest => dest.Status,      opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.EntityType,  opt => opt.MapFrom(src => src.GetType().Name));

        CreateMap<UserDynamoModel, User>()
            .ForPath(dest => dest.ContactInfo.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
            .ForPath(dest => dest.ContactInfo.Email,       opt => opt.MapFrom(src => src.Email));

        CreateMap<CreateUserDto, User>();
    }
}