using AuthSystem.Areas.Identity.Data;
using AuthSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthSystem.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ChatRoomController : ControllerBase
    {
        private readonly AuthDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserActivityService _userActivityService;

        public ChatRoomController(AuthDbContext context, UserManager<ApplicationUser> userManager, IUserActivityService userActivityService)
        {
            _context = context;
            _userManager = userManager;
            _userActivityService = userActivityService;
        }

        [HttpPost("CreateRoom")]
        public async Task<IActionResult> CreateRoom(string name, bool isGroupChat)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            try
            {
                await _userActivityService.LogActivity(user.Id, "CreateRoom");
            }
            catch (Exception ex)
            {
                // Log or handle exception
                return StatusCode(500, "Failed to log user activity");
            }

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
            if (user == null)
            {
                return Unauthorized();
            }

            try
            {
                await _userActivityService.LogActivity(user.Id, "GetRooms");
            }
            catch (Exception ex)
            {
                // Log or handle exception
                return StatusCode(500, "Failed to log user activity");
            }

            var rooms = await _context.ChatRooms
                .Where(cr => cr.Participants.Any(p => p.UserId == user.Id))
                .ToListAsync();

            return Ok(rooms);
        }

        [HttpPost("LogPageVisit")]
        public async Task<IActionResult> LogPageVisit([FromBody] LogPageVisitModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            try
            {
                await _userActivityService.LogActivity(user.Id, model.PageName);
                return Ok();
            }
            catch (Exception ex)
            {
                // Log or handle exception
                return StatusCode(500, "Failed to log user activity");
            }
        }
    }

    public class LogPageVisitModel
    {
        public string PageName { get; set; }
    }
}
