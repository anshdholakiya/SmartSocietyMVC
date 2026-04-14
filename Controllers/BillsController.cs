using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartSocietyMVC.Data;
using SmartSocietyMVC.Models;
using System.Security.Claims;

namespace SmartSocietyMVC.Controllers
{
    [Authorize]
    public class BillsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BillsController(ApplicationDbContext context)
        {
            _context = context;
        }

        private int GetUserId()
        {
            var idClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(idClaim, out int userId)) return userId;
            return 0;
        }

        private int GetSocietyId()
        {
            var claim = User.Claims.FirstOrDefault(c => c.Type == "SocietyId")?.Value;
            if (int.TryParse(claim, out int societyId)) return societyId;
            return 0;
        }

        private string GetUserRole()
        {
            return User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value ?? "resident";
        }

        public async Task<IActionResult> Index()
        {
            ViewData["ActiveTab"] = "bills";
            var role = GetUserRole();
            var societyId = GetSocietyId();
            var userId = GetUserId();

            if (role == "admin")
            {
                var bills = await _context.Bills
                    .Include(b => b.User)
                    .Where(b => b.User != null && b.User.SocietyId == societyId)
                    .OrderByDescending(b => b.DueDate)
                    .ToListAsync();
                ViewBag.Bills = bills;

                var residents = await _context.Users
                    .Where(u => u.SocietyId == societyId && u.Role == "resident")
                    .OrderBy(u => u.Name)
                    .ToListAsync();
                ViewBag.Residents = residents;
            }
            else
            {
                var bills = await _context.Bills
                    .Where(b => b.UserId == userId)
                    .OrderByDescending(b => b.DueDate)
                    .ToListAsync();
                ViewBag.Bills = bills;
            }

            return View("~/Views/Bills/Index.cshtml");
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Create([Bind("UserId,Amount,Month,DueDate")] Bill bill)
        {
            if (ModelState.IsValid)
            {
                bill.Status = "pending";
                _context.Add(bill);
                await _context.SaveChangesAsync();
                TempData["BillSuccess"] = "Bill generated successfully!";
            }
            else
            {
                ViewBag.Error = "Invalid bill data.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> MarkPaid(int id)
        {
            var bill = await _context.Bills.Include(b => b.User).FirstOrDefaultAsync(b => b.Id == id);
            if (bill != null && bill.User?.SocietyId == GetSocietyId())
            {
                bill.Status = "paid";
                await _context.SaveChangesAsync();
                TempData["BillSuccess"] = "Payment recorded successfully.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
