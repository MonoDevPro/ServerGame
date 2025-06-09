using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServerGame.Infrastructure.Identity;
using ServerGame.Infrastructure.Identity.Entities;

namespace ServerGame.Infrastructure.Data.Configurations;

/// <summary>
/// Configuração do Entity Framework para ApplicationUser
/// </summary>
public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        // Configurar relacionamento com Account (opcional)
        builder.HasOne(u => u.Account)
            .WithOne()
            .HasForeignKey<ApplicationUser>(u => u.AccountId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        // Configurar AccountId
        builder.Property(u => u.AccountId)
            .IsRequired(false);

        // Ignorar eventos de domínio (não persistir no banco)
        builder.Ignore(u => u.Notifications);
    }
}
