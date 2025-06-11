using Calabonga.UnitOfWork;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using ServerGame.Application.Common.Interfaces.Database.Repository;
using ServerGame.Application.Common.Interfaces.Dispatchers;
using ServerGame.Domain.Entities.Accounts;
using ServerGame.Domain.Events;
using ServerGame.Infrastructure.Data;
using ServerGame.Infrastructure.Data.Context;
using ServerGame.Infrastructure.Data.Events;
using ServerGame.Infrastructure.Data.Repositories;
using ServerGame.Infrastructure.Data.Repositories.Reader;
using ServerGame.Infrastructure.Data.Repositories.Writer;
using ServerGame.Infrastructure.Database.Common;
using ServerGame.Infrastructure.Database.Common.Interceptors;
using ServerGame.Infrastructure.Database.Domain;
using ServerGame.Infrastructure.Database.Domain.Dispatchers;
using ServerGame.Infrastructure.Database.Domain.Interceptors;
using ServerGame.Infrastructure.Identity.Entities;

namespace ServerGame.Infrastructure.Database;

public static class DatabaseServicesExtension
{
    
    public static IHostApplicationBuilder ConfigureDatabaseServices(
        this IHostApplicationBuilder hostBuilder,
        string connectionName,
        Action<NpgsqlDbContextOptionsBuilder>? postgreDbContextSettings = null,
        Action<DbContextOptionsBuilder>? optionsBuilder = null)
    {
        hostBuilder.Services.AddScoped<INotificationDispatcher<INotification>, NotificationDispatcher>();
        hostBuilder.Services.AddScoped<IEventDispatcher<IDomainEvent>, EventDispatcher>();
        
        // Vai ser utilizado no UNIT OF WORK do Calabonga
        hostBuilder.Services.AddScoped<ISaveChangesInterceptor, UnitOfWorkInterceptor>();
        
        // Registra interceptors de pré e pós salvamento adaptado para o Calabonga Unit of Work
        hostBuilder.Services.AddScoped<IEnumerable<IPreSaveInterceptor>>(sp 
            => sp.GetServices<IPreSaveInterceptor>());
        hostBuilder.Services.AddScoped<IEnumerable<IPostSaveInterceptor>>(sp 
            => sp.GetServices<IPostSaveInterceptor>());
        
        // Add interceptor de pre e pós salvamento
        // Auditable interceptor
        hostBuilder.Services.AddScoped<IPreSaveInterceptor, UnityOfWorkAuditableInterceptor>();
        // Post events and notifications interceptors
        hostBuilder.Services.AddScoped<EntityEventDispatcherInterceptor>();
        hostBuilder.Services.AddScoped<IPreSaveInterceptor>(sp => sp.GetRequiredService<EntityEventDispatcherInterceptor>());
        hostBuilder.Services.AddScoped<IPostSaveInterceptor>(sp => sp.GetRequiredService<EntityEventDispatcherInterceptor>());

        hostBuilder.Services.AddUnitOfWork<ApplicationDbContext, DomainDbContext>();
        
        // Configura o DbContext e os interceptors
        ConfigureDatabaseServicesWithAction<CommonDbContext>(hostBuilder, connectionName, reg =>
        {
            reg.AddEntity<Account>();
            reg.AddEntity<ApplicationUser>();
        }, opt =>
        {
            opt.MigrationsAssembly(typeof(CommonDbContext).Assembly);
        }, 
            optionsBuilder);

        ConfigureDatabaseServicesWithAction<ApplicationDbContext>(hostBuilder, connectionName, reg =>
            {
                reg.AddEntity<ApplicationUser>();
            },
            postgreDbContextSettings, optionsBuilder);
        
        ConfigureDatabaseServicesWithAction<DomainDbContext>(hostBuilder, connectionName, reg =>
        {
            reg.AddEntity<Account>();
        }, 
            postgreDbContextSettings, optionsBuilder);
        
        return hostBuilder;
    }
    
