using System.Text.Json.Serialization;
using MedMatch.Api.Common;
using MedMatch.Api.Hubs;
using MedMatch.Application;
using MedMatch.Infrastructure;
using MedMatch.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// SignalR. Birden fazla API örneğinde mesajlar diğer sunuculara bağlı istemcilere de ulaşsın diye
// Redis backplane; ConnectionStrings:Redis boşsa tek sunuculu (süreç içi) çalışır.
var signalR = builder.Services.AddSignalR();
var redis = builder.Configuration.GetConnectionString("Redis");
if (!string.IsNullOrWhiteSpace(redis))
    signalR.AddStackExchangeRedis(redis, o =>
    {
        o.Configuration.ChannelPrefix = StackExchange.Redis.RedisChannel.Literal("medmatch");
        o.Configuration.AbortOnConnectFail = false; // Redis geri gelince yeniden başlatmadan toparlan
    });

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Bearer token doğrulaması (JwtBearer). "sub" claim'i NameIdentifier'a eşlenir (MapInboundClaims):
// UserContext ve SignalR'ın kullanıcı yönlendirmesi (Clients.User) bunu kullanır.
var jwt = builder.Configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();
var tokenValidation = jwt.ValidationParameters(); // lambda dışında: secret eksik/kısaysa açılışta düşer
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = tokenValidation;
        // SignalR websocket handshake'i header gönderemez: token'ı ?access_token= ile al
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
