using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SmartSocietyMVC.Data;
using Microsoft.EntityFrameworkCore;
using SmartSocietyMVC.Models;

namespace SmartSocietyMVC.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            ViewData["ActiveTab"] = "dashboard";
            ViewBag.Stats = new 
            {
                TotalCollected = 150000,
                TotalPending = 25000,
                TotalResidents = 120,
                ActiveIssues = 4
            };
            return View();
        }

        public async Task<IActionResult> Notices()
        {
            ViewData["ActiveTab"] = "notices";
            
            var societyIdClaim = User.FindFirstValue("SocietyId");
            if (!int.TryParse(societyIdClaim, out int societyId)) societyId = 1; // Fallback for old sessions

            var notices = await _context.Notices
                .Where(n => n.SocietyId == societyId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return View(notices);
        }

        [HttpPost]
        public async Task<IActionResult> PostNotice(string title, string type, string description)
        {
            var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            var societyIdClaim = User.FindFirstValue("SocietyId");

            if (userRole != "admin") return Unauthorized();
            if (!int.TryParse(societyIdClaim, out int societyId)) societyId = 1; // Fallback

            var notice = new Notice
            {
                Title = title,
                Type = type ?? "alert",
                Description = description,
                CreatedAt = DateTime.UtcNow,
                SocietyId = societyId
            };

            _context.Notices.Add(notice);
            await _context.SaveChangesAsync();
            return RedirectToAction("Notices");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteNotice(int id)
        {
            var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            if (userRole != "admin") return Unauthorized();

            var notice = await _context.Notices.FindAsync(id);
            if (notice != null)
            {
                _context.Notices.Remove(notice);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Notices");
        }

        public async Task<IActionResult> Complaints()
        {
            ViewData["ActiveTab"] = "complaints";
            
            var societyIdClaim = User.FindFirstValue("SocietyId");
            if (!int.TryParse(societyIdClaim, out int societyId)) societyId = 1; // Fallback for old sessions

            var complaints = await _context.Complaints
                .Include(c => c.User)
                .Where(c => c.SocietyId == societyId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return View(complaints);
        }

        [HttpPost]
        public async Task<IActionResult> FileComplaint(string title, string description, IFormFile image)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var societyIdString = User.FindFirstValue("SocietyId");

            if (!int.TryParse(userIdString, out int userId)) return RedirectToAction("Login", "Account");
            if (!int.TryParse(societyIdString, out int societyId)) societyId = 1; // Fallback

            var complaint = new Complaint
            {
                Title = title,
                Description = description,
                Status = "pending",
                CreatedAt = DateTime.UtcNow,
                UserId = userId,
                SocietyId = societyId
            };

            if (image != null && image.Length > 0)
            {
                // Simple file upload for proof of concept. In production, use cloud storage.
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                Directory.CreateDirectory(uploadsFolder);
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + image.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(fileStream);
                }
                complaint.ImageUrl = "/uploads/" + uniqueFileName;
            }

            _context.Complaints.Add(complaint);
            await _context.SaveChangesAsync();

            return RedirectToAction("Complaints");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateComplaintStatus(int id, string status)
        {
            var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            if (userRole != "admin") return Unauthorized();

            var complaint = await _context.Complaints.FindAsync(id);
            if (complaint != null)
            {
                complaint.Status = status;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Complaints");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteComplaint(int id)
        {
            var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            if (userRole != "admin") return Unauthorized();

            var complaint = await _context.Complaints.FindAsync(id);
            if (complaint != null)
            {
                _context.Complaints.Remove(complaint);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Complaints");
        }

        public IActionResult Society()
        {
            ViewData["ActiveTab"] = "society";
            ViewBag.Society = new {
                name = "AutoCraft Heights",
                address = "123 Smart Drive, Tech Park",
                contactNumber = "+1 (555) 123-4567",
                amenities = new[] { "Swimming Pool", "Gymnasium", "Club House", "24/7 Security" },
                gallery = new[] { "https://images.unsplash.com/photo-1545324418-cc1a3fa10c00?w=500", "https://images.unsplash.com/photo-1512917774080-9991f1c4c750?w=500" }
            };
            return View();
        }

        public async Task<IActionResult> Facilities()
        {
            ViewData["ActiveTab"] = "facilities";
            
            var societyIdClaim = User.FindFirstValue("SocietyId");
            if (!int.TryParse(societyIdClaim, out int societyId)) societyId = 1; // Fallback

            var facilities = await _context.Facilities
                .Where(f => f.SocietyId == societyId)
                .OrderBy(f => f.Name)
                .ToListAsync();

            return View(facilities);
        }

        [HttpPost]
        public async Task<IActionResult> SaveFacility(int? id, string name, decimal pricePerDay, int capacity, string description)
        {
            var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            var societyIdClaim = User.FindFirstValue("SocietyId");

            if (userRole != "admin") return Unauthorized();
            if (!int.TryParse(societyIdClaim, out int societyId)) societyId = 1; // Fallback

            if (id.HasValue && id.Value > 0)
            {
                // Update
                var facility = await _context.Facilities.FindAsync(id.Value);
                if (facility != null)
                {
                    facility.Name = name;
                    facility.PricePerDay = pricePerDay;
                    facility.Capacity = capacity;
                    facility.Description = description;
                    _context.Facilities.Update(facility);
                }
            }
            else
            {
                // Create
                var newFacility = new Facility
                {
                    Name = name,
                    PricePerDay = pricePerDay,
                    Capacity = capacity,
                    Description = description,
                    SocietyId = societyId
                };
                _context.Facilities.Add(newFacility);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Facilities");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteFacility(int id)
        {
            var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            if (userRole != "admin") return Unauthorized();

            var facility = await _context.Facilities.FindAsync(id);
            if (facility != null)
            {
                _context.Facilities.Remove(facility);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Facilities");
        }

        public IActionResult Bookings()
        {
            ViewData["ActiveTab"] = "bookings";
            ViewBag.Bookings = new List<dynamic>
            {
                new { id = 1, Facility = new { name = "Community Hall" }, date = DateTime.Now.AddDays(5), User = new { name = "John Doe", wing = "A", flatNumber = "101" }, days = 1, totalPrice = 5000, purpose = "Birthday Party", status = "pending" },
                new { id = 2, Facility = new { name = "Swimming Pool Area" }, date = DateTime.Now.AddDays(10), User = new { name = "Alice Smith", wing = "B", flatNumber = "205" }, days = 2, totalPrice = 4000, purpose = "Weekend Gathering", status = "approved" }
            };
            return View();
        }

        [HttpGet]
        public IActionResult Invite()
        {
            ViewData["ActiveTab"] = "invite";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Invite(string name, string email, string role, string wing, string flatNumber)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(name))
            {
                TempData["ErrorMessage"] = "Name and Email are required.";
                return RedirectToAction("Invite");
            }

            var societyIdClaim = User.FindFirstValue("SocietyId");
            if (string.IsNullOrEmpty(societyIdClaim) || !int.TryParse(societyIdClaim, out int societyId))
            {
                TempData["ErrorMessage"] = "Unauthorized or missing Society ID.";
                return RedirectToAction("Index");
            }

            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (existingUser != null)
            {
                TempData["ErrorMessage"] = "A user with this email already exists.";
                return RedirectToAction("Invite");
            }

            var newUser = new User
            {
                Name = name,
                Email = email,
                Role = role ?? "resident",
                Wing = wing,
                FlatNumber = flatNumber,
                PasswordHash = "",
                IsSetup = false,
                SocietyId = societyId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            // Example link: /Account/SetupPassword?token=email
            string setupLink = $"/Account/SetupPassword?token={email}";

            TempData["SuccessMessage"] = $"Invitation generated! Share this link with the resident: {setupLink}";
            return RedirectToAction("Invite");
        }

        public IActionResult Bills()
        {
            ViewData["ActiveTab"] = "bills";
            
            // Mock bills for the UI
            ViewBag.AllBills = new List<dynamic>
            {
                new { id = 1, amount = 2500, month = "March 2026 Maintenance", description = "Monthly dues", status = "paid", User = new { name = "John Doe", email = "john@example.com" } },
                new { id = 2, amount = 1500, month = "Event Fund", description = "Holi Contribution", status = "pending", User = new { name = "Alice Smith", email = "alice@example.com" } }
            };
            
            ViewBag.MyBills = new List<dynamic>
            {
                new { id = 1, amount = 2500, month = "March 2026", description = "Maintenance", status = "paid" }
            };
            
            return View();
        }

        public IActionResult Residents()
        {
            ViewData["ActiveTab"] = "residents";
            ViewBag.Users = new List<dynamic>
            {
                new { id = 1, name = "Admin User", email = "admin@society.com", role = "admin", wing = "A", flatNumber = "Penthouse", isSetup = true, createdAt = DateTime.Now.AddYears(-1) },
                new { id = 2, name = "John Doe", email = "john@example.com", role = "resident", wing = "A", flatNumber = "101", isSetup = true, createdAt = DateTime.Now.AddMonths(-5) },
                new { id = 3, name = "Pending Resident", email = "pending@example.com", role = "resident", wing = "B", flatNumber = "202", isSetup = false, createdAt = DateTime.Now.AddDays(-2) }
            };
            return View();
        }

        public async Task<IActionResult> Events()
        {
            ViewData["ActiveTab"] = "events";
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var societyIdClaim = User.FindFirstValue("SocietyId");

            if (!int.TryParse(userIdClaim, out int userId)) return RedirectToAction("Index");
            if (!int.TryParse(societyIdClaim, out int societyId)) societyId = 1; // Fallback

            ViewBag.Facilities = await _context.Facilities
                .Where(f => f.SocietyId == societyId)
                .OrderBy(f => f.Name)
                .ToListAsync();

            ViewBag.MyBookings = await _context.Bookings
                .Include(b => b.Facility)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.Date)
                .ToListAsync();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> BookEvent(int facilityId, DateTime date, int days, string purpose)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out int userId)) return Unauthorized();

            var facility = await _context.Facilities.FindAsync(facilityId);
            if (facility == null) return NotFound();

            var newBooking = new Booking
            {
                FacilityId = facilityId,
                UserId = userId,
                Date = date,
                Days = days,
                TotalPrice = facility.PricePerDay * days,
                Purpose = purpose,
                Status = "pending"
            };

            _context.Bookings.Add(newBooking);
            await _context.SaveChangesAsync();

            return RedirectToAction("Events");
        }
    }
}
