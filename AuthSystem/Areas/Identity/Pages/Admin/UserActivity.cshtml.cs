using AuthSystem.Areas.Identity.Data;
using AuthSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

[Authorize(Roles = "Admin")]
public class UserActivitiesModel : PageModel
{
    private readonly AuthDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public UserActivitiesModel(AuthDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public List<UserActivityViewModel> UserActivities { get; set; }
    [BindProperty(SupportsGet = true)]
    public string CurrentFilter { get; set; }

    public class UserActivityViewModel
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string PageName { get; set; }
        public DateTime VisitTime { get; set; }
    }

    public async Task OnGetAsync()
    {
        UserActivities = new List<UserActivityViewModel>();

        var activitiesQuery = _context.UserActivities.Include(ua => ua.User).AsQueryable();

        if (!string.IsNullOrEmpty(CurrentFilter))
        {
            activitiesQuery = activitiesQuery.Where(a => a.User.Email.Contains(CurrentFilter));
        }

        var activities = await activitiesQuery.ToListAsync();

        foreach (var activity in activities)
        {
            UserActivities.Add(new UserActivityViewModel
            {
                Id = activity.Id,
                Email = activity.User.Email,
                PageName = activity.PageName,
                VisitTime = activity.VisitTime
            });
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var activity = await _context.UserActivities.FindAsync(id);
        if (activity == null)
        {
            return NotFound();
        }

        _context.UserActivities.Remove(activity);
        await _context.SaveChangesAsync();

        return RedirectToPage();
    }
}
