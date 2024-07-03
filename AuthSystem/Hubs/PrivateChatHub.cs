using AuthSystem.Areas.Identity.Data;
using AuthSystem.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

    public class PrivateChatHub : Hub
    {
        private readonly AuthDbContext _context;
    private readonly ILogger<PrivateChatHub> _logger;
    public PrivateChatHub(AuthDbContext context, ILogger<PrivateChatHub> logger)
        {
            _context = context;
        _logger = logger;
        Console.WriteLine("ChatHub constructor called.");
        }

    /* public async Task JoinChatRoom(int chatRoomId)
     {
         var userId = Context.UserIdentifier;
         var chatRoom = await _context.ChatRooms
             .Include(cr => cr.Participants)
             .FirstOrDefaultAsync(cr => cr.Id == chatRoomId);

         if (chatRoom == null || !chatRoom.Participants.Any(p => p.UserId == userId))
         {
             throw new HubException("You are not authorized to join this chat room.");
         }

         await Groups.AddToGroupAsync(Context.ConnectionId, chatRoomId.ToString());
     }*/

    public async Task JoinChatRoom(string chatRoomId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, chatRoomId);
    }

    public async Task AddUserToChatRoom(int chatRoomId, string userIdToAdd)
    {
        var userId = Context.UserIdentifier;
        var chatRoom = await _context.ChatRooms
            .Include(c => c.Participants)
            .FirstOrDefaultAsync(c => c.Id == chatRoomId);

        if (chatRoom == null)
        {
            throw new HubException("Chat room not found.");
        }

        if (chatRoom.CreatorId != userId)
        {
            throw new HubException("Only the creator can add users to this chat room.");
        }

        if (chatRoom.Participants.Any(p => p.UserId == userIdToAdd))
        {
            throw new HubException("User is already a participant in the chat room.");
        }

        var participant = new ChatParticipant { UserId = userIdToAdd, ChatRoomId = chatRoomId };
        chatRoom.Participants.Add(participant);
        await _context.SaveChangesAsync();

        await Groups.AddToGroupAsync(userIdToAdd, chatRoomId.ToString());
        await Clients.User(userIdToAdd).SendAsync("AddedToChatRoom", chatRoomId);
    }
    /*public async Task SendMessage(string chatRoomId, string message)
    {
        _logger.LogInformation("SendMessage called with chatRoomId: {ChatRoomId}, message: {Message}", chatRoomId, message);
        await Clients.Group(chatRoomId).SendAsync("ReceiveMessage", Context.User.Identity.Name, message);
    }*/
    public async Task SendMessage(int chatRoomId, string message)
      {
          var userId = Context.UserIdentifier;
          var chatRoom = await _context.ChatRooms
              .Include(cr => cr.Participants)
              .FirstOrDefaultAsync(cr => cr.Id == chatRoomId);

          if (chatRoom == null || !chatRoom.Participants.Any(p => p.UserId == userId))
          {
              throw new HubException("You are not authorized to send messages in this chat room.");
          }

          await Clients.Group(chatRoomId.ToString()).SendAsync("ReceiveMessage", userId, message);
      }
 
    public async Task LeaveChatRoom(int chatRoomId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatRoomId.ToString());
        }
    }
