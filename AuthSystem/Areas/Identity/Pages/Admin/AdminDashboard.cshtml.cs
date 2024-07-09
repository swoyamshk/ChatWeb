using AuthSystem.Areas.Identity.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AuthSystem.Areas.Identity.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class AdminDashboardModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
       

        public AdminDashboardModel(UserManager<ApplicationUser> userManager, IUserActivityService userActivityService)
        {
            _userManager = userManager;
          
        }

        public int ActiveUsers { get; set; }
        public int InactiveUsers { get; set; }
        public int TotalUsers { get; set; }
        public int TotalManagers { get; set; }
        public int TotalAdmins { get; set; }

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
        }
    }
}
