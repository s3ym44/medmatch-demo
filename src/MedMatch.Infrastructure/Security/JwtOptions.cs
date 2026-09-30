using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace MedMatch.Infrastructure.Security;

public sealed class JwtOptions
{
    /// <summary>HS256 anahtarı. Koda/appsettings.json'a gömülmez: Development'ta appsettings.Development.json, üretimde Jwt__Secret.</summary>
    public string Secret { get; set; } = "";
    public string Issuer { get; set; } = "medmatch";
    public int LifetimeHours { get; set; } = 24;

    public SymmetricSecurityKey SigningKey()
    {
        var bytes = Encoding.UTF8.GetBytes(Secret);
        if (bytes.Length < 32)
            throw new InvalidOperationException("Jwt:Secret en az 32 byte olmalı (HS256). Üretimde Jwt__Secret ortam değişkeniyle verin.");
        return new SymmetricSecurityKey(bytes);
    }

    /// <summary>Token üretimi ve doğrulaması (JwtBearer + ITokenService.Validate) aynı kuralları kullanır.</summary>
    public TokenValidationParameters ValidationParameters() => new()
    {
        ValidateIssuer = true,
        ValidIssuer = Issuer,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = SigningKey(),
        ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 }
    };
}
