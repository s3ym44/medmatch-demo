# Production'a geçiş: in-memory → EF Core + PostgreSQL

Demo, dış NuGet paketi olmayan bir ortamda üretildiği için persistence in-memory, auth elle HS256,
mediator düz servislerdi. Bu doküman, kendi makinende (NuGet açık) bunları gerçek altyapıya
taşıman için sırayla uygulanacak commit'leri verir.

Mimari sözleşme korunuyor: repository ve servis **arayüzleri** sabit. Değişen tek şey
Infrastructure'daki implementasyonlar ve DI. Domain ve Application'a dokunulmaz (tek istisna: seeder'ı
depo-bağımsız hale getiren küçük refactor).

Ön koşullar: .NET 8 SDK, Docker (Postgres için), `dotnet tool install --global dotnet-ef` (EF CLI).

---

## Risk notları (baştan bil)

- **Paket sürümü SDK ile eşleşsin.** .NET 8 kullanıyorsun → EF Core **8.0.x** ve Npgsql sağlayıcısı
  **8.0.x**. EF 9'a çıkma (o .NET 9 ister).
- **Guid PK'ler domain'de üretiliyor.** `Entity.Id` zaten `Guid.NewGuid()`. EF'in değer üretmesini
  kapat: her key için `ValueGeneratedNever()`. Yoksa EF boş Guid bekler ve çakışır.
- **`DoctorProfile.Photos` salt-okunur, arkasında `_photos` alanı var.** EF'e alan erişimi (field
  access) söylemen gerekir, yoksa koleksiyonu materyalize edemez.
- **`AgeRange` value object, parametresiz ctor'u yok.** Owned type olarak `AgeMin`/`AgeMax`
  kolonlarına eşlenir; EF 8 ctor binding'i `Min`/`Max` isimleriyle eşleştirir.
- **`DateTimeOffset` → `timestamptz`, `DateOnly` → `date`.** Npgsql 8 ikisini de otomatik yapar,
  ek konfigürasyon gerekmez. Ama Postgres'e yazarken `DateTimeOffset` UTC olmalı (kodda zaten
  `DateTimeOffset.UtcNow`).
- **SaveChanges nerede?** Demo repoları her çağrıda anında "kaydediyordu". Aynı davranışı korumak
  için EF repolarında mutasyon metotları `SaveChangesAsync` çağırır. Application'ı hiç değiştirmeden
  çalışır. Daha temiz yol (UnitOfWork) için en sonda not var.
- **Seeder InMemoryStore'a bağlı.** Sağlayıcıdan bağımsız olsun diye onu repository arayüzlerini
  kullanacak şekilde yeniden yazıyoruz (Adım 5). Böylece hem demo hem Postgres modunda çalışır.
- **Demo modunu kaybetme.** Bir `Database:Provider` bayrağıyla InMemory yolunu koruyoruz; kurulumsuz
  demo hâlâ çalışsın.

---

## Adım 1 — `build: EF Core + Npgsql paketleri ve docker-compose`

`src/MedMatch.Infrastructure/MedMatch.Infrastructure.csproj` içine paketleri ekle (FrameworkReference
kalsın):

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <ItemGroup>
    <FrameworkReference Include="Microsoft.AspNetCore.App" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.11" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.11" />
    <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.11" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\MedMatch.Application\MedMatch.Application.csproj" />
  </ItemGroup>
</Project>
```

Kök dizine `docker-compose.yml`:

```yaml
services:
  postgres:
    image: postgres:16
    environment:
      POSTGRES_DB: medmatch
      POSTGRES_USER: medmatch
      POSTGRES_PASSWORD: medmatch
    ports: ["5433:5432"] # 5432 host'ta yerel Postgres ile çakışıyor
    volumes: ["pgdata:/var/lib/postgresql/data"]
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U medmatch"]
      interval: 5s
      timeout: 5s
      retries: 5
  redis:
    image: redis:7
    ports: ["6379:6379"]
    healthcheck:
      test: ["CMD", "redis-cli", "ping"]
      interval: 5s
      timeout: 5s
      retries: 5
volumes:
  pgdata:
```

`src/MedMatch.Api/appsettings.json` → `ConnectionStrings` ve provider bayrağı ekle:

```json
{
  "Database": { "Provider": "Postgres" },
  "ConnectionStrings": {
    "Postgres": "Host=localhost;Port=5433;Database=medmatch;Username=medmatch;Password=medmatch"
  }
}
```

`docker compose up -d` ile Postgres + Redis'i ayağa kaldır.

---

## Adım 2 — `feat(infra): AppDbContext, entity configuration'lar, design-time factory`

`src/MedMatch.Infrastructure/Persistence/AppDbContext.cs`:

