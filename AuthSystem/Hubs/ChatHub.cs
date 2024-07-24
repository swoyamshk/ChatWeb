using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace AuthSystem.Hubs
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(string user, string message, string imageUrl)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message, imageUrl);
        }
    }
}