    /// <summary>
    /// Configura os serviços de banco de dados com suporte para múltiplas entidades usando expressões lambda
    /// </summary>
    /// <param name="hostBuilder">Coleção de serviços</param>
    /// <param name="connectionName">Nome da string de conexão</param>
    /// <param name="entityConfigurator">Configurador de entidades</param>
    /// <param name="postgreDbContextSettings">Configurações do provedor PostgreSQL</param>
    /// <param name="optionsBuilder">Configurações adicionais do contexto</param>
    private static IHostApplicationBuilder ConfigureDatabaseServicesWithAction<TContext>(
        this IHostApplicationBuilder hostBuilder,
        string connectionName,
        Action<IEntityTypeRegistrar<TContext>> entityConfigurator,
        Action<NpgsqlDbContextOptionsBuilder>? postgreDbContextSettings = null,
        Action<DbContextOptionsBuilder>? optionsBuilder = null)
        where TContext : DbContext
    {
        // Configura o DbContext e os interceptors
        ConfigureDbContext<TContext>(hostBuilder, connectionName, postgreDbContextSettings, optionsBuilder);
        
        // Usa o registrador para configurar as entidades
        var registrar = new EntityTypeRegistrar<TContext>(hostBuilder);
        entityConfigurator(registrar);
        
        return hostBuilder;
    }

    /// <summary>
    /// Interface para registro de entidades
    /// </summary>
    public interface IEntityTypeRegistrar<TContext> where TContext : DbContext
    {
        /// <summary>
        /// Registra um tipo de entidade para uso com repositórios
        /// </summary>
        IEntityTypeRegistrar<TContext> AddEntity<TEntity>() where TEntity : class;
    }

    /// <summary>
    /// Implementação do registrador de entidades
    /// </summary>
    private class EntityTypeRegistrar<TContext> : IEntityTypeRegistrar<TContext> where TContext : DbContext
    {
        private readonly IHostApplicationBuilder _hostBuilder;

        public EntityTypeRegistrar(IHostApplicationBuilder hostBuilder)
        {
            _hostBuilder = hostBuilder;
        }

        public IEntityTypeRegistrar<TContext> AddEntity<TEntity>() where TEntity : class
        {
            RegisterEntityRepositories<TEntity, TContext>(_hostBuilder);
            return this;
        }
    }

    /// <summary>
    /// Registra os repositórios para um tipo de entidade específico
    /// </summary>
    private static void RegisterEntityRepositories<TEntity, TContext>(IHostApplicationBuilder hostBuilder)
        where TEntity : class
        where TContext : DbContext
    {
        // Adiciona o repositório de escrita
        hostBuilder.Services.AddScoped<IWriterRepository<TEntity>>(opt =>
        {
            var dbContext = opt.GetRequiredService<TContext>();
            return new WriterRepository<TEntity>(dbContext, opt.GetRequiredService<ILogger<WriterRepository<TEntity>>>());
        });
        
        // Adiciona o repositório de leitura
        hostBuilder.Services.AddScoped<IReaderRepository<TEntity>>(opt =>
        {
            var dbContext = opt.GetRequiredService<TContext>();
            return new ReaderRepository<TEntity>(dbContext.Set<TEntity>().AsQueryable());
        });
        
        // Registra o compose de repositórios
        hostBuilder.Services.AddScoped<IRepositoryCompose<TEntity>>(opt =>
        {
            var writerRepo = opt.GetRequiredService<IWriterRepository<TEntity>>();
            var readerRepo = opt.GetRequiredService<IReaderRepository<TEntity>>();
            return new RepositoryCompose<TEntity>(writerRepo, readerRepo);
        });
    }

    /// <summary>
    /// Configura o DbContext e os interceptors
    /// </summary>
    private static void ConfigureDbContext<TContext>(
        IHostApplicationBuilder hostBuilder, 
        string connectionName,
        Action<NpgsqlDbContextOptionsBuilder>? postgreDbContextSettings,
        Action<DbContextOptionsBuilder>? optionsBuilder)
        where TContext : DbContext
    {
        hostBuilder.Services.AddDbContext<TContext>((sp, options) =>
        {
            var connectionString = hostBuilder.Configuration.GetConnectionString(connectionName);

            Guard.Against.Null(connectionString, message: "Connection string not found.");

            // Use the connection string named "serverdb" (will be injected by Aspire)
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());

            options.UseNpgsql(connectionString, postgreDbContextSettings)
                .AddAsyncSeeding(sp);

            optionsBuilder?.Invoke(options);
        });
        
        // Register the DbContext as a design-time factory
        hostBuilder.EnrichNpgsqlDbContext<TContext>();
        
    }
}
