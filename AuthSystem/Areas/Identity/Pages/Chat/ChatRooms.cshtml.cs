using AuthSystem.Areas.Identity.Data;
using AuthSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;


public class ChatRoomsModel : PageModel
{
    private readonly AuthDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ChatRoomsModel(AuthDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public IList<ChatRoom> ChatRooms { get; set; }

    public async Task OnGetAsync()
    {
        var userId = _userManager.GetUserId(User);

        ChatRooms = await _context.ChatRooms
            .Where(cr => cr.CreatorId == userId || cr.Participants.Any(p => p.UserId == userId))
            .ToListAsync();
    }
}
