using Microsoft.EntityFrameworkCore;
using SmartSocietyMVC.Data;
using SmartSocietyMVC.Models;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

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
    
    // Ensure database is created/migrated
    context.Database.Migrate();

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
