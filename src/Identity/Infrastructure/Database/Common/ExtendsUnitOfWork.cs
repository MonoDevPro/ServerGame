using Calabonga.UnitOfWork;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ServerGame.Application.Common.Adapters;
using ServerGame.Application.Common.Interfaces.Dispatchers;
using ServerGame.Domain.Entities;
using ServerGame.Domain.Events;
using ServerGame.Infrastructure.Data.Context;

namespace ServerGame.Infrastructure.Database.Common;

public class ApplicationUnitOfWork : UnitOfWork<ApplicationDbContext>
{

    public ApplicationUnitOfWork(
        ApplicationDbContext dbContext,
        ILogger<ApplicationUnitOfWork> logger)
        : base(dbContext)
    {
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        // 1) antes do commit: coletar eventos (se quiser)
        var domainEvents = DbContext.ChangeTracker
            .Entries<IHasDomainEvents>()
            .SelectMany(e => e.Entity.Events)
            .ToList();

        // 2) chama o commit pai (SaveChangesAsync + commit de transaction)
        await base.CommitAsync(cancellationToken);

        // 3) depois do commit: despacha
        if (domainEvents.Any())
        {
            await _dispatcher.DispatchAsync(
                domainEvents
                    .Select(e => new DomainEventNotification<IDomainEvent>(e))
                    .ToList(),
                cancellationToken);
        }
    }
}
