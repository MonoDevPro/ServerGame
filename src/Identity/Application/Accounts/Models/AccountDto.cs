using ServerGame.Domain.Entities;
using ServerGame.Domain.Entities.Accounts;
using ServerGame.Domain.Enums;
using ServerGame.Domain.ValueObjects;
using ServerGame.Domain.ValueObjects.Accounts;

namespace ServerGame.Application.Accounts.Models
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
                CreateMap<LoginInfo, LoginInfoDto>();
            }
        }
    }
}
