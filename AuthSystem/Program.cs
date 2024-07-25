using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AuthSystem.Areas.Identity.Data;
using AuthSystem.Services;
using SendGrid.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity.UI.Services;
using AuthSystem.Hubs;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("AuthDbContextConnection") ?? throw new InvalidOperationException("Connection string 'AuthDbContextConnection' not found.");
var services = builder.Services;
var configuration = builder.Configuration;

builder.Services.AddSignalR();

services.AddAuthentication().AddGoogle(googleOptions =>
{
    googleOptions.ClientId = configuration["Authentication:Google:ClientId"];
    googleOptions.ClientSecret = configuration["Authentication:Google:ClientSecret"];
    googleOptions.Scope.Add("profile");
});

builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
});


builder.Services.AddDbContext<AuthDbContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AuthDbContext>();

services.AddScoped<IUserActivityService, UserActivityService>();

builder.Services.Configure<DataProtectionTokenProviderOptions>(options => options.TokenLifespan = TimeSpan.FromHours(1)); // Token valid for 1 hour

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Configure SendGrid Settings to Dotnet Application
builder.Services.Configure<SendGridSettings>(builder.Configuration.GetSection("SendGridSettings"));
builder.Services.AddSendGrid(options =>
{
    options.ApiKey = builder.Configuration.GetSection("SendGridSettings").GetValue<string>("ApiKey"); // Configuring ApiKey for SendGrid Library
});

builder.Services.AddScoped<IEmailSender, EmailSenderService>();

// Register the bot framework
builder.Services.AddSingleton<IBotFrameworkHttpAdapter, BotFrameworkHttpAdapter>();
builder.Services.AddSingleton<IBot, SupportBot>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();
app.MapHub<ChatHub>("/chatHub");
app.MapHub<PrivateChatHub>("/privatechathub");
app.MapHub<SupportBotHub>("/supportBotHub");

// Add Roles to the Database
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    var roles = new[] { "Admin", "Manager", "User" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }
}

// Add Admin account to the database
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    string email = configuration["AdminUser:Email"];
    string password = configuration["AdminUser:Password"];

    logger.LogInformation("Attempting to create admin user with Email: {Email}", email);

    if (await userManager.FindByEmailAsync(email) == null)
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FirstName = "Admin",
            LastName =""
        };

        var result = await userManager.CreateAsync(user, password);

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, "Admin");
            logger.LogInformation("Admin user created successfully.");
        }
        else
        {
            foreach (var error in result.Errors)
            {
                logger.LogError("Error creating admin user: {Error}", error.Description);
            }
        }
    }
    else
    {
        logger.LogInformation("Admin user already exists.");
    }
}

app.Run();
