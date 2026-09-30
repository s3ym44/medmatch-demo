using FluentValidation;
using MedMatch.Application.Behaviors;
using Microsoft.Extensions.DependencyInjection;

namespace MedMatch.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            // Sıra önemli: önce validation (geçersiz istek transaction açmaz), sonra transaction
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            cfg.AddOpenBehavior(typeof(TransactionBehavior<,>));
        });
        services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);
        return services;
    }
}
