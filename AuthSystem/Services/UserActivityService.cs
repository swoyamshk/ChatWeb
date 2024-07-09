using AuthSystem.Areas.Identity.Data;
using AuthSystem.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

public interface IUserActivityService
{
    Task LogActivity(string userId, string pageName);
}

public class UserActivityService : IUserActivityService
{
    private readonly AuthDbContext _context;

    public UserActivityService(AuthDbContext context)
    {
        _context = context;
    }

    public async Task LogActivity(string userId, string pageName)
    {
        var activity = new UserActivity
        {
            UserId = userId,
            PageName = pageName,
            VisitTime = DateTime.Now
        };

        _context.UserActivities.Add(activity);
        await _context.SaveChangesAsync();
    }
}
