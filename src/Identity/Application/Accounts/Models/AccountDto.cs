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
        public string Username { get; set; } = default!;
        public string Email { get; set; } = default!;
        public bool IsActive { get; set; }
        public AccountType AccountType { get; set; }
        public BanInfoDto BanInfo { get; set; } = default!;
        public DateTime Created { get; set; }
        public LoginInfoDto? LastLoginInfo { get; set; }
        public List<string> Roles { get; set; } = new();
        
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
