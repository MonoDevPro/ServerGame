using Microsoft.Extensions.Logging;

namespace GameServer.Application.Common.Behaviours;

public class ValidationBehaviour<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators,
    ILogger<ValidationBehaviour<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        if (validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);

            var validationResults = await Task.WhenAll(
                validators.Select(v =>
                    v.ValidateAsync(context, ct)));

            var failures = validationResults
                .Where(r => r.Errors.Any())
                .SelectMany(r => r.Errors)
                .ToList();
            
            // Logue cada falha com nome da propriedade e mensagem
            foreach (var failure in failures)
            {
                logger.LogWarning(
                    "[Validation] {RequestType}.{PropertyName}: {ErrorMessage}",
                    typeof(TRequest).Name,
                    failure.PropertyName,
                    failure.ErrorMessage);
            }

            if (failures.Any())
                throw new ValidationException(failures);
        }
        return await next(ct);
    }
}
