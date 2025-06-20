using GameServer.Domain.Events.Accounts.Base;

namespace GameServer.Domain.Events.Accounts;

/// <summary>
/// Evento disparado quando um novo usuário é registrado no sistema
/// </summary>
public class AccountCreatedEvent(Account account) : AccountEvent(account);
