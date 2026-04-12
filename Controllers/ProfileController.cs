using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSocietyMVC.Data;
using System.Security.Claims;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace SmartSocietyMVC.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;

        public ProfileController(ApplicationDbContext context, IWebHostEnvironment env, IConfiguration config)
        {
            _context = context;
            _env = env;
            _config = config;
        }

        private int GetUserId()
        {
            var claim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(claim, out int userId)) return userId;
            return 0;
        }

        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return RedirectToAction("Login", "Account");
            ViewData["ActiveTab"] = "profile";
            return View("~/Views/Profile/Index.cshtml", user);
        }

        [HttpPost]
        public async Task<IActionResult> Update(string name, string profession, string wing, string flatNumber, IFormFile? profilePicture)
        {
            var userId = GetUserId();
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return RedirectToAction(nameof(Index));

            user.Name = name ?? user.Name;
            user.Profession = profession;
            user.Wing = wing;
            user.FlatNumber = flatNumber;

            if (profilePicture != null && profilePicture.Length > 0)
            {
                var account = new Account(
                    _config["Cloudinary:CloudName"],
                    _config["Cloudinary:ApiKey"],
                    _config["Cloudinary:ApiSecret"]
                );
                var cloudinary = new Cloudinary(account);

                using var stream = profilePicture.OpenReadStream();
                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(profilePicture.FileName, stream),
                    Folder = "smartsociety/profiles"
                };

                var uploadResult = await cloudinary.UploadAsync(uploadParams);
                user.ProfilePicture = uploadResult.SecureUrl.ToString();
            }

            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Profile updated successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}
