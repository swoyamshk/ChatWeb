using AuthSystem.Areas.Identity.Data;
using AuthSystem.Hubs;
using AuthSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;


    public class GlobalChatModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AuthDbContext _context;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly ILogger<GlobalChatModel> _logger;
        private readonly IWebHostEnvironment _environment;

    public GlobalChatModel(
            UserManager<ApplicationUser> userManager,
            AuthDbContext context,
            IHubContext<ChatHub> hubContext,
            ILogger<GlobalChatModel> logger,
            IWebHostEnvironment environment)
        {
            _userManager = userManager;
            _context = context;
            _hubContext = hubContext;
            _logger = logger;
            _environment = environment;
        }
    public IList<Message> Messages { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        Messages = await _context.Messages
            .OrderBy(m => m.Timestamp)
            .ToListAsync();

        foreach (var message in Messages)
        {
            if (message.Text == null)
            {
                message.Text = "[No Content]";
            }
            if (message.ImageUrl == null)
            {
                message.ImageUrl = "/path/to/default/image.png";
            }
        }

        return Page();
    }


    public async Task<IActionResult> OnPostAsync(string Content, IFormFile Image)
    {
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

        if (string.IsNullOrEmpty(Content) && string.IsNullOrEmpty(imageUrl))
        {
            ModelState.AddModelError(string.Empty, "Message or image must be provided.");
            return Page();
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
            _logger.LogError(ex, "An unexpected error occurred while saving the message.");
            return StatusCode(500, "An unexpected error occurred while saving the message.");
        }

        return RedirectToPage();
    }





}

