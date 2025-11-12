using Microsoft.AspNetCore.SignalR;

namespace PayHelp.WebApp.Mvc.Hubs;

public class ChatHub : Hub
{
    public Task JoinTicket(string ticketId)
        => Groups.AddToGroupAsync(Context.ConnectionId, ticketId);

    public Task LeaveTicket(string ticketId)
        => Groups.RemoveFromGroupAsync(Context.ConnectionId, ticketId);

    public Task JoinSupport()
        => Groups.AddToGroupAsync(Context.ConnectionId, "support");
}
