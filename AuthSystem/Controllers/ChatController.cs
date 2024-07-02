using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AuthSystem.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthSystem.Areas.Identity.Data;

namespace AuthSystem.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ChatRoomController : ControllerBase
    {
        private readonly AuthDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ChatRoomController(AuthDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost("CreateRoom")]
        public async Task<IActionResult> CreateRoom(string name, bool isGroupChat)
        {
            var user = await _userManager.GetUserAsync(User);

            var chatRoom = new ChatRoom
            {
                Name = name,
                IsGroupChat = isGroupChat,
                Participants = new List<ChatParticipant>
                {
                    new ChatParticipant { UserId = user.Id }
                }
            };

            _context.ChatRooms.Add(chatRoom);
            await _context.SaveChangesAsync();

            return Ok(chatRoom.Id);
        }

        [HttpGet("GetRooms")]
        public async Task<IActionResult> GetRooms()
        {
            var user = await _userManager.GetUserAsync(User);
            var rooms = await _context.ChatRooms
                .Where(cr => cr.Participants.Any(p => p.UserId == user.Id))
                .ToListAsync();

            return Ok(rooms);
        }
    }
}
