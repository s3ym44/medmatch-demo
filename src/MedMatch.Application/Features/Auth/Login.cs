using MedMatch.Application.Abstractions;
using MedMatch.Application.Common;
using MedMatch.Application.Contracts;
using MediatR;

namespace MedMatch.Application.Features.Auth;

/// <summary>Son aktiflik zamanını yazdığı için command.</summary>
public sealed record LoginCommand(LoginRequest Request) : ICommand<AuthResponse>;

internal sealed class LoginHandler : IRequestHandler<LoginCommand, AuthResponse>
{
    private readonly IUserRepository _users;
    private readonly IProfileRepository _profiles;
    private readonly IPasswordHasher _hasher;
    private readonly ITokenService _tokens;
    private readonly IClock _clock;

    public LoginHandler(IUserRepository users, IProfileRepository profiles, IPasswordHasher hasher, ITokenService tokens, IClock clock)
    {
        _users = users; _profiles = profiles; _hasher = hasher; _tokens = tokens; _clock = clock;
    }

    public async Task<AuthResponse> Handle(LoginCommand cmd, CancellationToken ct)
    {
        var email = (cmd.Request.Email ?? "").Trim().ToLowerInvariant();
        var user = await _users.GetByEmailAsync(email, ct);
        if (user is null || !_hasher.Verify(cmd.Request.Password ?? "", user.PasswordHash))
            throw AppException.Unauthorized("E-posta veya parola hatalı.");

        user.Touch(_clock.Now);
        await _users.UpdateAsync(user, ct);

        var profile = await _profiles.GetByUserIdAsync(user.Id, ct);
        var (token, exp) = _tokens.Issue(user.Id, user.Email);
        return new AuthResponse(token, exp, user.Id, HasProfile: profile is not null);
    }
}
