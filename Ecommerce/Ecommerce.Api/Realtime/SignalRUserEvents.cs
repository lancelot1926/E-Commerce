using Ecommerce.Api.Hubs;
using Ecommerce.Application.Common.Realtime;
using Microsoft.AspNetCore.SignalR;

public class SignalRUserEvents : IUserEvents
{
    private readonly IHubContext<UserEventsHub> _hub;
    public SignalRUserEvents(IHubContext<UserEventsHub> hub) => _hub = hub;

    public Task ForceLogoutAsync(int userId, string reason, CancellationToken ct = default)
        => _hub.Clients.User(userId.ToString())
               .SendAsync("ForceLogout", new { reason }, ct);
}