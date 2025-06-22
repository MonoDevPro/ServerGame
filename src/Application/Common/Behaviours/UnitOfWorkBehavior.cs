using GameServer.Application.Common.Interfaces.Persistence;
using Microsoft.Extensions.Logging;

namespace GameServer.Application.Common.Behaviours;

/// <summary>
/// Pipeline behavior do MediatR que dispara SaveChanges no final de cada request.
/// </summary>
/// <typeparam name="TRequest"></typeparam>
/// <typeparam name="TResponse"></typeparam>
public class UnitOfWorkBehavior<TRequest, TResponse>(
    IUnitOfWork unitOfWork, 
    ILogger<UnitOfWorkBehavior<TRequest, TResponse>> logger
    ) : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        // Executa o handler
        var response = await next(ct);

        // Persiste alterações após a execução
        var result = await unitOfWork.SaveChangesAsync(ct);
        
        logger.LogInformation("Save changes for request {RequestType} HasSaves: {Saves}", typeof(TRequest).Name, result);
        
        return response;
    }
}
