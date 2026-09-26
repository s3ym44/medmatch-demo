using MedMatch.Api.Common;
using MedMatch.Application.Contracts;
using MedMatch.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedMatch.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/profiles")]
public sealed class ProfilesController : ControllerBase
{
    private readonly ProfileService _profiles;
    public ProfilesController(ProfileService profiles) => _profiles = profiles;

    [HttpGet("me")]
    public async Task<ActionResult<ProfileDto>> Me(CancellationToken ct)
    {
        var dto = await _profiles.GetMineAsync(User.GetUserId(), ct);
        return dto is null ? NotFound(new { error = "Profil yok." }) : Ok(dto);
    }

    [HttpPost]
    public async Task<ActionResult<ProfileDto>> Create(CreateProfileRequest req, CancellationToken ct)
        => Ok(await _profiles.CreateAsync(User.GetUserId(), req, ct));

    [HttpPut("me/preferences")]
    public async Task<ActionResult<ProfileDto>> UpdatePreferences(UpdatePreferencesRequest req, CancellationToken ct)
        => Ok(await _profiles.UpdatePreferencesAsync(User.GetUserId(), req, ct));

    [HttpPost("me/photos")]
    public async Task<ActionResult<ProfileDto>> AddPhoto(AddPhotoRequest req, CancellationToken ct)
        => Ok(await _profiles.AddPhotoAsync(User.GetUserId(), req, ct));
}
