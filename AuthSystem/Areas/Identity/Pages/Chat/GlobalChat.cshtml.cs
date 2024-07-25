using AuthSystem.Areas.Identity.Data;
using AuthSystem.Hubs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using AuthSystem.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;

public class GlobalChatModel : PageModel
{
    private readonly ILogger<GlobalChatModel> _logger;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _environment;
    private readonly AuthDbContext _context;
    private readonly IHubContext<ChatHub> _hubContext;

    public List<Message> Messages { get; set; }

    public GlobalChatModel(
        ILogger<GlobalChatModel> logger,
        UserManager<ApplicationUser> userManager,
        IWebHostEnvironment environment,
        AuthDbContext context,
        IHubContext<ChatHub> hubContext)
    {
        _logger = logger;
        _userManager = userManager;
        _environment = environment;
        _context = context;
        _hubContext = hubContext;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            Messages = await _context.Messages
                .OrderBy(m => m.Timestamp)
                .ToListAsync();
            _logger.LogInformation("Retrieved messages from the database.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving messages.");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string Content, IFormFile Image)
    {
        _logger.LogInformation("OnPostSendMessageAsync called with Content: {Content} and Image: {Image}", Content, Image?.FileName);

        if (string.IsNullOrEmpty(Content) && (Image == null || Image.Length == 0))
        {
            _logger.LogWarning("Validation failed: Message or image must be provided.");
            return BadRequest("Message or image must be provided.");
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            _logger.LogWarning("User not found.");
            return BadRequest("User not found.");
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

        var message = new Message
        {
            UserName = user.Email,
            Text = Content,
            Timestamp = DateTime.Now,
            ImageUrl = imageUrl
        };

        _context.Messages.Add(message);

        try
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation("Message saved to database.");
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", user.Email, Content, imageUrl);
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
        return new JsonResult(new { imageUrl });
    }

}
