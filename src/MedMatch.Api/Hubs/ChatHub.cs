using MedMatch.Api.Common;
using MedMatch.Application.Features.Chat;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace MedMatch.Api.Hubs;

/// <summary>
/// Gerçek zamanlı sohbet. İstemci bir eşleşme sohbetine katılınca ilgili gruba eklenir;
/// mesajlar REST üzerinden kalıcı hale gelir ve bu hub'ın grubuna yayınlanır.
/// </summary>
[Authorize]
public sealed class ChatHub : Hub
{
    private readonly ISender _sender;
    public ChatHub(ISender sender) => _sender = sender;

    public static string MatchGroup(Guid matchId) => $"match:{matchId}";

    public async Task JoinMatch(Guid matchId)
    {
        var userId = Context.User!.GetUserId();
        if (!await _sender.Send(new IsMatchMemberQuery(userId, matchId), Context.ConnectionAborted))
            throw new HubException("Bu sohbete erişim yetkiniz yok.");

        await Groups.AddToGroupAsync(Context.ConnectionId, MatchGroup(matchId));
    }

    public async Task LeaveMatch(Guid matchId)
        => await Groups.RemoveFromGroupAsync(Context.ConnectionId, MatchGroup(matchId));
}
