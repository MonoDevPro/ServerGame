namespace ServerGame.Application.Common.Interfaces.Database;

public interface IDatabaseSeeding
{
    Task InitialiseAsync();
    Task SeedAsync();
    Task TrySeedAsync();
}
