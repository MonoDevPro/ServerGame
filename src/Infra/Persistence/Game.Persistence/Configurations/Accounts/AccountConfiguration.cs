using GameServer.Domain.Common;
using GameServer.Domain.Entities;
using GameServer.Domain.ValueObjects.Accounts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Game.Persistence.Configurations.Accounts;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.HasBaseType<BaseAuditableEntity>();

        builder.ToTable("Accounts");

        builder.OwnsOne(a => a.BanInfo, ban =>
        {
            ban.Property(b => b.Status)
                .HasColumnName("BanStatus")
                .HasConversion<string>()
                .IsRequired();

            ban.Property(b => b.ExpiresAt)
                .HasColumnName("BanExpiresAt");

            ban.Property(b => b.Reason)
                .HasColumnName("BanReason")
                .HasMaxLength(500);

            ban.Property(b => b.BannedById)
                .HasColumnName("BannedById");
        });

        builder.Property(a => a.LastLoginInfo)
            .HasConversion(
                vo => LoginInfoConverters.ToProvider(vo),
                str => LoginInfoConverters.FromProvider(str)
            )
            .HasColumnName("LastLoginInfo")
            .HasMaxLength(100)
            .IsRequired(false);

        // Characters relationship configuration
        builder.HasMany(a => a.Characters)
            .WithOne(c => c.Account)
            .HasForeignKey(c => c.AccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public static class LoginInfoConverters
{
    public static string? ToProvider(LoginInfo? vo) =>
        vo is null
            ? null
            : $"{vo.LastLoginIp}|{vo.LastLoginDate:o}";

    public static LoginInfo? FromProvider(string? str)
    {
        if (string.IsNullOrEmpty(str))
            return null;

        var parts = str.Split('|', 2);
        var date = DateTime.Parse(
            parts[1],
            null,
            System.Globalization.DateTimeStyles.RoundtripKind
        );
        return LoginInfo.Create(parts[0], date);
    }
}
