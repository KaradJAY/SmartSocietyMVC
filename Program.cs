using Microsoft.EntityFrameworkCore;
using SmartSocietyMVC.Data;
using SmartSocietyMVC.Models;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

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
    
    // Ensure database is created/migrated
    context.Database.Migrate();

    // ──────────────────────────────────────────────────────────────
    // ADDITIVE SEEDING: each block runs independently so existing
    // databases also get the new dummy data on the next startup.
    // ──────────────────────────────────────────────────────────────

    // 1. Ensure a default society exists
    if (!context.Societies.Any())
    {
        var defaultSociety = new Society
        {
            Name = "Smart Society AutoCraft",
            Address = "123 Main Street, Tech City",
            ContactNumber = "+1 (555) 000-0000",
            CreatedAt = DateTime.UtcNow
        };
        context.Societies.Add(defaultSociety);
        context.SaveChanges();
    }

    var society = context.Societies.OrderBy(s => s.Id).First();

    // 2. Ensure admin user exists
    if (!context.Users.Any(u => u.Role == "admin"))
    {
        context.Users.Add(new User
        {
            Name = "System Admin",
            Email = "admin@society.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
            Role = "admin",
            IsSetup = true,
            CreatedAt = DateTime.UtcNow,
            SocietyId = society.Id
        });
        context.SaveChanges();
    }

    // 3. Seed dummy residents if fewer than 3 residents exist
    if (context.Users.Count(u => u.Role == "resident" && u.SocietyId == society.Id) < 3)
    {
        var existingEmails = context.Users.Select(u => u.Email).ToHashSet();
        var toAdd = new List<User>();

        if (!existingEmails.Contains("ravi@society.com"))
            toAdd.Add(new User { Name = "Ravi Sharma", Email = "ravi@society.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"), Role = "resident", IsSetup = true, FlatNumber = "101", Wing = "A", SocietyId = society.Id });

        if (!existingEmails.Contains("anita@society.com"))
            toAdd.Add(new User { Name = "Anita Desai", Email = "anita@society.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"), Role = "resident", IsSetup = true, FlatNumber = "202", Wing = "B", SocietyId = society.Id });

        if (!existingEmails.Contains("karan@society.com"))
            toAdd.Add(new User { Name = "Karan Patel", Email = "karan@society.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"), Role = "resident", IsSetup = true, FlatNumber = "305", Wing = "C", SocietyId = society.Id });

        if (toAdd.Any())
        {
            context.Users.AddRange(toAdd);
            context.SaveChanges();
        }
    }

    // 4. Seed dummy facilities if none exist for this society
    if (!context.Facilities.Any(f => f.SocietyId == society.Id))
    {
        context.Facilities.AddRange(
            new Facility { Name = "Swimming Pool", Description = "Olympic size swimming pool", Status = "Available", Capacity = 20, OperatingHours = "6 AM - 10 PM", PricePerDay = 500, SocietyId = society.Id },
            new Facility { Name = "Fitness Center", Description = "Modern equipment with AC", Status = "Available", Capacity = 15, OperatingHours = "5 AM - 11 PM", PricePerDay = 200, SocietyId = society.Id },
            new Facility { Name = "Community Hall", Description = "Air-conditioned hall for events", Status = "Available", Capacity = 100, OperatingHours = "8 AM - 11 PM", PricePerDay = 2000, SocietyId = society.Id }
        );
        context.SaveChanges();
    }

    // 5. Seed dummy notices if none exist
    if (!context.Notices.Any(n => n.SocietyId == society.Id))
    {
        context.Notices.AddRange(
            new Notice { Title = "Water Supply Maintenance", Description = "No water from 10 AM to 2 PM on Sunday due to tank cleaning.", Type = "alert", SocietyId = society.Id, CreatedAt = DateTime.UtcNow.AddDays(-1) },
            new Notice { Title = "Holi Celebration Meeting", Description = "Join us at the clubhouse to discuss the upcoming Holi party arrangements.", Type = "event", SocietyId = society.Id, CreatedAt = DateTime.UtcNow }
        );
        context.SaveChanges();
    }

    // 6. Seed dummy bills if none exist
    var ravi = context.Users.FirstOrDefault(u => u.Email == "ravi@society.com");
    if (ravi != null && !context.Bills.Any(b => b.UserId == ravi.Id))
    {
        context.Bills.AddRange(
            new Bill { Amount = 2500, Month = "March 2026", DueDate = DateTime.UtcNow.AddDays(10), Status = "pending", UserId = ravi.Id },
            new Bill { Amount = 1500, Month = "February 2026", DueDate = DateTime.UtcNow.AddDays(-10), Status = "paid", UserId = ravi.Id }
        );
        context.SaveChanges();
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
