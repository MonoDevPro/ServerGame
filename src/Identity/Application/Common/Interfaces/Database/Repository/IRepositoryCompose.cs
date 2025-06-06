namespace ServerGame.Application.Common.Interfaces.Database.Repository;

public interface IRepositoryCompose<TEntity>
    where TEntity : class
{
    public IWriterRepository<TEntity> WriterRepository { get; }
    public IReaderRepository<TEntity> ReaderRepository { get; }
}
