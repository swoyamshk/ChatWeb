using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;


namespace AuthSystem.Areas.Identity.Pages
{
[Authorize(Roles = "Admin")]
    public class AdminUserCreationConfirmationModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
