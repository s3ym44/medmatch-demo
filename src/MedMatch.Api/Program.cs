using System.Text.Json.Serialization;
using MedMatch.Api.Auth;
using MedMatch.Api.Common;
using MedMatch.Api.Hubs;
using MedMatch.Infrastructure;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddSignalR();
builder.Services.AddInfrastructure(builder.Configuration);

// Bearer token doğrulaması (JwtBearer paketi yerine özel handler)
builder.Services
    .AddAuthentication(TokenAuthenticationHandler.SchemeName)
    .AddScheme<AuthenticationSchemeOptions, TokenAuthenticationHandler>(TokenAuthenticationHandler.SchemeName, null);
builder.Services.AddAuthorization();

const string CorsPolicy = "spa";
builder.Services.AddCors(o => o.AddPolicy(CorsPolicy, p => p
    .SetIsOriginAllowed(origin =>
    {
        // dev: localhost/127.0.0.1 herhangi bir port (Vite 5173 vb.)
        if (Uri.TryCreate(origin, UriKind.Absolute, out var u))
            return u.Host is "localhost" or "127.0.0.1";
        return false;
    })
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()));

var app = builder.Build();

// veritabanını hazırla (Postgres: migrate) ve demo verisini yükle
await app.Services.InitializeDatabaseAsync(builder.Configuration);

app.UseMiddleware<ExceptionMiddleware>();
app.UseCors(CorsPolicy);
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapControllers();
app.MapHub<ChatHub>("/hubs/chat");

app.Run();

public partial class Program { }
