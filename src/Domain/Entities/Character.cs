using GameServer.Domain.Events.Characters;

namespace GameServer.Domain.Entities;

public class Character : BaseAuditableEntity
{
    public long AccountId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public CharacterClass Class { get; private set; }
    public int Level { get; private set; }
    public long Experience { get; private set; }

    // Navigation property
    public virtual Account Account { get; private set; } = null!;

    protected Character() { }

    public static Character Create(long accountId, string name, CharacterClass characterClass)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Character name cannot be null or empty", nameof(name));

        if (name.Length is < 3 or > 20)
            throw new ArgumentException("Character name must be between 3 and 20 characters", nameof(name));

        var character = new Character
        {
            AccountId = accountId,
            Name = name.Trim(),
            Class = characterClass,
            Level = 1,
            Experience = 0,
            IsActive = true
        };

        character.AddDomainEvent(new CharacterCreatedEvent(character));
        return character;
    }

    public void GainExperience(long amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Experience amount must be positive", nameof(amount));

        Experience += amount;

        // Simple level calculation (can be enhanced with more complex logic)
        var newLevel = CalculateLevel(Experience);
        if (newLevel > Level)
        {
            var previousLevel = Level;
            Level = newLevel;
            AddDomainEvent(new CharacterLevelUpEvent(this, previousLevel, newLevel));
        }
    }

    public void SetLevel(int level)
    {
        if (level <= 0)
            throw new ArgumentException("Level must be positive", nameof(level));

        if (level != Level)
        {
            var previousLevel = Level;
            Level = level;
            AddDomainEvent(new CharacterLevelUpEvent(this, previousLevel, level));
        }
    }

    public void Deactivate()
    {
        IsActive = false;
        AddDomainEvent(new CharacterDeactivatedEvent(this));
    }

    private static int CalculateLevel(long experience)
    {
        // Simple level calculation: every 1000 XP = 1 level
        // This can be enhanced with more complex formulas
        return (int)(experience / 1000) + 1;
    }
}
