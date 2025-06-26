using GameServer.Domain.Entities;
using GameServer.Domain.Enums;
using GameServer.Domain.ValueObjects.Accounts;

namespace GameServer.Application.Accounts.Queries.Models
{
    public class AccountDto
    {
        public long Id { get; set; }
        public bool IsActive { get; set; }
        public AccountType AccountType { get; set; }
        public BanInfoDto? BanInfo { get; set; } = null!;
        public DateTime Created { get; set; }
        public LoginInfoDto? LastLoginInfo { get; set; }
        
        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<Account, AccountDto>()
                    .ForMember(a => a.Created,
                        opt => opt.MapFrom(src => src.Created.UtcDateTime));
                
                CreateMap<BanInfo, BanInfoDto>();
                CreateMap<LoginInfo, LoginInfoDto>()
                    .ForMember(a => a.LastLoginDate,
                        opt => opt.MapFrom(src => src.LastLoginDate.UtcDateTime));
            }
        }
    }
}
