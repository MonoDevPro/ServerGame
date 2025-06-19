using GameServer.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameServer.Infrastructure.Persistence.Configurations.Common;

public class BaseDomainConfiguration : IEntityTypeConfiguration<BaseEntity>,  IEntityTypeConfiguration<BaseAuditableEntity>
{
    public void Configure(EntityTypeBuilder<BaseEntity> builder)
    {
        builder.UseTpcMappingStrategy();
        
        // Filtro global para soft delete
        builder.HasQueryFilter(a => a.IsActive);
        
        // Configurações comuns para entidades base
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();
        builder.Ignore(e => e.Events); // Ignora eventos de domínio para evitar loops infinitos
    }

    public void Configure(EntityTypeBuilder<BaseAuditableEntity> builder)
    {
        builder.HasBaseType<BaseEntity>();
        
        // Configurações específicas para entidades auditáveis
        builder.Property(e => e.CreatedBy).HasMaxLength(256);
        builder.Property(e => e.LastModifiedBy).HasMaxLength(256);
        builder.Property(e => e.Created).IsRequired();
        builder.Property(e => e.LastModified).IsRequired();
    }
}
