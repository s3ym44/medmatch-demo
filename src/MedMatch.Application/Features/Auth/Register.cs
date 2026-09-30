using FluentValidation;
using MedMatch.Application.Abstractions;
using MedMatch.Application.Common;
using MedMatch.Application.Contracts;
using MedMatch.Domain.Users;
using MediatR;

namespace MedMatch.Application.Features.Auth;

public sealed record RegisterCommand(RegisterRequest Request) : ICommand<AuthResponse>;

internal sealed class RegisterValidator : AbstractValidator<RegisterCommand>
{
    public RegisterValidator()
    {
        RuleFor(x => x.Request.Email)
            .Must(e => (e ?? "").Trim().Contains('@')).WithMessage("Geçerli bir e-posta girin.");
        RuleFor(x => x.Request.Password)
            .Must(p => !string.IsNullOrWhiteSpace(p) && p.Length >= 6).WithMessage("Parola en az 6 karakter olmalı.");
    }
}

internal sealed class RegisterHandler : IRequestHandler<RegisterCommand, AuthResponse>
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _hasher;
    private readonly ITokenService _tokens;
    private readonly IClock _clock;

    public RegisterHandler(IUserRepository users, IPasswordHasher hasher, ITokenService tokens, IClock clock)
    {
        _users = users; _hasher = hasher; _tokens = tokens; _clock = clock;
    }

    public async Task<AuthResponse> Handle(RegisterCommand cmd, CancellationToken ct)
    {
        var email = cmd.Request.Email.Trim().ToLowerInvariant();
        if (await _users.EmailExistsAsync(email, ct))
            throw AppException.Conflict("Bu e-posta zaten kayıtlı.");

        var user = User.Create(email, _hasher.Hash(cmd.Request.Password), _clock.Now);
        await _users.AddAsync(user, ct);

        var (token, exp) = _tokens.Issue(user.Id, user.Email);
        return new AuthResponse(token, exp, user.Id, HasProfile: false);
    }
}
