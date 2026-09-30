using MedMatch.Application.Contracts;
using MedMatch.Application.Features.Profiles;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedMatch.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/prompts")]
public sealed class PromptsController : ControllerBase
{
    private readonly ISender _sender;
    public PromptsController(ISender sender) => _sender = sender;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PromptCatalogItemDto>>> Catalog(CancellationToken ct)
        => Ok(await _sender.Send(new GetPromptCatalogQuery(), ct));
}
