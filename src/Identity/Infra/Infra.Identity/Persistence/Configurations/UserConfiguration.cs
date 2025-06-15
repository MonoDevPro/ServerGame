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
        // Filtro global para soft delete
        builder.HasQueryFilter(a => a.IsActive);
            
    }
}
