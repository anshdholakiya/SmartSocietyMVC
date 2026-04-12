using Microsoft.EntityFrameworkCore;
using SmartSocietyMVC.Data;
using SmartSocietyMVC.Models;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddControllersWithViews();

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
    
    // Ensure database is created
    context.Database.EnsureCreated();

    if (!context.Societies.Any())
    {
        var defaultSociety = new Society
        {
            Name = "Smart Society",
            Address = "123 Main Street, Tech City",
            ContactNumber = "+1 (555) 000-0000",
            CreatedAt = DateTime.UtcNow
        };
        context.Societies.Add(defaultSociety);
        context.SaveChanges();

        if (!context.Users.Any())
        {
            var adminUser = new User
            {
                Name = "System Admin",
                Email = "admin@society.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"), // Default admin password
                Role = "admin",
                IsSetup = true,
                CreatedAt = DateTime.UtcNow,
                SocietyId = defaultSociety.Id
            };
            context.Users.Add(adminUser);
            context.SaveChanges();
        }
    }

    var society = context.Societies.FirstOrDefault();
    if (society != null)
    {
        // Seed Facilities
        if (!context.Facilities.Any())
        {
            var facilities = new List<Facility>
            {
                new Facility { Name = "Clubhouse", Description = "Main clubhouse for events and gatherings.", Capacity = 100, PricePerDay = 5000, SocietyId = society.Id },
                new Facility { Name = "Swimming Pool", Description = "Olympic size swimming pool.", Capacity = 30, PricePerDay = 1000, SocietyId = society.Id },
                new Facility { Name = "Gymnasium", Description = "Fully equipped gym with modern machines.", Capacity = 20, PricePerDay = 500, SocietyId = society.Id }
            };
            context.Facilities.AddRange(facilities);
            context.SaveChanges();
        }

        // Seed Notices
        if (!context.Notices.Any())
        {
            var notices = new List<Notice>
            {
                new Notice { Type = "alert", Title = "Water Supply Interruption", Description = "Water supply will be interrupted tomorrow.", CreatedAt = DateTime.UtcNow.AddDays(-1), SocietyId = society.Id }
            };
            context.Notices.AddRange(notices);
            context.SaveChanges();
        }
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
