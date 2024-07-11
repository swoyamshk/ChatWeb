using AuthSystem.Areas.Identity.Data;
using AuthSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthSystem.Areas.Identity.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class AdminDashboardModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AuthDbContext _context;

        public AdminDashboardModel(UserManager<ApplicationUser> userManager, AuthDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public int ActiveUsers { get; set; }
        public int InactiveUsers { get; set; }
        public int TotalUsers { get; set; }
        public int TotalManagers { get; set; }
        public int TotalAdmins { get; set; }
        public List<string> PageVisitLabels { get; set; }
        public List<int> PageVisitData { get; set; }
        public List<string> MessageLabels { get; set; }
        public List<int> MessageData { get; set; }

        public async Task OnGetAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            TotalUsers = users.Count;

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                if (roles.Contains("Admin"))
                {
                    TotalAdmins++;
                }

                if (roles.Contains("Manager"))
                {
                    TotalManagers++;
                }

                if (user.ActiveStatus == "Active")
                {
                    ActiveUsers++;
                }
                else
                {
                    InactiveUsers++;
                }
            }

            var pageVisits = await _context.UserActivities
                .GroupBy(ua => ua.PageName)  
                .Select(g => new { PageName = g.Key, Count = g.Count() })
                .ToListAsync();

            PageVisitLabels = pageVisits.Select(pv => pv.PageName).ToList();
            PageVisitData = pageVisits.Select(pv => pv.Count).ToList();

            // Fetching message data
            var messages = await _context.ChatMessages
                .GroupBy(m => m.Timestamp.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync();

            MessageLabels = messages.Select(m => m.Date.ToShortDateString()).ToList();
            MessageData = messages.Select(m => m.Count).ToList();
        }
    }
}
