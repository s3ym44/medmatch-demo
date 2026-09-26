using MedMatch.Api.Common;
using MedMatch.Application.Contracts;
using MedMatch.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedMatch.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/verification")]
public sealed class VerificationController : ControllerBase
{
    private readonly VerificationAppService _verification;
    public VerificationController(VerificationAppService verification) => _verification = verification;

    /// <summary>Belge/kanıt gönderir. Demo'da mock sağlayıcı anında sonuç döner.</summary>
    [HttpPost("submit")]
    public async Task<ActionResult<VerificationDto>> Submit(SubmitVerificationRequest req, CancellationToken ct)
        => Ok(await _verification.SubmitAsync(User.GetUserId(), req, ct));

    [HttpGet("status")]
    public async Task<ActionResult<VerificationDto>> Status(CancellationToken ct)
    {
        var dto = await _verification.GetStatusAsync(User.GetUserId(), ct);
        return dto is null ? Ok(new { status = "Unverified" }) : Ok(dto);
    }
}