```csharp
using MedMatch.Domain.Matching;
using MedMatch.Domain.Messaging;
using MedMatch.Domain.Profiles;
using MedMatch.Domain.Users;
using MedMatch.Domain.Verification;
using Microsoft.EntityFrameworkCore;

namespace MedMatch.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<DoctorProfile> Profiles => Set<DoctorProfile>();
    public DbSet<ProfilePhoto> Photos => Set<ProfilePhoto>();
    public DbSet<VerificationRequest> Verifications => Set<VerificationRequest>();
    public DbSet<Swipe> Swipes => Set<Swipe>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<Message> Messages => Set<Message>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
}
```

`src/MedMatch.Infrastructure/Persistence/Configurations/` altına yapılandırmalar. Hepsi
`IEntityTypeConfiguration<T>`. Önemli olanlar:

```csharp
// UserConfiguration.cs
public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();
        b.Property(x => x.Email).IsRequired().HasMaxLength(256);
        b.HasIndex(x => x.Email).IsUnique();
        b.Property(x => x.PasswordHash).IsRequired();
    }
}

// DoctorProfileConfiguration.cs
public sealed class DoctorProfileConfiguration : IEntityTypeConfiguration<DoctorProfile>
{
    public void Configure(EntityTypeBuilder<DoctorProfile> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();
        b.HasIndex(x => x.UserId).IsUnique();          // 1-1 kullanıcı ilişkisi
        b.Property(x => x.DisplayName).IsRequired().HasMaxLength(120);
        b.Property(x => x.City).IsRequired().HasMaxLength(80);
        b.Property(x => x.VerificationStatus).HasConversion<int>(); // enum -> int (varsayılan zaten int)

        // AgeRange value object -> AgeMin / AgeMax kolonları
        b.OwnsOne(x => x.AgeRange, a =>
        {
            a.Property(p => p.Min).HasColumnName("AgeMin");
            a.Property(p => p.Max).HasColumnName("AgeMax");
        });
        b.Navigation(x => x.AgeRange).IsRequired();

        // Photos: salt-okunur koleksiyon, _photos backing field
        b.HasMany(x => x.Photos).WithOne().HasForeignKey(p => p.ProfileId).OnDelete(DeleteBehavior.Cascade);
        b.Navigation(x => x.Photos).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

// ProfilePhotoConfiguration.cs
public sealed class ProfilePhotoConfiguration : IEntityTypeConfiguration<ProfilePhoto>
{
    public void Configure(EntityTypeBuilder<ProfilePhoto> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();
        b.Property(x => x.Url).IsRequired();
    }
}

// VerificationRequestConfiguration.cs
public sealed class VerificationRequestConfiguration : IEntityTypeConfiguration<VerificationRequest>
{
    public void Configure(EntityTypeBuilder<VerificationRequest> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();
        b.Property(x => x.Method).HasConversion<int>();
        b.Property(x => x.Status).HasConversion<int>();
        b.HasIndex(x => x.UserId);
    }
}

// SwipeConfiguration.cs
public sealed class SwipeConfiguration : IEntityTypeConfiguration<Swipe>
{
    public void Configure(EntityTypeBuilder<Swipe> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();
        b.Property(x => x.Decision).HasConversion<int>();
        b.HasIndex(x => new { x.SwiperId, x.TargetId }).IsUnique(); // aynı kişiye iki oy yok
    }
}

// MatchConfiguration.cs
public sealed class MatchConfiguration : IEntityTypeConfiguration<Match>
{
    public void Configure(EntityTypeBuilder<Match> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();
        b.HasIndex(x => new { x.UserAId, x.UserBId }).IsUnique();
        b.ToTable(t => t.HasCheckConstraint("CK_Match_Canonical", "\"UserAId\" < \"UserBId\""));
    }
}

// MessageConfiguration.cs
public sealed class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();
        b.Property(x => x.Content).IsRequired();
        b.HasIndex(x => x.MatchId);
        b.HasIndex(x => x.SentAt);
    }
}
```

Design-time factory (EF CLI'nin API'yi ayağa kaldırmadan migration üretebilmesi için):

```csharp
// Persistence/DesignTimeDbContextFactory.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MedMatch.Infrastructure.Persistence;

public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var conn = Environment.GetEnvironmentVariable("ConnectionStrings__Postgres")
            ?? "Host=localhost;Port=5433;Database=medmatch;Username=medmatch;Password=medmatch";
        var options = new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(conn).Options;
        return new AppDbContext(options);
    }
}
```

---

## Adım 3 — `feat(infra): EF Core repository implementasyonları`

Mevcut in-memory repolarını silme; yanına EF sürümlerini koy (DI seçecek). Her mutasyon metodu
`SaveChangesAsync` çağırır. Örnekler (kalanları aynı desenle):

