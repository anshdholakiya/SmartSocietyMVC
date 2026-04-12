using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartSocietyMVC.Data;
using System.Security.Claims;

namespace SmartSocietyMVC.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        private int GetUserId()
        {
            var idClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(idClaim, out int userId)) return userId;
            return 0;
        }

        private string GetUserRole()
        {
            return User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value ?? "resident";
        }

        public async Task<IActionResult> Index()
        {
            ViewData["ActiveTab"] = "dashboard";
            var userId = GetUserId();
            var role = GetUserRole();

            dynamic stats;

            if (role == "admin")
            {
                stats = new
                {
                    TotalCollected = await _context.Bills.Where(b => b.Status == "paid").SumAsync(b => b.Amount),
                    TotalPending = await _context.Bills.Where(b => b.Status == "pending").SumAsync(b => b.Amount),
                    TotalResidents = await _context.Users.CountAsync(),
                    ActiveIssues = await _context.Complaints.CountAsync(c => c.Status != "resolved")
                };
            }
            else
            {
                stats = new
                {
                    MyBalance = await _context.Bills.Where(b => b.UserId == userId && b.Status == "pending").SumAsync(b => b.Amount),
                    ActiveIssues = await _context.Complaints.CountAsync(c => c.UserId == userId && c.Status != "resolved"),
                    TotalSpent = await _context.Bills.Where(b => b.UserId == userId && b.Status == "paid").SumAsync(b => b.Amount)
                };
            }

            ViewBag.Stats = stats;
            return View();
        }
    }
}
