using MedMatch.Api.Common;
using MedMatch.Api.Hubs;
using MedMatch.Application.Contracts;
using MedMatch.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace MedMatch.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/matches")]
public sealed class MatchesController : ControllerBase
{
    private readonly MatchingService _matching;
    private readonly ChatService _chat;
    private readonly IHubContext<ChatHub> _hub;

    public MatchesController(MatchingService matching, ChatService chat, IHubContext<ChatHub> hub)
    {
        _matching = matching; _chat = chat; _hub = hub;
    }

    [HttpPost("swipe")]
    public async Task<ActionResult<SwipeResultDto>> Swipe(SwipeRequest req, CancellationToken ct)
    {
        var result = await _matching.SwipeAsync(User.GetUserId(), req, ct);
        if (result.Matched && result.MatchId is { } matchId)
        {
            // her iki tarafa da "yeni eşleşme" bildir
            await _hub.Clients.Users(User.GetUserId().ToString(), req.TargetUserId.ToString())
                .SendAsync("Matched", new { matchId }, ct);
        }
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<MatchDto>>> Mine(CancellationToken ct)
        => Ok(await _chat.GetMatchesAsync(User.GetUserId(), ct));

    [HttpGet("{matchId:guid}/messages")]
    public async Task<ActionResult<IReadOnlyList<MessageDto>>> Messages(Guid matchId, CancellationToken ct)
        => Ok(await _chat.GetMessagesAsync(User.GetUserId(), matchId, ct));

    [HttpPost("{matchId:guid}/messages")]
    public async Task<ActionResult<MessageDto>> Send(Guid matchId, SendMessageRequest req, CancellationToken ct)
    {
        var msg = await _chat.SendAsync(User.GetUserId(), matchId, req, ct);
        // sohbet grubuna gerçek zamanlı yayınla
        await _hub.Clients.Group(ChatHub.MatchGroup(matchId)).SendAsync("ReceiveMessage", msg, ct);
        return Ok(msg);
    }
}
