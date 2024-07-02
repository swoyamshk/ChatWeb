using AuthSystem.Areas.Identity.Data;
using AuthSystem.Models;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

    public class PrivateChatHub : Hub
    {
        private readonly AuthDbContext _context;

        public PrivateChatHub(AuthDbContext context)
        {
            _context = context;
            Console.WriteLine("ChatHub constructor called.");
        }

        public async Task SendMessage(string userEmail, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", userEmail, message);
            Console.WriteLine("Message sent from user: " + userEmail);
        }

        public async Task JoinChatRoom(int chatRoomId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, chatRoomId.ToString());
        }

        public async Task LeaveChatRoom(int chatRoomId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatRoomId.ToString());
        }
    }
