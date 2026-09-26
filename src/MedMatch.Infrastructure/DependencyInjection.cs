using MedMatch.Application.Abstractions;
using MedMatch.Application.Services;
using MedMatch.Infrastructure.Persistence;
using MedMatch.Infrastructure.Persistence.Repositories;
using MedMatch.Infrastructure.Security;
using MedMatch.Infrastructure.Seed;
using MedMatch.Infrastructure.Verification;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MedMatch.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // JWT ayarları (appsettings > "Jwt")
        var jwt = new JwtOptions();
        configuration.GetSection("Jwt").Bind(jwt);
        services.AddSingleton(jwt);

        // Depo (demo: süreç içi tek örnek) — üretimde EF Core + PostgreSQL ile değişir
        services.AddSingleton<InMemoryStore>();

        // Altyapı servisleri
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddSingleton<ITokenService, JwtTokenService>();
        services.AddSingleton<IVerificationService, MockVerificationService>();
        services.AddSingleton<DemoSeeder>();

        // Repository'ler
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IProfileRepository, ProfileRepository>();
        services.AddScoped<IVerificationRepository, VerificationRepository>();
        services.AddScoped<ISwipeRepository, SwipeRepository>();
        services.AddScoped<IMatchRepository, MatchRepository>();
        services.AddScoped<IMessageRepository, MessageRepository>();

        // Application servisleri
        services.AddScoped<AuthService>();
        services.AddScoped<ProfileService>();
        services.AddScoped<VerificationAppService>();
        services.AddScoped<DiscoveryService>();
        services.AddScoped<MatchingService>();
        services.AddScoped<ChatService>();

        return services;
    }
}
