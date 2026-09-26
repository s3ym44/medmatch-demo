using System.Security.Claims;
using System.Text.Encodings.Web;
using MedMatch.Application.Abstractions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MedMatch.Api.Auth;

/// <summary>
/// Bearer token doğrulaması. JwtBearer paketi yerine ITokenService (elle HS256) kullanır.
/// SignalR websocket'leri için token'ı query string'den (access_token) de kabul eder.
/// </summary>
public sealed class TokenAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "Bearer";
    private readonly ITokenService _tokens;

    public TokenAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ITokenService tokens)
        : base(options, logger, encoder)
    {
        _tokens = tokens;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var token = ExtractToken();
        if (string.IsNullOrEmpty(token))
            return Task.FromResult(AuthenticateResult.NoResult());

        var userId = _tokens.Validate(token);
        if (userId is null)
            return Task.FromResult(AuthenticateResult.Fail("Geçersiz veya süresi dolmuş token."));

        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()) };
        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    private string? ExtractToken()
    {
        var header = Request.Headers.Authorization.ToString();
        if (header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return header["Bearer ".Length..].Trim();

        // websocket handshake: ?access_token=...
        var qs = Request.Query["access_token"].ToString();
        return string.IsNullOrEmpty(qs) ? null : qs;
    }
}