```csharp
// Persistence/Repositories/EfProfileRepository.cs
using MedMatch.Application.Abstractions;
using MedMatch.Domain.Profiles;
using Microsoft.EntityFrameworkCore;

namespace MedMatch.Infrastructure.Persistence.Repositories;

public sealed class EfProfileRepository : IProfileRepository
{
    private readonly AppDbContext _db;
    public EfProfileRepository(AppDbContext db) => _db = db;

    public Task<DoctorProfile?> GetByUserIdAsync(Guid userId, CancellationToken ct = default) =>
        _db.Profiles.Include(p => p.Photos).FirstOrDefaultAsync(p => p.UserId == userId, ct);

    public Task<DoctorProfile?> GetByIdAsync(Guid profileId, CancellationToken ct = default) =>
        _db.Profiles.Include(p => p.Photos).FirstOrDefaultAsync(p => p.Id == profileId, ct);

    public async Task<IReadOnlyList<DoctorProfile>> GetAllAsync(CancellationToken ct = default) =>
        await _db.Profiles.Include(p => p.Photos).ToListAsync(ct);

    public async Task AddAsync(DoctorProfile profile, CancellationToken ct = default)
    {
        _db.Profiles.Add(profile);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(DoctorProfile profile, CancellationToken ct = default)
    {
        // profile zaten context tarafından izleniyor (GetByUserId ile yüklendi) -> SaveChanges yeter
        await _db.SaveChangesAsync(ct);
    }
}
```

```csharp
// Persistence/Repositories/EfMatchRepository.cs (GetBetween kanonik sırayı kullanır)
public Task<Match?> GetBetweenAsync(Guid x, Guid y, CancellationToken ct = default)
{
    var (a, b) = x.CompareTo(y) < 0 ? (x, y) : (y, x);
    return _db.Matches.FirstOrDefaultAsync(m => m.UserAId == a && m.UserBId == b, ct);
}
```

Diğerleri (User, Verification, Swipe, Message) birebir aynı kalıp: sorgular `FirstOrDefaultAsync` /
`Where(...).ToListAsync`, mutasyonlar `Add` + `SaveChangesAsync`.

> Not: `DiscoveryService` tüm profilleri çekip bellekte filtreliyor. Şimdilik parite için bırak;
> ölçek büyüyünce filtreleri SQL'e it (Where'leri EF sorgusuna taşı, sonra `ToListAsync`).

---

## Adım 4 — `feat(infra): DbContext'i DI'a bağla, provider seçimi`

`DependencyInjection.cs` içinde depo katmanını bayrağa göre seç:

```csharp
var provider = configuration["Database:Provider"] ?? "Postgres";

if (provider.Equals("InMemory", StringComparison.OrdinalIgnoreCase))
{
    services.AddSingleton<InMemoryStore>();
    services.AddScoped<IUserRepository, UserRepository>();
    services.AddScoped<IProfileRepository, ProfileRepository>();
    // ... diğer in-memory repolar (mevcut hâli)
}
else
{
    services.AddDbContext<AppDbContext>(o =>
        o.UseNpgsql(configuration.GetConnectionString("Postgres")));
    services.AddScoped<IUserRepository, EfUserRepository>();
    services.AddScoped<IProfileRepository, EfProfileRepository>();
    services.AddScoped<IVerificationRepository, EfVerificationRepository>();
    services.AddScoped<ISwipeRepository, EfSwipeRepository>();
    services.AddScoped<IMatchRepository, EfMatchRepository>();
    services.AddScoped<IMessageRepository, EfMessageRepository>();
}

// altyapı servisleri (IClock, IPasswordHasher, ITokenService, IVerificationService, DemoSeeder)
// ve application servisleri iki modda da aynı kalır
```

`Program.cs` başlangıcında Postgres modunda migration uygula, sonra seed et:

```csharp
using (var scope = app.Services.CreateScope())
{
    var sp = scope.ServiceProvider;
    if (!builder.Configuration.GetValue<string>("Database:Provider")!.Equals("InMemory", StringComparison.OrdinalIgnoreCase))
        await sp.GetRequiredService<AppDbContext>().Database.MigrateAsync();
    await sp.GetRequiredService<DemoSeeder>().SeedAsync();
}
```

---

## Adım 5 — `refactor(infra): seeder'ı depo-bağımsız yap`

`DemoSeeder`'ı `InMemoryStore` yerine repository arayüzleri + `IClock` + `IPasswordHasher` alacak
şekilde yeniden yaz. Mantık aynı, sadece `_store.X[...] = e` yerine `await _repo.AddAsync(e)`.
İdempotenslik: başta `await _users.GetByEmailAsync(DemoEmail) is not null` ise çık. `Seed()` → `SeedAsync()`.

