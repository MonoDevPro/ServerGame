using Infra.Identity.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infra.Identity.Persistence.Configurations;

/// <summary>
/// Configuração do Entity Framework para ApplicationUser e entidades de Identity relacionadas
/// </summary>
public class UserConfiguration : 
    IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.UseTpcMappingStrategy();
        
        // Filtro global para soft delete
        builder.HasQueryFilter(a => a.IsActive);
        
        // Configurações de propriedades
        builder.Property(u => u.UserName)
            .IsRequired()
            .HasMaxLength(256);
        builder.Property(u => u.NormalizedUserName)
            .IsRequired()
            .HasMaxLength(256);
        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(256);
        builder.Property(u => u.NormalizedEmail)
            .IsRequired()
            .HasMaxLength(256);
    }
}
