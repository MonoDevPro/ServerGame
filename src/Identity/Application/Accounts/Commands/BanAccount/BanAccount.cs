using System.ComponentModel.DataAnnotations;
using GameServer.Shared.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using ServerGame.Application.Common.Interfaces.Database;
using ServerGame.Application.Common.Interfaces.Database.Repository;
using ServerGame.Application.Common.Security;
using ServerGame.Domain.Constants;
using ServerGame.Domain.Entities;
using ServerGame.Domain.Enums;
using ServerGame.Domain.ValueObjects;

namespace ServerGame.Application.Accounts.Commands.BanAccount;

[Authorize(Roles = Roles.Administrator)]
public record BanAccountCommand(
    [Required] UsernameOrEmail UsernameOrEmail,
    [Required] string Reason,
    [Required] BanStatus BanType,
    DateTime? ExpiresAt = null,
    long? BannedById = null
) : IRequest;

public class BanAccountCommandHandler : IRequestHandler<BanAccountCommand>
{
    private readonly IReaderRepository<Account> _accountRepositoryReader;
    private readonly IWriterRepository<Account> _accountRepositoryWriter;
    private readonly ILogger<BanAccountCommandHandler> _logger;

    public BanAccountCommandHandler(
        IReaderRepository<Account> accountRepositoryReader,
        IWriterRepository<Account> accountRepositoryWriter,
        ILogger<BanAccountCommandHandler> logger
    )
    {
        _accountRepositoryReader = accountRepositoryReader;
        _accountRepositoryWriter = accountRepositoryWriter;
        _logger = logger;
    }

    public async Task Handle(BanAccountCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Buscar entidade de domínio
            var entity = await _accountRepositoryReader.QuerySingleAsync(
                a => a.Email.Value == request.UsernameOrEmail.Value 
                     || a.Username.Value == request.UsernameOrEmail.Value,
                account => account,
                trackingType: TrackingType.Tracking,
                cancellationToken: cancellationToken
            );
            
            Guard.Against.Null(entity, nameof(entity), "Account not found");
            
            // Criar informação de banimento
            BanInfo? banInfo = request.BanType switch
            {
                BanStatus.TemporaryBan when request.ExpiresAt.HasValue => 
                    BanInfo.CreateTemporary(request.Reason, request.ExpiresAt.Value, request.BannedById ?? 0),
                BanStatus.PermanentBan => 
                    BanInfo.CreatePermanent(request.Reason, request.BannedById ?? 0),
                BanStatus.NotBanned => 
                    BanInfo.NotBanned,
                _ => throw new ArgumentException("Invalid ban type or missing expiration date for temporary ban")
            };
            
            Guard.Against.Null(banInfo, nameof(banInfo), "Ban information could not be created");

            // Aplicar ban
            entity.UpdateBan(banInfo);

            // Salvar
            await _accountRepositoryWriter.UpdateAsync(entity, cancellationToken);
            
            var saveResult = await _accountRepositoryWriter.SaveChangesAsync(cancellationToken);

            Guard.Against.Default(
                saveResult, 
                nameof(saveResult), 
                "Account ban could not be updated in the database."
            );
        }
        catch (DomainException ex)
        {
            _logger.LogError(ex, "Domain error while banning account: {Message}", ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while banning account");
            throw new ApplicationException("An error occurred while banning the account.", ex);
        }
    }
}
