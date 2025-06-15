using ServerGame.Application.Common.Interfaces.Persistence.Repository;

namespace ServerGame.Infrastructure.Persistence.Repositories;

public class RepositoryCompose<TEntity>(
    IWriterRepository<TEntity> writerRepository,
    IReaderRepository<TEntity> readerRepository)
    : IRepositoryCompose<TEntity>
    where TEntity : class
{
    public IWriterRepository<TEntity> WriterRepository { get; } = writerRepository;
    public IReaderRepository<TEntity> ReaderRepository { get; } = readerRepository;
}
