using AuthSystem.Areas.Identity.Data;
using AuthSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

public class ChatModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AuthDbContext _context;
    private readonly IHubContext<PrivateChatHub> _hubContext;
    private readonly ILogger<ChatModel> _logger;
    private readonly IUserActivityService _userActivityService;
    private readonly IWebHostEnvironment _environment;

    public ChatModel(
        UserManager<ApplicationUser> userManager,
        AuthDbContext context,
        IHubContext<PrivateChatHub> hubContext,
        ILogger<ChatModel> logger,
        IUserActivityService userActivityService,
        IWebHostEnvironment environment)
    {
        _userManager = userManager;
        _context = context;
        _hubContext = hubContext;
        _logger = logger;
        _userActivityService = userActivityService;
        _environment = environment;
        Messages = new List<ChatMessage>();
        UsersInChat = new List<ApplicationUser>();
    }

    public ChatRoom Room { get; set; }
    public IList<ChatMessage> Messages { get; set; }
    public IList<ApplicationUser> UsersInChat { get; set; }
    public IList<ApplicationUser> Participants { get; set; }

    [BindProperty]
    public string UserIdToAdd { get; set; }

    [BindProperty]
    public string UserIdToRemove { get; set; }

    public async Task<IActionResult> OnGetAsync(int roomId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Challenge();
        }

        await _userActivityService.LogActivity(userId, "Private Chat Room");

        Room = await _context.ChatRooms
            .Include(cr => cr.Creator)
            .Include(cr => cr.Participants)
            .ThenInclude(p => p.User)
            .FirstOrDefaultAsync(cr => cr.Id == roomId);

        if (Room == null)
        {
            return NotFound();
        }

        if (!Room.Participants.Any(p => p.UserId == userId))
        {
            return Forbid();
        }

        Messages = await _context.ChatMessages
            .Where(m => m.ChatRoomId == roomId)
            .Include(m => m.Sender)
            .ToListAsync();

        UsersInChat = Room.Participants.Select(p => p.User).ToList();
        Participants = Room.Participants.Select(p => p.User).ToList();

        foreach (var message in Messages)
        {
            if (message.Content == null)
            {
                message.Content = "";
            }
            if (message.ImageUrl == null)
            {
                message.ImageUrl = "";
            }
        }

        return Page();
    }



    public async Task<IActionResult> OnPostAsync(string Content, int ChatRoomId, IFormFile Image)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            _logger.LogWarning("User not found.");
            return Challenge();
        }

        var chatRoom = await _context.ChatRooms
            .Include(cr => cr.Participants)
            .FirstOrDefaultAsync(cr => cr.Id == ChatRoomId);

        if (chatRoom == null || !chatRoom.Participants.Any(p => p.UserId == user.Id))
        {
            _logger.LogWarning("User not authorized to post in this chat room.");
            return Forbid();
        }

        string imageUrl = null;
        if (Image != null)
        {
            var uploads = Path.Combine(_environment.WebRootPath, "uploads");
            if (!Directory.Exists(uploads))
            {
                Directory.CreateDirectory(uploads);
            }

            var filePath = Path.Combine(uploads, Image.FileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await Image.CopyToAsync(fileStream);
            }
            imageUrl = $"/uploads/{Image.FileName}";
        }

        if (string.IsNullOrEmpty(Content) && string.IsNullOrEmpty(imageUrl))
        {
            ModelState.AddModelError(string.Empty, "Message or image must be provided.");
            return Page();
        }

        var message = new ChatMessage
        {
            Content = Content,
            Timestamp = DateTime.Now,
            SenderId = user.Id,
            ChatRoomId = ChatRoomId,
            ImageUrl = imageUrl
        };

        _context.ChatMessages.Add(message);

        try
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation("Message saved to database.");
            await _hubContext.Clients.Group(ChatRoomId.ToString()).SendAsync("ReceiveMessage", user.Email, Content, imageUrl);
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
            .ThenInclude(p => p.User)
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
            Participants = chatRoom.Participants.Select(p => p.User).ToList();
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
            Participants = chatRoom.Participants.Select(p => p.User).ToList();
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
            Participants = chatRoom.Participants.Select(p => p.User).ToList();
            return Page();
        }

        Room = chatRoom;
        Messages = await _context.ChatMessages
            .Where(m => m.ChatRoomId == ChatRoomId)
            .Include(m => m.Sender)
            .ToListAsync();
        Participants = chatRoom.Participants.Select(p => p.User).ToList();
        return RedirectToPage(new { roomId = ChatRoomId });
    }

    public async Task<IActionResult> OnPostRemoveUserAsync(int ChatRoomId, string UserIdToRemove)
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
            .ThenInclude(p => p.User)
            .FirstOrDefaultAsync(cr => cr.Id == ChatRoomId);

        if (chatRoom == null)
        {
            _logger.LogWarning("Chat room not found.");
            return NotFound();
        }

        if (chatRoom.CreatorId != user.Id)
        {
            _logger.LogWarning("User is not authorized to remove users from this chat room.");
            return Forbid();
        }

        var participantToRemove = chatRoom.Participants.FirstOrDefault(p => p.UserId == UserIdToRemove);
        if (participantToRemove == null)
        {
            ModelState.AddModelError("UserIdToRemove", "Participant not found.");
            Room = chatRoom;
            Messages = await _context.ChatMessages
                .Where(m => m.ChatRoomId == ChatRoomId)
                .Include(m => m.Sender)
                .ToListAsync();
            Participants = chatRoom.Participants.Select(p => p.User).ToList();
            return Page();
        }

        try
        {
            chatRoom.Participants.Remove(participantToRemove);

            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "A database update error occurred while removing the user.");
            ModelState.AddModelError("", "A database update error occurred.");
            Room = chatRoom;
            Messages = await _context.ChatMessages
                .Where(m => m.ChatRoomId == ChatRoomId)
                .Include(m => m.Sender)
                .ToListAsync();
            Participants = chatRoom.Participants.Select(p => p.User).ToList();
            return Page();
        }

        Room = chatRoom;
        Messages = await _context.ChatMessages
            .Where(m => m.ChatRoomId == ChatRoomId)
            .Include(m => m.Sender)
            .ToListAsync();
        Participants = chatRoom.Participants.Select(p => p.User).ToList();
        return RedirectToPage(new { roomId = ChatRoomId });
    }
}
