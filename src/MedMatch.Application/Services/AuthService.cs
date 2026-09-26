using MedMatch.Application.Abstractions;
using MedMatch.Application.Common;
using MedMatch.Application.Contracts;
using MedMatch.Domain.Users;

namespace MedMatch.Application.Services;

public sealed class AuthService
{
    private readonly IUserRepository _users;
    private readonly IProfileRepository _profiles;
    private readonly IPasswordHasher _hasher;
    private readonly ITokenService _tokens;
    private readonly IClock _clock;

    public AuthService(IUserRepository users, IProfileRepository profiles, IPasswordHasher hasher, ITokenService tokens, IClock clock)
    {
        _users = users; _profiles = profiles; _hasher = hasher; _tokens = tokens; _clock = clock;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest req, CancellationToken ct = default)
    {
        var email = (req.Email ?? "").Trim().ToLowerInvariant();
        if (!email.Contains('@')) throw AppException.Validation("Geçerli bir e-posta girin.");
        if (string.IsNullOrWhiteSpace(req.Password) || req.Password.Length < 6)
            throw AppException.Validation("Parola en az 6 karakter olmalı.");
        if (await _users.EmailExistsAsync(email, ct))
            throw AppException.Conflict("Bu e-posta zaten kayıtlı.");

        var user = User.Create(email, _hasher.Hash(req.Password), _clock.Now);
        await _users.AddAsync(user, ct);

        var (token, exp) = _tokens.Issue(user.Id, user.Email);
        return new AuthResponse(token, exp, user.Id, HasProfile: false);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest req, CancellationToken ct = default)
    {
        var email = (req.Email ?? "").Trim().ToLowerInvariant();
        var user = await _users.GetByEmailAsync(email, ct);
        if (user is null || !_hasher.Verify(req.Password ?? "", user.PasswordHash))
            throw AppException.Unauthorized("E-posta veya parola hatalı.");

        user.Touch(_clock.Now);
        await _users.UpdateAsync(user, ct);

        var profile = await _profiles.GetByUserIdAsync(user.Id, ct);
        var (token, exp) = _tokens.Issue(user.Id, user.Email);
        return new AuthResponse(token, exp, user.Id, HasProfile: profile is not null);
    }
}
