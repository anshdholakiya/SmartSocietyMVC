using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartSocietyMVC.Data;
using SmartSocietyMVC.Models;
using System.Security.Claims;

namespace SmartSocietyMVC.Controllers
{
    [Authorize]
    public class NoticesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NoticesController(ApplicationDbContext context)
        {
            _context = context;
        }

        private int GetSocietyId()
        {
            var claim = User.Claims.FirstOrDefault(c => c.Type == "SocietyId")?.Value;
            if (int.TryParse(claim, out int societyId)) return societyId;
            return 0; // Or handle error
        }

        public async Task<IActionResult> Index()
        {
            ViewData["ActiveTab"] = "notices";
            var societyId = GetSocietyId();
            var notices = await _context.Notices
                .Where(n => n.SocietyId == societyId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            ViewBag.Notices = notices;
            return View("~/Views/Notices/Index.cshtml");
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Create([Bind("Title,Description,Type")] Notice notice)
        {
            if (ModelState.IsValid)
            {
                notice.SocietyId = GetSocietyId();
                notice.CreatedAt = DateTime.UtcNow;
                _context.Add(notice);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var notice = await _context.Notices.FindAsync(id);
            if (notice != null && notice.SocietyId == GetSocietyId())
            {
                _context.Notices.Remove(notice);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
