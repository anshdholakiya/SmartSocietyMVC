using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartSocietyMVC.Data;
using System.Security.Claims;

namespace SmartSocietyMVC.Controllers
{
    [Authorize]
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
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
            ViewData["ActiveTab"] = "residents";
            var societyId = GetSocietyId();

            var users = await _context.Users
                .Where(u => u.SocietyId == societyId)
                .OrderBy(u => u.Name)
                .ToListAsync();

            ViewBag.Users = users;
            return View("~/Views/Users/Index.cshtml");
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> ToggleRole(int id)
        {
            var userToModify = await _context.Users.FindAsync(id);
            var currentUserIdStr = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            
            if (userToModify != null && userToModify.SocietyId == GetSocietyId() && currentUserIdStr != id.ToString())
            {
                userToModify.Role = userToModify.Role == "admin" ? "resident" : "admin";
                await _context.SaveChangesAsync();
                TempData["UserSuccess"] = "User role updated.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userToDelete = await _context.Users.FindAsync(id);
            var currentUserIdStr = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (userToDelete != null && userToDelete.SocietyId == GetSocietyId() && currentUserIdStr != id.ToString())
            {
                _context.Users.Remove(userToDelete);
                await _context.SaveChangesAsync();
                TempData["UserSuccess"] = "User removed from society.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
