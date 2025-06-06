using ServerGame.Application.Accounts.Models;
using ServerGame.Application.Common.Models;
using ServerGame.Domain.Entities;
using ServerGame.Domain.Enums;
using ServerGame.Domain.ValueObjects;

namespace ServerGame.Application.Accounts.Mappings;

// Foi centralizado os profiles dentro dos próprios DTOs.

/*
public class AccountMappingProfile : Profile
{
    public AccountMappingProfile()
    {
        CreateMap<Account, AccountDto>()
            // VOs para primitivas
            .ForMember(d => d.Username,
                opt => opt.MapFrom(a => a.Username.Value))
            .ForMember(d => d.Email,
                opt => opt.MapFrom(a => a.Email.Value))
            // Lista de RoleVO para List<string>
            .ForMember(d => d.Roles,
                opt => opt.MapFrom(a => a.Roles.Select(r => r.Value)))
            // BanInfoVO → BanInfoDto
            .ForMember(d => d.BanInfo,
                opt => opt.MapFrom(a => a.BanInfo))
            // LoginInfoVO → LoginInfoDto (pode ser nulo)
            .ForMember(d => d.LastLoginInfo,
                opt => opt.MapFrom(a => a.LastLoginInfo));

        CreateMap<BanInfo, BanInfoDto>()
            .ForMember(d => d.Status,
                opt => opt.MapFrom(vo => vo.IsActive() ? BanStatus.NotBanned : BanStatus.PermanentBan))
            .ForMember(d => d.ExpiresAt,
                opt => opt.MapFrom(vo => vo.ExpiresAt))
            .ForMember(d => d.Reason,
                opt => opt.MapFrom(vo => vo.Reason))
            .ForMember(d => d.BannedById,
                opt => opt.MapFrom(vo => vo.BannedById));

        CreateMap<LoginInfo, LoginInfoDto>()
            .ForMember(d => d.LastLoginIp,
                opt => opt.MapFrom(vo => vo.LastLoginIp))
            .ForMember(d => d.LastLoginDate,
                opt => opt.MapFrom(vo => vo.LastLoginDate));
    }
}
*/
