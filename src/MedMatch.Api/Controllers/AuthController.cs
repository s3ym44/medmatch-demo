using MedMatch.Application.Contracts;
using MedMatch.Application.Features.Auth;
using MedMatch.Infrastructure.Seed;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedMatch.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly ISender _sender;
    public AuthController(ISender sender) => _sender = sender;

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest req, CancellationToken ct)
        => Ok(await _sender.Send(new RegisterCommand(req), ct));

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest req, CancellationToken ct)
        => Ok(await _sender.Send(new LoginCommand(req), ct));

    /// <summary>Demo hesabının hazır kimlik bilgileri (frontend "tek tıkla giriş" için).</summary>
    [AllowAnonymous]
    [HttpGet("demo-credentials")]
    public ActionResult<object> DemoCredentials()
        => Ok(new { email = DemoSeeder.DemoEmail, password = DemoSeeder.DemoPassword });
}
