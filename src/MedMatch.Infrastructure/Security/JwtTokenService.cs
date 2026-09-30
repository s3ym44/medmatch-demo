using System.Security.Claims;
using MedMatch.Application.Abstractions;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace MedMatch.Infrastructure.Security;

/// <summary>
/// HS256 JWT üretimi (Microsoft.IdentityModel). HTTP/SignalR istekleri JwtBearer ile doğrulanır;
/// <see cref="Validate"/> aynı parametrelerle çalışır ve arayüz uyumu için korunur.
/// </summary>
public sealed class JwtTokenService : ITokenService
{
    private readonly JwtOptions _opt;
    private readonly IClock _clock;
    private readonly JsonWebTokenHandler _handler = new();

    public JwtTokenService(JwtOptions opt, IClock clock)
    {
        _opt = opt; _clock = clock;
    }

    public (string token, DateTimeOffset expiresAt) Issue(Guid userId, string email)
    {
        var now = _clock.Now;
        var exp = now.AddHours(_opt.LifetimeHours);

        var token = _handler.CreateToken(new SecurityTokenDescriptor
        {
            Issuer = _opt.Issuer,
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, email)
            }),
            IssuedAt = now.UtcDateTime,
            NotBefore = now.UtcDateTime,
            Expires = exp.UtcDateTime,
            SigningCredentials = new SigningCredentials(_opt.SigningKey(), SecurityAlgorithms.HmacSha256)
        });

        return (token, exp);
    }

    public Guid? Validate(string token)
    {
        if (string.IsNullOrWhiteSpace(token)) return null;

        var result = _handler.ValidateTokenAsync(token, _opt.ValidationParameters()).GetAwaiter().GetResult();
        if (!result.IsValid) return null;

        return result.Claims.TryGetValue(JwtRegisteredClaimNames.Sub, out var sub) &&
               Guid.TryParse(sub?.ToString(), out var userId)
            ? userId
            : null;
    }
}
