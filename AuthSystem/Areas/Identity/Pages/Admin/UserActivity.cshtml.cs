using AuthSystem.Areas.Identity.Data;
using AuthSystem.Models;
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
public class UserActivitiesModel : PageModel
{
    private readonly AuthDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public UserActivitiesModel(AuthDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public List<UserActivityViewModel> UserActivities { get; set; }

    [BindProperty(SupportsGet = true)]
    public string CurrentFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? StartDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? EndDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public string SortColumn { get; set; }

    [BindProperty(SupportsGet = true)]
    public string SortDirection { get; set; } = "desc";

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    public int TotalPages { get; set; }

    public class UserActivityViewModel
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string PageName { get; set; }
        public DateTime VisitTime { get; set; }
    }

    public async Task OnGetAsync()
    {
        UserActivities = new List<UserActivityViewModel>();

        var activitiesQuery = _context.UserActivities.Include(ua => ua.User).AsQueryable();

        if (!string.IsNullOrEmpty(CurrentFilter))
        {
            activitiesQuery = activitiesQuery.Where(a => a.User.Email.Contains(CurrentFilter));
        }

        if (StartDate.HasValue)
        {
            activitiesQuery = activitiesQuery.Where(a => a.VisitTime >= StartDate.Value);
        }

        if (EndDate.HasValue)
        {
            activitiesQuery = activitiesQuery.Where(a => a.VisitTime <= EndDate.Value);
        }

        activitiesQuery = SortColumn switch
        {
            "VisitTime" => SortDirection == "asc" ? activitiesQuery.OrderBy(a => a.VisitTime) : activitiesQuery.OrderByDescending(a => a.VisitTime),
            _ => activitiesQuery.OrderByDescending(a => a.VisitTime),
        };

        var totalRecords = await activitiesQuery.CountAsync();
        TotalPages = (int)Math.Ceiling(totalRecords / 10.0);

        var activities = await activitiesQuery
            .Skip((PageNumber - 1) * 10)
            .Take(10)
            .ToListAsync();

        foreach (var activity in activities)
        {
            UserActivities.Add(new UserActivityViewModel
            {
                Id = activity.Id,
                Email = activity.User.Email,
                PageName = activity.PageName,
                VisitTime = activity.VisitTime
            });
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var activity = await _context.UserActivities.FindAsync(id);
        if (activity == null)
        {
            return NotFound();
        }

        _context.UserActivities.Remove(activity);
        await _context.SaveChangesAsync();

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostExportToExcelAsync()
    {
        var activities = await GetFilteredActivitiesAsync();

        using (var package = new ExcelPackage())
        {
            var worksheet = package.Workbook.Worksheets.Add("UserActivities");

            worksheet.Cells[1, 1].Value = "Email";
            worksheet.Cells[1, 2].Value = "Page Name";
            worksheet.Cells[1, 3].Value = "Visit Time";

            for (int i = 0; i < activities.Count; i++)
            {
                worksheet.Cells[i + 2, 1].Value = activities[i].Email;
                worksheet.Cells[i + 2, 2].Value = activities[i].PageName;
                worksheet.Cells[i + 2, 3].Value = activities[i].VisitTime.ToString("yyyy-MM-dd HH:mm:ss");
            }

            var stream = new MemoryStream(package.GetAsByteArray());
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "UserActivities.xlsx");
        }
    }

    public async Task<IActionResult> OnPostExportToPdfAsync()
    {
        var activities = await GetFilteredActivitiesAsync();

        using (var stream = new MemoryStream())
        {
            var document = new Document(PageSize.A4);
            var writer = PdfWriter.GetInstance(document, stream);
            document.Open();

            var table = new PdfPTable(3);
            table.AddCell("Email");
            table.AddCell("Page Name");
            table.AddCell("Visit Time");

            foreach (var activity in activities)
            {
                table.AddCell(activity.Email);
                table.AddCell(activity.PageName);
                table.AddCell(activity.VisitTime.ToString("yyyy-MM-dd HH:mm:ss"));
            }

            document.Add(table);
            document.Close();

            return File(stream.ToArray(), "application/pdf", "UserActivities.pdf");
        }
    }

    private async Task<List<UserActivityViewModel>> GetFilteredActivitiesAsync()
    {
        var activitiesQuery = _context.UserActivities.Include(ua => ua.User).AsQueryable();

        if (!string.IsNullOrEmpty(CurrentFilter))
        {
            activitiesQuery = activitiesQuery.Where(a => a.User.Email.Contains(CurrentFilter));
        }

        if (StartDate.HasValue)
        {
            activitiesQuery = activitiesQuery.Where(a => a.VisitTime >= StartDate.Value);
        }

        if (EndDate.HasValue)
        {
            activitiesQuery = activitiesQuery.Where(a => a.VisitTime <= EndDate.Value);
        }

        activitiesQuery = SortColumn switch
        {
            "VisitTime" => SortDirection == "asc" ? activitiesQuery.OrderBy(a => a.VisitTime) : activitiesQuery.OrderByDescending(a => a.VisitTime),
            _ => activitiesQuery.OrderByDescending(a => a.VisitTime),
        };

        var activities = await activitiesQuery.ToListAsync();

        return activities.Select(a => new UserActivityViewModel
        {
            Id = a.Id,
            Email = a.User.Email,
            PageName = a.PageName,
            VisitTime = a.VisitTime
        }).ToList();
    }
}
