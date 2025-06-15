using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServerGame.Domain.Entities;
using ServerGame.Domain.Entities.Accounts;
using ServerGame.Domain.ValueObjects.Accounts;

namespace ServerGame.Infrastructure.Persistence.Configurations.Accounts;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.HasBaseType<BaseAuditableEntity>();
        
        builder.ToTable("Accounts");
        
        // Configurações de propriedades
        builder.Property(a => a.IsActive);
        builder.Property(a => a.AccountType);

        // Configuração dos Value Objects com conversores customizados
        builder.Property(a => a.Email)
            .HasConversion(
                v => v.Value, // Para o banco
                v => Email.Create(v) // Para o domínio
            )
            .HasColumnName("Email").IsRequired();

        builder.Property(a => a.Username)
            .HasConversion(
                v => v.Value,
                v => Username.Create(v)
            )
            .HasColumnName("Username").IsRequired();


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
                vo   => LoginInfoConverters.ToProvider(vo),
                str  => LoginInfoConverters.FromProvider(str)
            )
            .HasColumnName("LastLoginInfo")
            .HasMaxLength(100)
            .IsRequired(false);
        
        builder.OwnsMany(a => a.Roles, roles =>
        {
            roles.WithOwner().HasForeignKey("AccountId");
            roles.ToTable("AccountRoles");

            // converter e nome de coluna
            roles.Property(a => a.Value)
                .HasColumnName("Role")
                .HasConversion(
                    v => v,
                    v => Role.Create(v)!
                );

            // chave primária composta: shadow AccountId + CLR Value
            roles.HasKey("AccountId", nameof(Role.Value));
        });
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
