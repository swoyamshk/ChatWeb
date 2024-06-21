using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using AuthSystem.Services;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using Microsoft.AspNetCore.Identity;
using AuthSystem.Areas.Identity.Data;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authorization;

namespace AuthSystem.Areas.Identity.Pages
{
    [Authorize(Roles = "Admin")]
    public class AdminUserCreationModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AdminUserCreationModel> _logger;
        private readonly IEmailSender _emailSender;

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [StringLength(50)]
            public string FirstName { get; set; }

            [Required]
            [StringLength(50)]
            public string LastName { get; set; }

            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public AdminUserCreationModel(UserManager<ApplicationUser> userManager, ILogger<AdminUserCreationModel> logger, IEmailSender emailSender)
        {
            _userManager = userManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = new ApplicationUser
            {
                FirstName = Input.FirstName,
                LastName = Input.LastName,
                UserName = Input.Email,
                Email = Input.Email,
            };
            var temporaryPassword = GenerateTemporaryPassword();

            var passwordHasher = new PasswordHasher<ApplicationUser>();
            var hashedPassword = passwordHasher.HashPassword(user, temporaryPassword);

            user.PasswordHash = hashedPassword;
            // Generate a temporary password

            var result = await _userManager.CreateAsync(user);
            if (result.Succeeded)
            {
                user.ActiveStatus = "Inactive";
                await _userManager.AddToRoleAsync(user, "Manager");


                _logger.LogInformation("User created a new account with password.");

                var code = await _userManager.GeneratePasswordResetTokenAsync(user);

                var callbackUrl = Url.Page(
                    "/ResetPasswordAdmin",
                    pageHandler: null,
                    values: new { token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code)), email = user.Email },
                    protocol: Request.Scheme);

                await _emailSender.SendEmailAsync(
                    Input.Email,
                    "Setup Your Password",
                     $"Your temporary password is {temporaryPassword}. Please set up your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                return RedirectToPage("/AdminUserCreationConfirmation");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }

        private string GenerateTemporaryPassword(int length = 12)
        {
            if (length < 4)
                throw new ArgumentException("Password length must be at least 4.", nameof(length));

            const string lowerCaseChars = "abcdefghijklmnopqrstuvwxyz";
            const string upperCaseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string digitChars = "1234567890";
            const string specialChars = "!@#$%^&*()";
            const string allChars = lowerCaseChars + upperCaseChars + digitChars + specialChars;

            StringBuilder res = new StringBuilder();
            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] uintBuffer = new byte[sizeof(uint)];

                // Ensure at least one of each required character type
                res.Append(GetRandomChar(lowerCaseChars, rng, uintBuffer));
                res.Append(GetRandomChar(upperCaseChars, rng, uintBuffer));
                res.Append(GetRandomChar(digitChars, rng, uintBuffer));
                res.Append(GetRandomChar(specialChars, rng, uintBuffer));

                // Fill the rest of the password length with random characters from all character sets
                for (int i = 4; i < length; i++)
                {
                    res.Append(GetRandomChar(allChars, rng, uintBuffer));
                }
            }

            // Shuffle the resulting characters to avoid any predictable pattern
            return Shuffle(res.ToString());
        }

        private char GetRandomChar(string chars, RNGCryptoServiceProvider rng, byte[] uintBuffer)
        {
            rng.GetBytes(uintBuffer);
            uint num = BitConverter.ToUInt32(uintBuffer, 0);
            return chars[(int)(num % (uint)chars.Length)];
        }

        private string Shuffle(string input)
        {
            char[] array = input.ToCharArray();
            Random rng = new Random();
            int n = array.Length;
            while (n > 1)
            {
                int k = rng.Next(n--);
                (array[n], array[k]) = (array[k], array[n]);
            }
            return new string(array);
        }
    }
}
