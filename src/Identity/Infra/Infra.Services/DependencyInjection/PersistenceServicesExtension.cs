using System.Reflection;
using Ardalis.GuardClauses;
using Infra.Services.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ServerGame.Application.Accounts.Services;
using ServerGame.Application.Common.Interfaces.Persistence.Repository;
using ServerGame.Domain.Entities.Accounts;
using ServerGame.Infrastructure.Persistence.DbContexts;
using ServerGame.Infrastructure.Persistence.Repositories;
using ServerGame.Infrastructure.Persistence.Repositories.Reader;
using ServerGame.Infrastructure.Persistence.Repositories.Writer;

namespace Infra.Services.DependencyInjection;

public static class PersistenceServicesExtension
{
    public static IHostApplicationBuilder ConfigurePersistenceServices(
        this IHostApplicationBuilder builder,
        Action<DbContextOptionsBuilder, IServiceProvider> contextOptionsBuilder)
    {
        // Accounts
        builder.ConfigureAccountServices(contextOptionsBuilder);

        return builder;
    }
    
    private static IHostApplicationBuilder ConfigureAccountServices(
        this IHostApplicationBuilder hostBuilder,
        Action<DbContextOptionsBuilder, IServiceProvider> contextOptionsBuilder)
    {
        hostBuilder.Services.AddScoped<IAccountService, AccountService>();
        
        hostBuilder.Services.AddDbContext<GameDbContext>((sp, opt) =>
        {
            contextOptionsBuilder.Invoke(opt, sp);
        });
        
        // Registrar repositórios
        hostBuilder.RegisterRepositoriesFor<GameDbContext>(
            typeof(Account) /* entidades de domínio */);

        hostBuilder.EnrichNpgsqlDbContext<GameDbContext>();

        return hostBuilder;
    }
}
