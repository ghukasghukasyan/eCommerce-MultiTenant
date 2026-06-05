using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace eCommerce.Api.Hubs
{
    [Authorize(Roles = "Admin")]
    public class OrderHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "admins");
            await base.OnConnectedAsync();
        }
    }
}
