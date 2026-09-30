using MedMatch.Api.Common;
using MedMatch.Api.Hubs;
using MedMatch.Application.Contracts;
using MedMatch.Application.Features.Chat;
using MedMatch.Application.Features.Matching;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace MedMatch.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/matches")]
public sealed class MatchesController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IHubContext<ChatHub> _hub;
    private readonly ILogger<MatchesController> _log;

    public MatchesController(ISender sender, IHubContext<ChatHub> hub, ILogger<MatchesController> log)
    {
        _sender = sender; _hub = hub; _log = log;
    }

    [HttpPost("swipe")]
    public async Task<ActionResult<SwipeResultDto>> Swipe(SwipeRequest req, CancellationToken ct)
    {
        var result = await _sender.Send(new SwipeCommand(User.GetUserId(), req), ct);
        if (result.Matched && result.MatchId is { } matchId)
        {
            // her iki tarafa da "yeni eşleşme" bildir
            await BroadcastAsync(() => _hub.Clients.Users(User.GetUserId().ToString(), req.TargetUserId.ToString())
                .SendAsync("Matched", new { matchId }, ct));
        }
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<MatchDto>>> Mine(CancellationToken ct)
        => Ok(await _sender.Send(new GetMatchesQuery(User.GetUserId()), ct));

    [HttpGet("{matchId:guid}/messages")]
    public async Task<ActionResult<IReadOnlyList<MessageDto>>> Messages(Guid matchId, CancellationToken ct)
        => Ok(await _sender.Send(new GetMessagesQuery(User.GetUserId(), matchId), ct));

    [HttpPost("{matchId:guid}/messages")]
    public async Task<ActionResult<MessageDto>> Send(Guid matchId, SendMessageRequest req, CancellationToken ct)
    {
        var msg = await _sender.Send(new SendMessageCommand(User.GetUserId(), matchId, req), ct);
        // sohbet grubuna gerçek zamanlı yayınla
        await BroadcastAsync(() => _hub.Clients.Group(ChatHub.MatchGroup(matchId)).SendAsync("ReceiveMessage", msg, ct));
        return Ok(msg);
    }

    /// <summary>
    /// Canlı bildirim best-effort: kayıt zaten yapıldı, backplane (Redis) kesintisi isteği düşürmesin
    /// (yoksa istemci tekrar dener ve mesaj çift yazılır). İstemciler veriyi REST'ten de çeker.
    /// </summary>
    private async Task BroadcastAsync(Func<Task> send)
    {
        try { await send(); }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _log.LogWarning(ex, "SignalR yayını başarısız; kayıt tamam, canlı bildirim atlandı.");
        }
    }
}
