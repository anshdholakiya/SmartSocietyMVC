using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartSocietyMVC.Data;
using SmartSocietyMVC.Models;
using System.Security.Claims;

namespace SmartSocietyMVC.Controllers
{
    [Authorize]
    public class EventsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EventsController(ApplicationDbContext context)
        {
            _context = context;
        }

        private int GetUserId()
        {
            var claim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(claim, out int id)) return id;
            return 0;
        }

        private int GetSocietyId()
        {
            var claim = User.Claims.FirstOrDefault(c => c.Type == "SocietyId")?.Value;
            if (int.TryParse(claim, out int societyId)) return societyId;
            return 0;
        }

        // GET: Events page — loads events, facilities, and bookings all in one
        public async Task<IActionResult> Index()
        {
            ViewData["ActiveTab"] = "events";
            var societyId = GetSocietyId();
            var userId = GetUserId();
            var isAdmin = User.IsInRole("admin");

            // Always load events and facilities
            var events = await _context.Events
                .Where(e => e.SocietyId == societyId)
                .OrderBy(e => e.EventDate)
                .ToListAsync();

            var facilities = await _context.Facilities
                .Where(f => f.SocietyId == societyId && f.Status == "Available")
                .ToListAsync();

            ViewBag.Events = events;
            ViewBag.Facilities = facilities;

            if (isAdmin)
            {
                // Admin sees ALL bookings for the society
                var allBookings = await _context.Bookings
                    .Include(b => b.User)
                    .Include(b => b.Facility)
                    .Where(b => b.Facility != null && b.Facility.SocietyId == societyId)
                    .OrderByDescending(b => b.Date)
                    .ToListAsync();
                ViewBag.AllBookings = allBookings;
            }
            else
            {
                // Resident sees only their own bookings
                var myBookings = await _context.Bookings
                    .Include(b => b.Facility)
                    .Where(b => b.UserId == userId)
                    .OrderByDescending(b => b.Date)
                    .ToListAsync();
                ViewBag.MyBookings = myBookings;
            }

            return View("~/Views/Events/Index.cshtml");
        }

        // POST: Admin creates an event
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Create(string title, string description, DateTime eventDate, string location, string organizer)
        {
            var societyId = GetSocietyId();
            if (societyId == 0) return RedirectToAction(nameof(Index));

            var ev = new Event
            {
                Title = title,
                Description = description,
                EventDate = eventDate,
                Location = location,
                Organizer = organizer,
                SocietyId = societyId
            };
            _context.Events.Add(ev);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Event created successfully!";
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin deletes an event
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev != null && ev.SocietyId == GetSocietyId())
            {
                _context.Events.Remove(ev);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: Resident requests a booking
        [HttpPost]
        [Authorize(Roles = "resident")]
        public async Task<IActionResult> RequestBooking(int facilityId, DateTime date, string purpose, int days)
        {
            var userId = GetUserId();
            if (userId == 0) return RedirectToAction(nameof(Index));

            var facility = await _context.Facilities.FindAsync(facilityId);
            if (facility == null) return RedirectToAction(nameof(Index));

            var booking = new Booking
            {
                UserId = userId,
                FacilityId = facilityId,
                Date = date,
                Purpose = purpose,
                Days = days > 0 ? days : 1,
                TotalPrice = facility.PricePerDay * (days > 0 ? days : 1),
                Status = "pending"
            };
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Booking request submitted! Awaiting admin approval.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin approves or rejects a booking
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateBookingStatus(int id, string status, string? rejectReason)
        {
            var booking = await _context.Bookings
                .Include(b => b.Facility)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking != null && booking.Facility?.SocietyId == GetSocietyId())
            {
                booking.Status = status;
                if (status == "rejected" && !string.IsNullOrEmpty(rejectReason))
                {
                    booking.RejectReason = rejectReason;
                }
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
