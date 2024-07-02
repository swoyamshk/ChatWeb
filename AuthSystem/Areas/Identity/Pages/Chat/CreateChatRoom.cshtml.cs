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

        public CreateChatRoomModel(AuthDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public string RoomName { get; set; }

        [BindProperty]
        public bool IsGroupChat { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);

                var chatRoom = new ChatRoom
                {
                    Name = RoomName,
                    IsGroupChat = IsGroupChat,
                    Participants = new List<ChatParticipant>
                    {
                        new ChatParticipant { UserId = user.Id }
                    }
                };

                _context.ChatRooms.Add(chatRoom);
                await _context.SaveChangesAsync();

                return RedirectToPage("/Chat/ChatRooms");
            }

            // If model state is not valid, return the page with validation errors
            return Page();
        }
    }

