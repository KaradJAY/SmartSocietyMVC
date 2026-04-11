using Microsoft.EntityFrameworkCore;
using SmartSocietyMVC.Data;
using SmartSocietyMVC.Models;
using System.Linq;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? builder.Configuration["POSTGRESQLCONNSTR_DefaultConnection"]
    ?? builder.Configuration["CUSTOMCONNSTR_DefaultConnection"]
    ?? "Host=fake;Database=fake;Username=fake;Password=fake";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<SmartSocietyMVC.Filters.ProfilePictureFilter>();
});
builder.Services.AddTransient<SmartSocietyMVC.Services.IEmailSender, SmartSocietyMVC.Services.EmailSender>();

builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
    });
var app = builder.Build();

// Seed Database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    
    try
    {
        // Ensure database is created/migrated
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        Console.WriteLine("STARTUP MIGRATION FAILED: " + ex.Message);
    }
}

// Configure the HTTP request pipeline.
app.UseDeveloperExceptionPage();

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
