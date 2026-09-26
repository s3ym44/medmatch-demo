using MedMatch.Api.Common;
using MedMatch.Application.Contracts;
using MedMatch.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedMatch.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/discovery")]
public sealed class DiscoveryController : ControllerBase
{
    private readonly DiscoveryService _discovery;
    public DiscoveryController(DiscoveryService discovery) => _discovery = discovery;

    [HttpGet("candidates")]
    public async Task<ActionResult<IReadOnlyList<CandidateDto>>> Candidates([FromQuery] int take = 20, CancellationToken ct = default)
        => Ok(await _discovery.GetCandidatesAsync(User.GetUserId(), take, ct));
}
