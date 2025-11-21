using Microsoft.AspNetCore.SignalR;

namespace PayHelp.Api.Hubs;

public class ChatHub : Hub
{
    public async Task JoinSupport()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "support");
    }

    public async Task LeaveSupport()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "support");
    }
}
