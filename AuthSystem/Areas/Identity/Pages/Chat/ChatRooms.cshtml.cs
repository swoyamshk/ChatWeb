using AuthSystem.Areas.Identity.Data;
using AuthSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;


public class ChatRoomsModel : PageModel
{
    private readonly AuthDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserActivityService _userActivityService;

    public ChatRoomsModel(AuthDbContext context, UserManager<ApplicationUser> userManager, IUserActivityService userActivityService)
    {
        _context = context;
        _userManager = userManager;
        _userActivityService = userActivityService;
    }

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
}
