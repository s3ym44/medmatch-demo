using MedMatch.Application.Abstractions;
using MedMatch.Application.Common;
using MediatR;

namespace MedMatch.Application.Behaviors;

/// <summary>Command'ları tek transaction'da çalıştırır (ör. swipe + eşleşme atomik). Query'lere dokunmaz.</summary>
public sealed class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IUnitOfWork _uow;
    public TransactionBehavior(IUnitOfWork uow) => _uow = uow;

    public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
        => request is ICommand
            ? _uow.ExecuteInTransactionAsync(() => next(), ct)
            : next();
}
