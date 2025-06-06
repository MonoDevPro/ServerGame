using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServerGame.Domain.Entities;
using ServerGame.Domain.ValueObjects;

namespace ServerGame.Infrastructure.Data.Configurations;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        // Configurações de propriedades
        builder.HasKey(a => a.Id);
        builder.Property(a => a.IsActive);
        builder.Property(a => a.AccountType);
        builder.Property(a => a.Created);
        builder.Ignore(a => a.DomainEvents); // Ignora eventos de domínio para evitar loops infinitos

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

        // BanInfo pode ser mapeado como OwnsOne se for um VO complexo
        builder.OwnsOne<BanInfo>(a => a.BanInfo, banInfo =>
        {
            banInfo.Property(b => b.Status).HasColumnName("BanStatus");
            banInfo.Property(b => b.ExpiresAt).HasColumnName("BanExpiresAt");
            banInfo.Property(b => b.Reason).HasColumnName("BanReason");
            banInfo.Property(b => b.BannedById).HasColumnName("BannedById");
        });

        // LoginInfoVO como propriedade complexa (OwnsOne)
        builder.OwnsOne<LoginInfo>(a => a.LastLoginInfo, loginInfo =>
        {
            loginInfo.Property(l => l.LastLoginIp)
                .HasColumnName("LastLoginIp")
                .IsRequired();
            loginInfo.Property(l => l.LastLoginDate)
                .HasColumnName("LastLoginDate")
                .IsRequired();
        });

        // Configuração do relacionamento com as roles
        
        builder.OwnsMany<Role>(
            a => a.Roles,
            roles =>
            {
                roles.WithOwner().HasForeignKey("AccountId");
                roles.Property(r => r.Value).HasColumnName("Role");
                roles.HasKey("AccountId", "Role");
                roles.ToTable("AccountRoles");
            });
    }
}
