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

            public string FirstName { get; set; }

            public string LastName { get; set; }

            [Required(ErrorMessage = "Please select at least one role.")]
            public IList<string> Roles { get; set; }

            [Required(ErrorMessage = "Please select an active status.")]
            public string ActiveStatus { get; set; }
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
                Roles = roles.ToList(),
                ActiveStatus = user.ActiveStatus
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

                var roles = await _userManager.GetRolesAsync(user);
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
                var roleResult = await _userManager.RemoveFromRolesAsync(user, roles);
                if (!roleResult.Succeeded)
                {
                    foreach (var error in roleResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return Page();
                }

                roleResult = await _userManager.AddToRolesAsync(user, Input.Roles);
                if (!roleResult.Succeeded)
                {
                    foreach (var error in roleResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return Page();
                }

                return RedirectToPage("Index");
            }

            return Page();
        }
    }
