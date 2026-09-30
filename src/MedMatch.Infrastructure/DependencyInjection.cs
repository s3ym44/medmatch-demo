using MedMatch.Application.Abstractions;
using MedMatch.Infrastructure.Persistence;
using MedMatch.Infrastructure.Persistence.Repositories;
using MedMatch.Infrastructure.Security;
using MedMatch.Infrastructure.Seed;
using MedMatch.Infrastructure.Verification;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MedMatch.Infrastructure;

public static class DependencyInjection
{
    /// <summary>"Database:Provider" değerini okur: InMemory (kurulumsuz demo) veya Postgres. Varsayılan Postgres.</summary>
    public static bool UseInMemoryDatabase(IConfiguration configuration)
    {
        var provider = configuration["Database:Provider"] ?? "Postgres";
        if (provider.Equals("InMemory", StringComparison.OrdinalIgnoreCase)) return true;
        if (provider.Equals("Postgres", StringComparison.OrdinalIgnoreCase)) return false;
        throw new InvalidOperationException($"Bilinmeyen Database:Provider '{provider}'. Geçerli değerler: InMemory, Postgres.");
    }

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // JWT ayarları (appsettings > "Jwt")
        var jwt = new JwtOptions();
        configuration.GetSection("Jwt").Bind(jwt);
        services.AddSingleton(jwt);

        // Altyapı servisleri
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddSingleton<ITokenService, JwtTokenService>();
        services.AddSingleton<IVerificationService, MockVerificationService>();

        // Repository'ler — provider'a göre (arayüzler aynı, üst katmanlar değişmez)
        if (UseInMemoryDatabase(configuration))
        {
            services.AddSingleton<InMemoryStore>(); // süreç içi tek örnek
            services.AddScoped<IUnitOfWork, InMemoryUnitOfWork>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IProfileRepository, ProfileRepository>();
            services.AddScoped<IVerificationRepository, VerificationRepository>();
            services.AddScoped<ISwipeRepository, SwipeRepository>();
            services.AddScoped<IMatchRepository, MatchRepository>();
            services.AddScoped<IMessageRepository, MessageRepository>();
        }
        else
        {
            var conn = configuration.GetConnectionString("Postgres")
                ?? throw new InvalidOperationException("ConnectionStrings:Postgres tanımlı değil.");
            services.AddDbContext<AppDbContext>(o => o.UseNpgsql(conn));
            services.AddScoped<IUnitOfWork, EfUnitOfWork>();
            services.AddScoped<IUserRepository, EfUserRepository>();
            services.AddScoped<IProfileRepository, EfProfileRepository>();
            services.AddScoped<IVerificationRepository, EfVerificationRepository>();
            services.AddScoped<ISwipeRepository, EfSwipeRepository>();
            services.AddScoped<IMatchRepository, EfMatchRepository>();
            services.AddScoped<IMessageRepository, EfMessageRepository>();
        }

        // Demo verisi (repository arayüzleri üzerinden; iki modda da çalışır)
        services.AddScoped<DemoSeeder>();

        return services;
    }

    /// <summary>
    /// Açılışta veritabanını hazırlar: Postgres modunda bekleyen migration'ları uygular, sonra demo verisini yükler.
    /// </summary>
    public static async Task InitializeDatabaseAsync(this IServiceProvider services, IConfiguration configuration)
    {
        using var scope = services.CreateScope();
        var sp = scope.ServiceProvider;

        if (!UseInMemoryDatabase(configuration))
            await sp.GetRequiredService<AppDbContext>().Database.MigrateAsync();

        await sp.GetRequiredService<DemoSeeder>().SeedAsync();
    }
}
