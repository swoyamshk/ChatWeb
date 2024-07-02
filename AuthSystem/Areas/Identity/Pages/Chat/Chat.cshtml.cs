using AuthSystem.Areas.Identity.Data;
using AuthSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


    public class ChatModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AuthDbContext _context;
        private readonly IHubContext<PrivateChatHub> _hubContext;
        private readonly ILogger<ChatModel> _logger;

        public ChatModel(
            UserManager<ApplicationUser> userManager,
            AuthDbContext context,
            IHubContext<PrivateChatHub> hubContext,
            ILogger<ChatModel> logger)
        {
            _userManager = userManager;
            _context = context;
            _hubContext = hubContext;
            _logger = logger;
        }

        public ChatRoom Room { get; set; }
        public IList<ChatMessage> Messages { get; set; }

        public async Task OnGetAsync(int roomId)
        {
            Room = await _context.ChatRooms.FindAsync(roomId);
            Messages = await _context.ChatMessages
                .Where(m => m.ChatRoomId == roomId)
                .Include(m => m.Sender)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync(string Content, int ChatRoomId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _logger.LogWarning("User not found.");
                return Challenge();
            }

            var userEmail = user.Email;
            _logger.LogInformation("User found: {Email}", userEmail);

            var message = new ChatMessage
            {
                Content = Content,
                Timestamp = DateTime.Now,
                SenderId = user.Id,
                ChatRoomId = ChatRoomId
            };

            _context.ChatMessages.Add(message);
            _logger.LogInformation("Message added to context: {Content}", message.Content);

            // Check if the entity is being tracked
            var entry = _context.Entry(message);
            _logger.LogInformation("Entity State: {State}", entry.State);

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Message saved to database.");

                await _hubContext.Clients.Group(ChatRoomId.ToString()).SendAsync("ReceiveMessage", userEmail, Content);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "A database update error occurred while saving the message.");
                return StatusCode(500, "A database update error occurred while saving the message.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while saving the message.");
                return StatusCode(500, "An error occurred while saving the message.");
            }

            return RedirectToPage(new { roomId = ChatRoomId });
        }
    }

