using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Ecommerce.Api.Hubs;

[Authorize] // only authenticated users connect
public class UserEventsHub : Hub
{
    public override Task OnConnectedAsync()
    {
        Console.WriteLine($"[Hub] Connected: UserIdentifier={Context.UserIdentifier}");
        return base.OnConnectedAsync();
    }
}
