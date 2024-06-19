using AuthSystem.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace AuthSystem.Areas.Identity.Pages
{
    public class ResetPasswordAdminModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ResetPasswordAdminModel> _logger;

        public ResetPasswordAdminModel(UserManager<ApplicationUser> userManager, ILogger<ResetPasswordAdminModel> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "Temporary Password")]
            public string TemporaryPassword { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "New Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm New Password")]
            [Compare("Password", ErrorMessage = "The new password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            public string Code { get; set; } // This will hold the token generated for password reset

            public string Email { get; set; }
        }

        public IActionResult OnGet(string token = null, string email = null)
        {
            if (token == null || email == null)
            {
                return BadRequest("A code and email must be supplied for password reset.");
            }

            try
            {
                Input = new InputModel
                {
                    Code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token)),
                    Email = email
                };
            }
            catch (FormatException)
            {
                return BadRequest("Invalid token format.");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToPage("/Index");
            }

            var isTempPasswordValid = await _userManager.CheckPasswordAsync(user, Input.TemporaryPassword);
            if (!isTempPasswordValid)
            {
                ModelState.AddModelError(string.Empty, "Invalid temporary password.");
                return Page();
            }

            var resetPasswordResult = await _userManager.ResetPasswordAsync(user, Input.Code, Input.Password);
            if (resetPasswordResult.Succeeded)
            {
                _logger.LogInformation("User password reset successfully.");
                user.EmailConfirmed = true;
                await _userManager.UpdateAsync(user);

                return RedirectToPage("/Chat");
            }

            foreach (var error in resetPasswordResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }

    }
}
