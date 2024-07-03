using AuthSystem.Areas.Identity.Data;
using AuthSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

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
        Messages = new List<ChatMessage>(); // Initialize Messages to an empty list
    }

    public ChatRoom Room { get; set; }
    public IList<ChatMessage> Messages { get; set; }
    [BindProperty]
    public string UserIdToAdd { get; set; }

    public async Task<IActionResult> OnGetAsync(int roomId)
    {
        Room = await _context.ChatRooms
            .Include(cr => cr.Creator)
            .FirstOrDefaultAsync(cr => cr.Id == roomId);

        if (Room == null)
        {
            return NotFound();
        }

        Messages = await _context.ChatMessages
            .Where(m => m.ChatRoomId == roomId)
            .Include(m => m.Sender)
            .ToListAsync();

        return Page();
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

        var chatRoom = await _context.ChatRooms
            .Include(cr => cr.Participants)
            .FirstOrDefaultAsync(cr => cr.Id == ChatRoomId);

        if (chatRoom == null || !chatRoom.Participants.Any(p => p.UserId == user.Id))
        {
            _logger.LogWarning("User not authorized to post in this chat room.");
            return Forbid();
        }

        var message = new ChatMessage
        {
            Content = Content,
            Timestamp = DateTime.Now,
            SenderId = user.Id,
            ChatRoomId = ChatRoomId
        };

        _context.ChatMessages.Add(message);
        _logger.LogInformation("Message added to context: {Content}", message.Content);

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

    public async Task<IActionResult> OnPostAddUserAsync(int ChatRoomId, string UserIdToAdd)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            _logger.LogWarning("User not found.");
            return Challenge();
        }

        var chatRoom = await _context.ChatRooms
            .Include(cr => cr.Creator)
            .Include(cr => cr.Participants)
            .FirstOrDefaultAsync(cr => cr.Id == ChatRoomId);

        if (chatRoom == null)
        {
            _logger.LogWarning("Chat room not found.");
            return NotFound();
        }

        if (chatRoom.CreatorId != user.Id)
        {
            _logger.LogWarning("User is not authorized to add users to this chat room.");
            return Forbid();
        }

        var userToAdd = await _userManager.FindByEmailAsync(UserIdToAdd);
        if (userToAdd == null)
        {
            ModelState.AddModelError("UserIdToAdd", "User not found.");
            Room = chatRoom;
            Messages = await _context.ChatMessages
                .Where(m => m.ChatRoomId == ChatRoomId)
                .Include(m => m.Sender)
                .ToListAsync();
            return Page();
        }

        if (chatRoom.Participants.Any(p => p.UserId == userToAdd.Id))
        {
            ModelState.AddModelError("UserIdToAdd", "User is already a participant.");
            Room = chatRoom;
            Messages = await _context.ChatMessages
                .Where(m => m.ChatRoomId == ChatRoomId)
                .Include(m => m.Sender)
                .ToListAsync();
            return Page();
        }

        try
        {
            chatRoom.Participants.Add(new ChatParticipant
            {
                UserId = userToAdd.Id,
                ChatRoomId = ChatRoomId
            });

            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "A database update error occurred while adding the user.");
            ModelState.AddModelError("", "A database update error occurred.");
            Room = chatRoom;
            Messages = await _context.ChatMessages
                .Where(m => m.ChatRoomId == ChatRoomId)
                .Include(m => m.Sender)
                .ToListAsync();
            return Page();
        }

        Room = chatRoom;
        Messages = await _context.ChatMessages
            .Where(m => m.ChatRoomId == ChatRoomId)
            .Include(m => m.Sender)
            .ToListAsync();
        return RedirectToPage(new { roomId = ChatRoomId });
    }


}
