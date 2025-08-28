using UserManagement.ApplicationFeatures.Users.Dtos;
using UserManagement.Domain.Entities;

namespace UserManagement.ApplicationFeatures.Users.Mappers
{
    public class UserMappingProfile : AutoMapper.Profile
    {
        public UserMappingProfile()
        {
            CreateMap<CreateUserRequestDto, User>()
                .ForMember(d => d.PasswordHash, opt => opt.Ignore())
                .ForMember(d => d.CreatedAt, opt => opt.Ignore())
                .ForMember(d => d.UserName, opt => opt.Ignore())
               .ReverseMap();


            CreateMap<User, UserResponseDto>()
                   .ForMember(dest => dest.Profile, opt => opt.MapFrom(src => src.Profile))
                   .ForMember(d => d.AccountType, opt => opt.Ignore())
                   .ForMember(d => d.AccessToken, opt => opt.Ignore())
                   .ReverseMap();
        }
    }
}
