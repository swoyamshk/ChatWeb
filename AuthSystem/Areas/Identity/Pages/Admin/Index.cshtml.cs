using AuthSystem.Areas.Identity.Data;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

[Authorize(Roles = "Admin")]
public class IndexModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public IndexModel(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public List<UserViewModel> Users { get; set; }
    public List<string> Roles { get; set; }

    [BindProperty(SupportsGet = true)]
    public string CurrentFilter { get; set; }
    [BindProperty(SupportsGet = true)]
    public DateTime? StartDate { get; set; }
    [BindProperty(SupportsGet = true)]
    public DateTime? EndDate { get; set; }
    [BindProperty(SupportsGet = true)]
    public string SelectedRole { get; set; }

    public class UserViewModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public IList<string> Roles { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public string ActiveStatus { get; set; }

    }

    public async Task OnGetAsync()
    {
        Users = new List<UserViewModel>();
        Roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();

        var usersQuery = _userManager.Users.AsQueryable();

        if (!string.IsNullOrEmpty(CurrentFilter))
        {
            usersQuery = usersQuery.Where(u => u.Email.Contains(CurrentFilter));
        }

        if (StartDate.HasValue)
        {
            usersQuery = usersQuery.Where(u => u.LastLoginDate >= StartDate);
        }

        if (EndDate.HasValue)
        {
            usersQuery = usersQuery.Where(u => u.LastLoginDate <= EndDate);
        }

        var users = await usersQuery.ToListAsync();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);

            if (string.IsNullOrEmpty(SelectedRole) || roles.Contains(SelectedRole))
            {
                Users.Add(new UserViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Roles = roles.ToList(),
                    LastLoginDate = user.LastLoginDate,
                    ActiveStatus = user.ActiveStatus
                });
            }
        }
    }

    public async Task<IActionResult> OnPostExportToExcelAsync()
    {
        var users = await GetFilteredUsersAsync();

        using (var package = new ExcelPackage())
        {
            var worksheet = package.Workbook.Worksheets.Add("Users");

            worksheet.Cells[1, 1].Value = "Email";
            worksheet.Cells[1, 2].Value = "Full Name";
            worksheet.Cells[1, 3].Value = "Roles";
            worksheet.Cells[1, 4].Value = "Last Logged In";
            worksheet.Cells[1, 5].Value = "Activity Status";

            for (int i = 0; i < users.Count; i++)
            {
                var user = users[i];
                worksheet.Cells[i + 2, 1].Value = user.Email;
                worksheet.Cells[i + 2, 2].Value = $"{user.FirstName} {user.LastName}";
                worksheet.Cells[i + 2, 3].Value = string.Join(", ", user.Roles);
                worksheet.Cells[i + 2, 4].Value = user.LastLoginDate.HasValue ? user.LastLoginDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : "Never Logged In";
                worksheet.Cells[i + 2, 5].Value = user.ActiveStatus;
            }

            var stream = new MemoryStream(package.GetAsByteArray());
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Users.xlsx");
        }
    }

    public async Task<IActionResult> OnPostExportToPdfAsync()
    {
        var users = await GetFilteredUsersAsync();

        using (var stream = new MemoryStream())
        {
            var document = new Document(PageSize.A4);
            var writer = PdfWriter.GetInstance(document, stream);
            document.Open();

            var table = new PdfPTable(5);
            table.AddCell("Email");
            table.AddCell("Full Name");
            table.AddCell("Roles");
            table.AddCell("Last Logged In");
            table.AddCell("Activity Status");

            foreach (var user in users)
            {
                table.AddCell(user.Email);
                table.AddCell($"{user.FirstName} {user.LastName}");
                table.AddCell(string.Join(", ", user.Roles));
                table.AddCell(user.LastLoginDate.HasValue ? user.LastLoginDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : "Never Logged In");
                table.AddCell(user.ActiveStatus);
            }

            document.Add(table);
            document.Close();

            return File(stream.ToArray(), "application/pdf", "Users.pdf");
        }
    }

    private async Task<List<UserViewModel>> GetFilteredUsersAsync()
    {
        var usersQuery = _userManager.Users.AsQueryable();

        if (!string.IsNullOrEmpty(CurrentFilter))
        {
            usersQuery = usersQuery.Where(u => u.Email.Contains(CurrentFilter));
        }

        var users = await usersQuery.ToListAsync();

        var userViewModels = new List<UserViewModel>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);

            userViewModels.Add(new UserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Roles = roles.ToList(),
                LastLoginDate = user.LastLoginDate,
                ActiveStatus = user.ActiveStatus
            });
        }

        return userViewModels;
    }
}
