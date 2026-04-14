using Microsoft.EntityFrameworkCore;
using SmartSocietyMVC.Data;
using SmartSocietyMVC.Models;
using System.Linq;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

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
    try 
    {
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<ApplicationDbContext>();
        
        // Ensure database is created
        context.Database.EnsureCreated();

        // Ensure Society exists
        var defaultSociety = context.Societies.FirstOrDefault();
        if (defaultSociety == null)
        {
            defaultSociety = new Society { Name = "Smart Society", Address = "123 Main St" };
            context.Societies.Add(defaultSociety);
            context.SaveChanges();
        }

        // Ensure Admin exists
        if (!context.Users.Any(u => u.Email == "masteradmin@society.com"))
        {
            context.Users.Add(new User
            {
                Name = "Master Admin",
                Email = "masteradmin@society.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("master123"),
                Role = "admin",
                IsSetup = true,
                SocietyId = defaultSociety.Id
            });
            context.SaveChanges();
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Critical Database Startup Failure: {ex.Message}");
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



