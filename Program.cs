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

    var society = context.Societies.FirstOrDefault();
    if (society != null)
    {
        // Seed resident users
        if (context.Users.Count() == 1) // only admin exists
        {
            var resident1 = new User { Name = "John Doe", Email = "john@society.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("resident123"), Role = "resident", FlatNumber = "A-101", Wing = "A", IsSetup = true, CreatedAt = DateTime.UtcNow, SocietyId = society.Id };
            var resident2 = new User { Name = "Jane Smith", Email = "jane@society.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("resident123"), Role = "resident", FlatNumber = "B-202", Wing = "B", IsSetup = true, CreatedAt = DateTime.UtcNow, SocietyId = society.Id };
            context.Users.AddRange(resident1, resident2);
            context.SaveChanges();
        }

        var residents = context.Users.Where(u => u.Role == "resident").ToList();

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
                new Notice { Type = "alert", Title = "Water Supply Interruption", Description = "Water supply will be interrupted tomorrow from 10 AM to 2 PM for maintenance.", CreatedAt = DateTime.UtcNow.AddDays(-1), SocietyId = society.Id },
                new Notice { Type = "event", Title = "Annual General Meeting", Description = "The AGM will be held on the 15th of next month in the Clubhouse. All residents are requested to attend.", CreatedAt = DateTime.UtcNow.AddDays(-2), SocietyId = society.Id },
                new Notice { Type = "maintenance", Title = "Lift Maintenance in Wing A", Description = "Lift 1 in Wing A will be under maintenance on Friday.", CreatedAt = DateTime.UtcNow, SocietyId = society.Id }
            };
            context.Notices.AddRange(notices);
            context.SaveChanges();
        }

        // Seed data depending on residents and facilities
        if (residents.Any())
        {
            var r1 = residents.FirstOrDefault();
            var pool = context.Facilities.FirstOrDefault(f => f.Name == "Swimming Pool");

            if (!context.Bookings.Any() && r1 != null && pool != null)
            {
                var booking = new Booking { Date = DateTime.UtcNow.AddDays(5), Days = 1, TotalPrice = pool.PricePerDay, Purpose = "Birthday Party", Status = "approved", FacilityId = pool.Id, UserId = r1.Id };
                context.Bookings.Add(booking);
                context.SaveChanges();
            }

            if (!context.Complaints.Any() && r1 != null)
            {
                var complaints = new List<Complaint>
                {
                    new Complaint { Title = "Leaking Pipe", Description = "There is a leaking pipe near parking slot A-101.", Status = "pending", CreatedAt = DateTime.UtcNow.AddDays(-2), UserId = r1.Id, SocietyId = society.Id },
                    new Complaint { Title = "Streetlight flickering", Description = "The streetlight near the main gate is flickering.", Status = "resolved", CreatedAt = DateTime.UtcNow.AddDays(-10), UserId = r1.Id, SocietyId = society.Id }
                };
                context.Complaints.AddRange(complaints);
                context.SaveChanges();
            }

            if (!context.Bills.Any() && r1 != null)
            {
                var bills = new List<Bill>
                {
                    new Bill { Title = "Maintenance for March", Month = "March 2026", Description = "Monthly maintenance charges.", Amount = 2500, DueDate = DateTime.UtcNow.AddDays(10), IsPaid = false, Status = "pending", Category = "maintenance", CreatedAt = DateTime.UtcNow, UserId = r1.Id },
                    new Bill { Title = "Event Contribution", Month = "March 2026", Description = "Contribution for the upcoming festival.", Amount = 500, DueDate = DateTime.UtcNow.AddDays(-5), IsPaid = true, Status = "paid", Category = "others", CreatedAt = DateTime.UtcNow.AddDays(-15), UserId = r1.Id }
                };
                context.Bills.AddRange(bills);
                context.SaveChanges();
            }
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
