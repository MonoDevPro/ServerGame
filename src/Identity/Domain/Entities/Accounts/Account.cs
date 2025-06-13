using Ardalis.GuardClauses;
using ServerGame.Domain.Events.Accounts;
using ServerGame.Domain.Exceptions;
using ServerGame.Domain.ValueObjects.Accounts;

namespace ServerGame.Domain.Entities.Accounts;

public class Account : BaseAuditableEntity
{
    // Informações básicas da conta
    public Username Username { get; private set; }
    public Email Email { get; private set; }
    public bool IsActive { get; private set; }
    public AccountType AccountType { get; private set; } = AccountType.Player;

    // Informações de segurança
    public BanInfo? BanInfo { get; private set; }
    public LoginInfo? LastLoginInfo { get; private set; }

    // Permissões
    private readonly List<Role> _roles = [];
    public IReadOnlyCollection<Role> Roles => _roles.AsReadOnly();

    // Construtor para criação de nova conta
    private Account(Username username, Email email)
    {
        Username = Guard.Against.Null(username, nameof(username));
        Email = Guard.Against.Null(email, nameof(email));
        IsActive = true;
        BanInfo = BanInfo.NotBanned;
    }
    
    public static Account Create(Username username, Email email)
    {
        Guard.Against.Null(username, nameof(username));
        Guard.Against.Null(email, nameof(email));
        var account = new Account(username, email);
        
        account.AddRole(Role.Player);
        account.AddDomainEvent(new AccountDomainCreatedEvent(account));
        return account;
    }
    
    

    // Status
    public bool IsStaff() => AccountType == AccountType.Staff;
    public bool IsAdministrator() => AccountType == AccountType.Administrator;
    public bool HasRole(Role role) => _roles.Contains(role);
    public bool HasRole(string role) => _roles.Any(r => r.Value.Equals(role, StringComparison.OrdinalIgnoreCase));

    // Ativar / Desativar
    public void Activate()
    {
        if (IsActive) 
            return;
        IsActive = true;
        AddDomainEvent(new AccountDomainActivatedEvent(this));
    }

    public void Deactivate()
    {
        if (!IsActive) return;
        IsActive = false;
        AddDomainEvent(new AccountDomainDeactivatedEvent(this));
    }

    // Ban management
    public void UpdateBan(BanInfo banInfo)
    {
        Guard.Against.Null(banInfo, nameof(banInfo));
        if (BanInfo != null && BanInfo.Equals(banInfo)) return; // evita evento duplicado

        if (banInfo.IsActive() && AccountType == AccountType.Administrator)
            throw new DomainException("Administradores não podem ser banidos");

        BanInfo = banInfo;
        AddDomainEvent(new AccountDomainBanUpdatedEvent(this, banInfo));
    }

    // Autenticação e sessão
    public void RecordSuccessfulLogin(LoginInfo loginInfo)
    {
        Guard.Against.Null(loginInfo, nameof(loginInfo));
        if (!IsActive)
            throw new DomainException("Conta inativa não pode realizar login");

        if (BanInfo != null && BanInfo.IsActive())
            throw new DomainException($"Conta banida até {BanInfo.ExpiresAt}. Motivo: {BanInfo.Reason}");

        LastLoginInfo = loginInfo;
        AddDomainEvent(new AccountDomainLoggedIn(this, loginInfo));
    }

    public void RecordFailedLoginAttempt(LoginInfo loginInfo)
    {
        Guard.Against.Null(loginInfo, nameof(loginInfo));
        AddDomainEvent(new AccountDomainLoginFailed(this, loginInfo));
    }

    public void RecordLogout()
    {
        AddDomainEvent(new AccountDomainLoggedOut(this));
    }

    // Atualização de dados
    public void UpdateEmail(Email newEmail)
    {
        Guard.Against.Null(newEmail, nameof(newEmail));
        if (newEmail.Equals(Email)) return;
        var previous = Email;
        Email = newEmail;
        AddDomainEvent(new AccountDomainEmailUpdatedEvent(this, previous, newEmail));
    }

    public void UpdateUsername(Username newUsername)
    {
        Guard.Against.Null(newUsername, nameof(newUsername));
        if (newUsername.Equals(Username)) return;
        var previous = Username;
        Username = newUsername;
        AddDomainEvent(new AccountDomainUsernameUpdatedEvent(this, previous, newUsername));
    }

    // Roles e tipo de conta
    public void AddRole(Role role)
    {
        Guard.Against.Null(role, nameof(role));
        if (_roles.Contains(role)) return;

        if (role == Role.Admin && AccountType != AccountType.Administrator)
            throw new DomainException("Apenas administradores podem ter a role admin");

        _roles.Add(role);
        AddDomainEvent(new AccountDomainRoleAddedEvent(this, role));
    }

    public void RemoveRole(Role role)
    {
        Guard.Against.Null(role, nameof(role));
        if (!_roles.Contains(role)) return;
        _roles.Remove(role);
        AddDomainEvent(new AccountDomainRoleRemovedEvent(this, role));
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
        AddDomainEvent(new AccountDomainTypeChangedEvent(this, previous, AccountType));
    }

    public void PromoteTo(AccountType newType)
    {
        if (!CanBePromotedTo(newType))
            throw new DomainException($"Não é possível promover a conta para {newType}");

        var previous = AccountType;
        AccountType = newType;

        // adiciona role admin automaticamente para admins
        if (newType == AccountType.Administrator && !HasRole(Role.Admin))
            _roles.Add(Role.Admin);

        AddDomainEvent(new AccountDomainTypeChangedEvent(this, previous, AccountType));
    }

    public bool HasPermissionTo(string action)
    {
        if (AccountType == AccountType.Administrator)
            return true;

        return action.ToLowerInvariant() switch
        {
            "moderate_chat" => Roles.Any(r => r.CanModerateChat()) || IsStaff(),
            "manage_accounts" => Roles.Any(r => r.CanManageAccounts()),
            "view_reports" => HasRole(Role.Moderator) || HasRole(Role.Support) || IsStaff(),
            "access_vip_area" => HasRole(Role.Vip) || AccountType == AccountType.VIP || IsStaff(),
            "create_game_items" => HasRole(Role.GameMaster) || IsStaff(),
            "manage_servers" => IsAdministrator(),
            _ => false
        };
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
            AccountType.Administrator => HasRole(Role.Admin) && IsStaff(),
            _ => false
        };
    }

    private bool HasMinimumRequirementsForStaff()
        => HasRole(Role.Moderator) || HasRole(Role.Support) || HasRole(Role.GameMaster);
}
