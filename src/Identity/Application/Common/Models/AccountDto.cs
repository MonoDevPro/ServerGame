using ServerGame.Domain.Entities;
using ServerGame.Domain.Enums;
using ServerGame.Domain.ValueObjects;

namespace ServerGame.Application.Common.Models
{
    public class AccountDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = default!;
        public string Email { get; set; } = default!;
        public bool IsActive { get; set; }
        public AccountType AccountType { get; set; }
        public BanInfoDto BanInfo { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
        public LoginInfoDto? LastLoginInfo { get; set; }
        public List<string> Roles { get; set; } = new();
        
        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<Account, AccountDto>();
                CreateMap<BanInfoVO, BanInfoDto>();
                CreateMap<LoginInfoVO, LoginInfoDto>();
            }
        }
    }

    public class BanInfoDto
    {
        public BanStatus Status { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public string? Reason { get; set; }
        public long? BannedById { get; set; }
    }

    public class LoginInfoDto
    {
        public string LastLoginIp { get; set; } = default!;
        public DateTime LastLoginDate { get; set; } = default!;
    }
}
