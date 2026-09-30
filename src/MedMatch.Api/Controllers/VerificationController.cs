using MedMatch.Api.Common;
using MedMatch.Application.Contracts;
using MedMatch.Application.Features.Verification;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedMatch.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/verification")]
public sealed class VerificationController : ControllerBase
{
    private readonly ISender _sender;
    public VerificationController(ISender sender) => _sender = sender;

    /// <summary>Belge/kanıt gönderir. Demo'da mock sağlayıcı anında sonuç döner.</summary>
    [HttpPost("submit")]
    public async Task<ActionResult<VerificationDto>> Submit(SubmitVerificationRequest req, CancellationToken ct)
        => Ok(await _sender.Send(new SubmitVerificationCommand(User.GetUserId(), req), ct));

    [HttpGet("status")]
    public async Task<ActionResult<VerificationDto>> Status(CancellationToken ct)
    {
        var dto = await _sender.Send(new GetVerificationStatusQuery(User.GetUserId()), ct);
        return dto is null ? Ok(new { status = "Unverified" }) : Ok(dto);
    }
}
