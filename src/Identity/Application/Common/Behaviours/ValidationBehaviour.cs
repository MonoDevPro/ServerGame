using Microsoft.Extensions.Logging;
using ValidationException = ServerGame.Application.Common.Exceptions.ValidationException;

namespace ServerGame.Application.Common.Behaviours;

public class ValidationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
     where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    private readonly ILogger<ValidationBehaviour<TRequest, TResponse>> _logger;

    public ValidationBehaviour(IEnumerable<IValidator<TRequest>> validators, ILogger<ValidationBehaviour<TRequest, TResponse>> logger)
    {
        _validators = validators;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);

            var validationResults = await Task.WhenAll(
                _validators.Select(v =>
                    v.ValidateAsync(context, cancellationToken)));

            var failures = validationResults
                .Where(r => r.Errors.Any())
                .SelectMany(r => r.Errors)
                .ToList();
            
            // Logue cada falha com nome da propriedade e mensagem
            foreach (var failure in failures)
            {
                _logger.LogWarning(
                    "[Validation] {RequestType}.{PropertyName}: {ErrorMessage}",
                    typeof(TRequest).Name,
                    failure.PropertyName,
                    failure.ErrorMessage);
            }

            if (failures.Any())
                throw new ValidationException(failures);
        }
        return await next();
    }
}
