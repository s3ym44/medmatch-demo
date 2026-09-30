using MediatR;

namespace MedMatch.Application.Common;

/// <summary>Durum değiştiren istek. <see cref="Behaviors.TransactionBehavior{TRequest,TResponse}"/> tek transaction'da çalıştırır.</summary>
public interface ICommand { }

public interface ICommand<out TResponse> : IRequest<TResponse>, ICommand { }

/// <summary>Salt okuma isteği; transaction açılmaz.</summary>
public interface IQuery<out TResponse> : IRequest<TResponse> { }
