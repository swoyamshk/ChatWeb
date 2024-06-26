using AuthSystem.Areas.Identity.Data; // Adjust namespace if necessary
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

    [Authorize(Roles = "Admin")]
    public class EditModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public EditModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        [Required(ErrorMessage = "First Name is required.")]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "Last Name is required")]
        public string LastName { get; set; }

            [Required(ErrorMessage = "Please select an active status.")]
            public string ActiveStatus { get; set; }
        public bool IsDisabled { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user == null)
        {
            return NotFound();
        }

        var roles = await _userManager.GetRolesAsync(user);

        Input = new InputModel
        {
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            ActiveStatus = user.ActiveStatus,
            IsDisabled = user.IsDisabled
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string id)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            user.Email = Input.Email;
            user.UserName = Input.Email;
            user.FirstName = Input.FirstName;
            user.LastName = Input.LastName;
            user.ActiveStatus = Input.ActiveStatus;
            user.IsDisabled = Input.IsDisabled;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return Page();
            }

            // Remove current roles and add selected roles

            return RedirectToPage("Index");
        }

        return Page();
    }


}
