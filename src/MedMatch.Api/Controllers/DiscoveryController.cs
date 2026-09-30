using MedMatch.Api.Common;
using MedMatch.Application.Contracts;
using MedMatch.Application.Features.Discovery;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedMatch.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/discovery")]
public sealed class DiscoveryController : ControllerBase
{
    private readonly ISender _sender;
    public DiscoveryController(ISender sender) => _sender = sender;

    [HttpGet("candidates")]
    public async Task<ActionResult<IReadOnlyList<CandidateDto>>> Candidates([FromQuery] int take = 20, CancellationToken ct = default)
        => Ok(await _sender.Send(new GetCandidatesQuery(User.GetUserId(), take), ct));
}
