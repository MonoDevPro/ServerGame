using GameServer.Domain.Common;
using GameServer.Domain.Entities;
using GameServer.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Game.Persistence.Configurations.Characters;

public class CharacterConfiguration : IEntityTypeConfiguration<Character>
{
    public void Configure(EntityTypeBuilder<Character> builder)
    {
        builder.HasBaseType<BaseAuditableEntity>();

        builder.ToTable("Characters");

        // Properties configuration
        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(c => c.Class)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(c => c.Level)
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(c => c.Experience)
            .IsRequired()
            .HasDefaultValue(0L);

        builder.Property(c => c.AccountId)
            .IsRequired();

        // Indexes
        builder.HasIndex(c => new { c.AccountId, c.Name })
            .IsUnique()
            .HasDatabaseName("IX_Characters_AccountId_Name");

        builder.HasIndex(c => c.AccountId)
            .HasDatabaseName("IX_Characters_AccountId");
    }
}
