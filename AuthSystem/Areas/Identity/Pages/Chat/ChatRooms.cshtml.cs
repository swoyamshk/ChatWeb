using AuthSystem.Areas.Identity.Data;
using AuthSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class ChatRoomsModel : PageModel
{
    private readonly AuthDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<ChatRoomsModel> _logger;
    private readonly IUserActivityService _userActivityService;

    public ChatRoomsModel(AuthDbContext context, UserManager<ApplicationUser> userManager, ILogger<ChatRoomsModel> logger, IUserActivityService userActivityService)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
        _userActivityService = userActivityService;
    }

    [BindProperty]
    public string RoomName { get; set; }

    [BindProperty]
    public bool IsGroupChat { get; set; }

    public IList<ChatRoom> ChatRooms { get; set; }

    public async Task OnGetAsync()
    {
        var userId = _userManager.GetUserId(User);
        if (userId != null)
        {
            await _userActivityService.LogActivity(userId, "Viewed Chat Rooms");
        }

        ChatRooms = await _context.ChatRooms
            .Where(cr => cr.CreatorId == userId || cr.Participants.Any(p => p.UserId == userId))
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            _logger.LogWarning("User not found.");
            return Challenge();
        }

        var chatRoom = new ChatRoom
        {
            Name = RoomName,
            CreatorId = user.Id,
            IsGroupChat = IsGroupChat,
            Participants = new List<ChatParticipant>
            {
                new ChatParticipant
                {
                    UserId = user.Id
                }
            }
        };

        _context.ChatRooms.Add(chatRoom);
        await _context.SaveChangesAsync();

        return RedirectToPage(new { roomId = chatRoom.Id });
    }
}