using AutoMapper;
using LoyaltySystem.Core.Dtos;
using LoyaltySystem.Core.Models;
using LoyaltySystem.Core.Models.Persistance;
using LoyaltySystem.Core.Utilities;
using static LoyaltySystem.Core.Models.Constants;

namespace LoyaltySystem.Core.MappingProfiles;

public class EmailTokenProfile : Profile
{
    public EmailTokenProfile()
    {
        // Email Token Base
        CreateMap<EmailToken, EmailTokenDynamoModel>()
            .ForMember(dest => dest.TokenId,      opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.EntityType,   opt => opt.MapFrom(src => src.GetType().BaseType!.Name));
        
        // User Email Token
        CreateMap<UserEmailTokenDynamoModel, UserEmailToken>();
        CreateMap<UserEmailToken, UserEmailTokenDynamoModel>()
            .IncludeBase<EmailToken, EmailTokenDynamoModel>()
            .ForMember(dest => dest.PK,     opt => opt.MapFrom(src => UserPrefix + src.UserId))
            .ForMember(dest => dest.SK,     opt => opt.MapFrom(src => TokenPrefix + src.Id))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId));
        
        // Business Email Token
        CreateMap<BusinessEmailTokenDynamoModel, BusinessEmailToken>();
        CreateMap<BusinessEmailToken, BusinessEmailTokenDynamoModel>()
            .IncludeBase<EmailToken, EmailTokenDynamoModel>()
            .ForMember(dest => dest.PK,         opt => opt.MapFrom(src => BusinessPrefix + src.BusinessId))
            .ForMember(dest => dest.SK,         opt => opt.MapFrom(src => TokenPrefix + src.Id))
            .ForMember(dest => dest.BusinessId, opt => opt.MapFrom(src => src.BusinessId));
    }
}