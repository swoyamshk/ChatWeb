using AuthSystem.Areas.Identity.Data;
using AuthSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Threading.Tasks;

    public class CreateChatRoomModel : PageModel
    {
        private readonly AuthDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<CreateChatRoomModel> _logger;

    public CreateChatRoomModel(AuthDbContext context, UserManager<ApplicationUser> userManager, ILogger<CreateChatRoomModel> logger)
        {
            _context = context;
            _userManager = userManager;
        _logger = logger;
    }

        [BindProperty]
        public string RoomName { get; set; }

        [BindProperty]
        public bool IsGroupChat { get; set; }

    public async Task<IActionResult> OnPostAsync()
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
            IsGroupChat = true,
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

        return RedirectToPage("Chat", new { roomId = chatRoom.Id });
    }
}

