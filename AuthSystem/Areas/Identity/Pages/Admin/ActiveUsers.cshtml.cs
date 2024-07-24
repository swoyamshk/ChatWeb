using AuthSystem.Areas.Identity.Data;
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
public class ActiveUsersModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;

    public ActiveUsersModel(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    // Properties for date filtering
    [BindProperty(SupportsGet = true)]
    public DateTime? StartDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? EndDate { get; set; }

    public List<UserViewModel> InactiveUsers { get; set; }
    public string SearchString { get; set; }

    public class UserViewModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public IList<string> Roles { get; set; }
        public DateTime? LastLoginDate { get; set; }
    }

    public async Task OnGetAsync(string searchString)
    {
        IQueryable<ApplicationUser> usersQuery = _userManager.Users.Where(u => u.ActiveStatus == "Inactive");

        // Apply search filter
        if (!string.IsNullOrEmpty(searchString))
        {
            usersQuery = usersQuery.Where(u => u.Email.Contains(searchString));
        }

        // Apply date filters
        if (StartDate.HasValue)
        {
            usersQuery = usersQuery.Where(u => u.LastLoginDate >= StartDate.Value);
        }
        if (EndDate.HasValue)
        {
            usersQuery = usersQuery.Where(u => u.LastLoginDate <= EndDate.Value);
        }

        var users = await usersQuery.ToListAsync();

        InactiveUsers = new List<UserViewModel>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);

            // Ensure roles other than "Admin" are included
            if (!roles.Contains("Admin"))
            {
                InactiveUsers.Add(new UserViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName, // Add if exists in ApplicationUser
                    LastName = user.LastName,   // Add if exists in ApplicationUser
                    Roles = roles.ToList(),     // Store all roles for the user
                    LastLoginDate = user.LastLoginDate
                });
            }
        }

        // Assign the search string and date filters to properties to maintain state
        SearchString = searchString;
    }
}