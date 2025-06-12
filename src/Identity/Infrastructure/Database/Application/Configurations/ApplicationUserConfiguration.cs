using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServerGame.Domain.Entities.Accounts;
using ServerGame.Infrastructure.Database.Application.Identity.Entities;

namespace ServerGame.Infrastructure.Database.Application.Configurations;

/// <summary>
/// Configuração do Entity Framework para ApplicationUser
/// </summary>
public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        // 1) Ignora completamente a propriedade de navegação
        builder.Ignore(u => u.Account);

        // 2) Mapeia só a FK como coluna opcional
        builder.Property(u => u.AccountId)
            .HasColumnName("AccountId")
            .IsRequired(false);

        // 3) Ignora eventos de domínio
        builder.Ignore(u => u.PendingNotifications);
    }
}
