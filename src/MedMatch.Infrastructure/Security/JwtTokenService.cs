using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using MedMatch.Application.Abstractions;

namespace MedMatch.Infrastructure.Security;

/// <summary>
/// Bağımlılıksız (elle) HS256 JWT üretimi ve doğrulaması. Demo içindir; üretimde
/// Microsoft.AspNetCore.Authentication.JwtBearer + döndürülen anahtar kullanılmalıdır.
/// </summary>
public sealed class JwtTokenService : ITokenService
{
    private readonly JwtOptions _opt;
    private readonly IClock _clock;

    public JwtTokenService(JwtOptions opt, IClock clock)
    {
        _opt = opt; _clock = clock;
    }

    public (string token, DateTimeOffset expiresAt) Issue(Guid userId, string email)
    {
        var now = _clock.Now;
        var exp = now.AddHours(_opt.LifetimeHours);

        var header = new Dictionary<string, object> { ["alg"] = "HS256", ["typ"] = "JWT" };
        var payload = new Dictionary<string, object>
        {
            ["sub"] = userId.ToString(),
            ["email"] = email,
            ["iss"] = _opt.Issuer,
            ["iat"] = now.ToUnixTimeSeconds(),
            ["exp"] = exp.ToUnixTimeSeconds()
        };

        var headerB64 = B64Url(JsonSerializer.SerializeToUtf8Bytes(header));
        var payloadB64 = B64Url(JsonSerializer.SerializeToUtf8Bytes(payload));
        var signingInput = $"{headerB64}.{payloadB64}";
        var sig = B64Url(Sign(signingInput));

        return ($"{signingInput}.{sig}", exp);
    }

    public Guid? Validate(string token)
    {
        if (string.IsNullOrWhiteSpace(token)) return null;
        var parts = token.Split('.');
        if (parts.Length != 3) return null;

        var signingInput = $"{parts[0]}.{parts[1]}";
        var expectedSig = B64Url(Sign(signingInput));
        if (!CryptographicOperations.FixedTimeEquals(
                Encoding.ASCII.GetBytes(expectedSig), Encoding.ASCII.GetBytes(parts[2])))
            return null;

        try
        {
            using var doc = JsonDocument.Parse(FromB64Url(parts[1]));
            var root = doc.RootElement;
            if (root.TryGetProperty("exp", out var expEl) &&
                expEl.GetInt64() < _clock.Now.ToUnixTimeSeconds())
                return null; // süresi dolmuş

            if (root.TryGetProperty("sub", out var subEl) &&
                Guid.TryParse(subEl.GetString(), out var userId))
                return userId;
        }
        catch (JsonException)
        {
            return null;
        }
        return null;
    }

    private byte[] Sign(string input)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_opt.Secret));
        return hmac.ComputeHash(Encoding.ASCII.GetBytes(input));
    }

    private static string B64Url(byte[] bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private static byte[] FromB64Url(string s)
    {
        var b64 = s.Replace('-', '+').Replace('_', '/');
        b64 = (b64.Length % 4) switch { 2 => b64 + "==", 3 => b64 + "=", _ => b64 };
        return Convert.FromBase64String(b64);
    }
}
