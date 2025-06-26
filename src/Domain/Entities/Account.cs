using GameServer.Domain.Events.Accounts;
using GameServer.Domain.ValueObjects.Accounts;

namespace GameServer.Domain.Entities;

public class Account : BaseAuditableEntity
{
    private readonly List<Character> _characters = [];

    // Propriedade virtual de navegação para o usuário.
    public AccountType AccountType { get; private set; } = AccountType.Player;
    public BanInfo? BanInfo { get; private set; }
    public LoginInfo? LastLoginInfo { get; private set; }

    // Characters collection
    public IReadOnlyCollection<Character> Characters => _characters.AsReadOnly();

    protected Account() { }

    public static Account Create()
    {
        var account = new Account
        {
            IsActive = true
        };
        account.AddDomainEvent(new AccountCreatedEvent(account));

        return account;
    }

    // Status
    public bool IsStaff() => AccountType == AccountType.Staff;
    public bool IsAdministrator() => AccountType == AccountType.Administrator;

    // Ativar / Desativar
    public void Activate()
    {
        if (IsActive) return;

        IsActive = true;
        AddDomainEvent(new AccountActivatedEvent(this));
    }

    public void Deactivate()
    {
        if (!IsActive) return;

        IsActive = false;
        AddDomainEvent(new AccountDeactivatedEvent(this));
    }

    // Ban management
    public void UpdateBan(BanInfo banInfo)
    {
        if (BanInfo != null && BanInfo.Equals(banInfo)) return; // evita evento duplicado

        if (banInfo.IsActive() && AccountType == AccountType.Administrator)
            throw new DomainException("Administradores não podem ser banidos");

        BanInfo = banInfo;
        AddDomainEvent(new AccountBanUpdatedEvent(this, banInfo));
    }

    // Autenticação e sessão
    public void Login(LoginInfo loginInfo)
    {
        if (!IsActive)
            throw new DomainException("Conta inativa não pode realizar login");

        if (BanInfo != null && BanInfo.IsActive())
            throw new DomainException($"Conta banida até {BanInfo.ExpiresAt}. Motivo: {BanInfo.Reason}");

        LastLoginInfo = loginInfo;
        AddDomainEvent(new AccountLoggedIn(this, loginInfo));
    }

    public void LoginAttemptFailed(LoginInfo loginInfo)
    {
        AddDomainEvent(new AccountLoginFailed(this, loginInfo));
    }

    public void Logout()
    {
        AddDomainEvent(new AccountLoggedOut(this));
    }

    public void PromoteToStaff()
    {
        if (AccountType == AccountType.Staff) return;
        if (BanInfo != null && BanInfo.IsActive())
            throw new DomainException("Contas banidas não podem ser promovidas a Staff");
        if (!IsActive)
            throw new DomainException("Apenas contas ativas podem se tornar Staff");
        if (!HasMinimumRequirementsForStaff())
            throw new DomainException("Conta não possui requisitos mínimos para virar Staff");

        var previous = AccountType;
        AccountType = AccountType.Staff;
        AddDomainEvent(new AccountTypeChangedEvent(this, previous, AccountType));
    }

    public void PromoteTo(AccountType newType)
    {
        if (!CanBePromotedTo(newType))
            throw new DomainException($"Não é possível promover a conta para {newType}");

        var previous = AccountType;
        AccountType = newType;

        AddDomainEvent(new AccountTypeChangedEvent(this, previous, AccountType));
    }

    public bool CanBePromotedTo(AccountType newType)
    {
        if (newType <= AccountType) return false;
        if (BanInfo != null && BanInfo.IsActive()) return false;
        if (!IsActive) return false;

        return newType switch
        {
            AccountType.VIP => true,
            AccountType.Staff => HasMinimumRequirementsForStaff(),
            AccountType.Administrator => IsStaff(), // Removido a exigência de HasRole(Role.Admin)
            _ => false
        };
    }

    private bool HasMinimumRequirementsForStaff()
        => true;

    // Character management
    public void AddCharacter(Character character)
    {
        if (character == null)
            throw new ArgumentNullException(nameof(character));

        if (character.AccountId != Id)
            throw new DomainException("Character AccountId must match the Account Id");

        if (_characters.Count >= 3)
            throw new DomainException("Account cannot have more than 3 characters");

        if (_characters.Any(c => c.Name.Equals(character.Name, StringComparison.OrdinalIgnoreCase)))
            throw new DomainException($"Character with name '{character.Name}' already exists for this account");

        _characters.Add(character);
        AddDomainEvent(new CharacterAddedToAccountEvent(this, character));
    }

    public Character? GetCharacterByName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        return _characters.FirstOrDefault(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    public void RemoveCharacter(Character character)
    {
        if (character == null)
            throw new ArgumentNullException(nameof(character));

        _characters.Remove(character);
    }
}
