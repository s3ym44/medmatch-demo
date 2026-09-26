using MedMatch.Api.Common;
using MedMatch.Application.Services;
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
    private readonly ChatService _chat;
    public ChatHub(ChatService chat) => _chat = chat;

    public static string MatchGroup(Guid matchId) => $"match:{matchId}";

    public async Task JoinMatch(Guid matchId)
    {
        var userId = Context.User!.GetUserId();
        if (!await _chat.IsMemberAsync(userId, matchId, Context.ConnectionAborted))
            throw new HubException("Bu sohbete erişim yetkiniz yok.");

        await Groups.AddToGroupAsync(Context.ConnectionId, MatchGroup(matchId));
    }

    public async Task LeaveMatch(Guid matchId)
        => await Groups.RemoveFromGroupAsync(Context.ConnectionId, MatchGroup(matchId));
}
