using AuthSystem.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class DisabledUsersModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;

    public DisabledUsersModel(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public List<UserViewModel> DisabledUsers { get; set; }

    public class UserViewModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public IList<string> Roles { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public bool IsDisabled { get; set; }
    }

    public async Task OnGetAsync()
    {
        DisabledUsers = new List<UserViewModel>();

        var users = await _userManager.Users.Where(u => u.IsDisabled == true).ToListAsync();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);

            // Exclude users with the "Admin" role
            if (roles.Contains("Admin"))
                continue;

            DisabledUsers.Add(new UserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Roles = roles.ToList(),
                LastLoginDate = user.LastLoginDate,
                IsDisabled = user.IsDisabled // Assign IsDisabled property
            });
        }
    }

}
