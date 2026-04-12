using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartSocietyMVC.Data;
using SmartSocietyMVC.Models;
using System.Security.Claims;

namespace SmartSocietyMVC.Controllers
{
    [Authorize]
    public class FacilitiesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FacilitiesController(ApplicationDbContext context)
        {
            _context = context;
        }

        private int GetSocietyId()
        {
            var claim = User.Claims.FirstOrDefault(c => c.Type == "SocietyId")?.Value;
            if (int.TryParse(claim, out int societyId)) return societyId;
            return 0;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["ActiveTab"] = "facilities";
            var societyId = GetSocietyId();
            var facilities = await _context.Facilities
                .Where(f => f.SocietyId == societyId)
                .ToListAsync();
            ViewBag.Facilities = facilities;
            return View("~/Views/Facilities/Index.cshtml");
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Create(string name, string description, int capacity, decimal pricePerDay, string operatingHours, string status)
        {
            var societyId = GetSocietyId();
            if (societyId == 0) return RedirectToAction(nameof(Index));

            var facility = new Facility
            {
                Name = name,
                Description = description,
                Capacity = capacity,
                PricePerDay = pricePerDay,
                OperatingHours = operatingHours,
                Status = status ?? "Available",
                SocietyId = societyId
            };
            _context.Facilities.Add(facility);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Facility added successfully!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Update(int id, string name, string description, int capacity, decimal pricePerDay, string operatingHours, string status)
        {
            var facility = await _context.Facilities.FindAsync(id);
            if (facility != null && facility.SocietyId == GetSocietyId())
            {
                facility.Name = name;
                facility.Description = description;
                facility.Capacity = capacity;
                facility.PricePerDay = pricePerDay;
                facility.OperatingHours = operatingHours;
                facility.Status = status;
                
                _context.Facilities.Update(facility);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Facility updated successfully!";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var facility = await _context.Facilities.FindAsync(id);
            if (facility != null && facility.SocietyId == GetSocietyId())
            {
                _context.Facilities.Remove(facility);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Facility removed.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
