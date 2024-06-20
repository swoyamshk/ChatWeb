using AuthSystem.Areas.Identity.Data; 
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class ActiveUsersModel : PageModel
{
	private readonly UserManager<ApplicationUser> _userManager;

	public ActiveUsersModel(UserManager<ApplicationUser> userManager)
	{
		_userManager = userManager;
	}

	public List<UserViewModel> ActiveUsers { get; set; }

	public class UserViewModel
	{
		public string Id { get; set; }
		public string Email { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public IList<string> Roles { get; set; }
		public DateTime? LastLoginDate { get; set; }
	}

	public async Task OnGetAsync()
	{
		ActiveUsers = new List<UserViewModel>();

		var users = await _userManager.Users.Where(u => u.ActiveStatus == "Active").ToListAsync();

		foreach (var user in users)
		{
			var roles = await _userManager.GetRolesAsync(user);

			ActiveUsers.Add(new UserViewModel
			{
				Id = user.Id,
				Email = user.Email,
				FirstName = user.FirstName, // Add if exists in ApplicationUser
				LastName = user.LastName,   // Add if exists in ApplicationUser
				Roles = roles.ToList(),
				LastLoginDate = user.LastLoginDate
			});
		}
	}
}
