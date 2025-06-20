using GameServer.Application.Accounts.Queries.Models;
using GameServer.Domain.Entities;

namespace GameServer.Application.Accounts.Extensions;

public static class AccountUpdateExtension
{
    public static Account Update(
        this Account account, AccountDto accountDto)
    {
        ArgumentNullException.ThrowIfNull(account);
        ArgumentNullException.ThrowIfNull(accountDto);

        if (accountDto.IsActive != account.IsActive)
            throw new InvalidOperationException("Cannot update IsActive status directly. Use a dedicated method for that.");
        if (accountDto.AccountType != account.AccountType)
            throw new InvalidOperationException("Cannot update AccountType directly. Use a dedicated method for that.");
        
        return account;
    }
}
