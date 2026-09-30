using FluentValidation;
using MedMatch.Application.Common;
using MediatR;

namespace MedMatch.Application.Behaviors;

/// <summary>İsteğin validator'larını çalıştırır; ilk hatayı 400 (AppException.Validation) olarak döner.</summary>
public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators) => _validators = validators;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        foreach (var validator in _validators)
        {
            var result = await validator.ValidateAsync(request, ct);
            if (!result.IsValid)
                throw AppException.Validation(result.Errors[0].ErrorMessage);
        }
        return await next();
    }
}
