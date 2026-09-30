using MedMatch.Api.Common;
using MedMatch.Application.Contracts;
using MedMatch.Application.Features.Profiles;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedMatch.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/profiles")]
public sealed class ProfilesController : ControllerBase
{
    private readonly ISender _sender;
    public ProfilesController(ISender sender) => _sender = sender;

    [HttpGet("me")]
    public async Task<ActionResult<ProfileDto>> Me(CancellationToken ct)
    {
        var dto = await _sender.Send(new GetMyProfileQuery(User.GetUserId()), ct);
        return dto is null ? NotFound(new { error = "Profil yok." }) : Ok(dto);
    }

    [HttpPost]
    public async Task<ActionResult<ProfileDto>> Create(CreateProfileRequest req, CancellationToken ct)
        => Ok(await _sender.Send(new CreateProfileCommand(User.GetUserId(), req), ct));

    [HttpPut("me/preferences")]
    public async Task<ActionResult<ProfileDto>> UpdatePreferences(UpdatePreferencesRequest req, CancellationToken ct)
        => Ok(await _sender.Send(new UpdatePreferencesCommand(User.GetUserId(), req), ct));

    [HttpPost("me/photos")]
    public async Task<ActionResult<ProfileDto>> AddPhoto(AddPhotoRequest req, CancellationToken ct)
        => Ok(await _sender.Send(new AddPhotoCommand(User.GetUserId(), req), ct));
}