Bu tek dosya değişir; imza `SeedAsync` olduğu için Program'daki çağrı da `await ...SeedAsync()` olur.
Böylece hem InMemory hem Postgres modunda aynı seeder çalışır.

---

## Adım 6 — `chore: ilk migration (InitialCreate)`

Postgres ayaktayken:

```bash
dotnet ef migrations add InitialCreate \
  --project src/MedMatch.Infrastructure \
  --startup-project src/MedMatch.Infrastructure
```

> Startup projesi Infrastructure: EF Design paketi `PrivateAssets=all` olduğu için Api'ye geçmez;
> EF CLI bağlantıyı `DesignTimeDbContextFactory`'den alır.

Migration'ı gözden geçir: tüm unique index'ler (Email, UserId, Swipe(SwiperId,TargetId),
Match(UserAId,UserBId)), `CK_Match_Canonical` check constraint ve enum kolonlarının `integer`
olduğunu doğrula. Sonra:

```bash
dotnet ef database update --project src/MedMatch.Infrastructure --startup-project src/MedMatch.Infrastructure
```

`dotnet run` → API açılışta migrate + seed eder. Demo'daki tüm curl akışı artık Postgres'e karşı çalışır.

Demo moduna dönmek istersen `appsettings.json` → `"Database:Provider": "InMemory"`.

---

## Adım 7 (opsiyonel) — `feat(api): JwtBearer ile gerçek JWT doğrulaması`

Elle yazılmış `TokenAuthenticationHandler`'ı framework doğrulamasıyla değiştir.

`src/MedMatch.Api/MedMatch.Api.csproj`:
```xml
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.11" />
```

`JwtTokenService.Issue`'yu `System.IdentityModel.Tokens.Jwt` ile üret (aynı `JwtOptions.Secret`,
`Issuer`). `Program.cs`'te:

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true, ValidIssuer = jwt.Issuer,
            ValidateAudience = false,
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Secret)),
            NameClaimType = ClaimTypes.NameIdentifier
        };
        // SignalR: token'ı query string'den al
        o.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                var token = ctx.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(token) && ctx.HttpContext.Request.Path.StartsWithSegments("/hubs"))
                    ctx.Token = token;
                return Task.CompletedTask;
            }
        };
    });
```

`TokenAuthenticationHandler.cs`'i sil. `UserContext.GetUserId` (NameIdentifier okur) aynı kalır.
Secret'ı üretimde konfigürasyondan/secret store'dan ver, `appsettings`'e gömme; en az 32 karakter.

---

## Adım 8 (opsiyonel) — `feat(infra): Redis SignalR backplane + cache`

Birden fazla API örneği çalıştıracaksan SignalR'ın Redis backplane'i şart (yoksa mesaj yalnızca
aynı sunucuya bağlı istemcilere gider).

`src/MedMatch.Api`:
```xml
<PackageReference Include="Microsoft.AspNetCore.SignalR.StackExchangeRedis" Version="8.0.11" />
```
```csharp
builder.Services.AddSignalR().AddStackExchangeRedis(builder.Configuration.GetConnectionString("Redis")!);
```

Keşif adaylarını / profil kartlarını `IDistributedCache` (Redis) ile cache'lemek ayrı, sonraki bir adım.

---

## Adım 9 (opsiyonel, daha büyük) — MediatR'a geçiş

Düz application servisleri iyi çalışıyor; MediatR'ı yalnızca cross-cutting davranış (validation,
logging, transaction pipeline) merkezileşsin istiyorsan ekle.

- `MedMatch.Application`'a `MediatR` paketi.
- Her servis metodu bir `IRequest<TResponse>` + `IRequestHandler` olur (örn. `SwipeCommand` →
  `SwipeHandler`).
- Controller'lar servis yerine `ISender.Send(command)` çağırır.
- `IPipelineBehavior` ile FluentValidation ve `DbContext` transaction'ı tek yerde toplanır.

Bu, dosya sayısını artıran bir refactor; persistence geçişi oturduktan sonra ayrı bir dalda yap.

---

## Sıra özeti

1. Paketler + docker-compose
2. AppDbContext + configuration'lar + design-time factory
3. EF repository'leri
4. DI'da provider seçimi + startup migrate/seed
5. Seeder'ı depo-bağımsız yap
6. InitialCreate migration + database update
7. (ops.) JwtBearer
8. (ops.) Redis backplane
9. (ops.) MediatR

Her adım kendi commit'i. 1-6 arası Domain/Application'a dokunmaz (5'teki seeder hariç). Takıldığın
adımda çıktıyı bana getir, birlikte çözelim.
