using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSocietyMVC.Data;
using SmartSocietyMVC.Models;
using System.Linq;
using System.Threading.Tasks;

namespace SmartSocietyMVC.Controllers
{
    [Authorize(Roles = "admin")]
    public class InviteController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly SmartSocietyMVC.Services.IEmailSender _emailSender;

        public InviteController(ApplicationDbContext context, SmartSocietyMVC.Services.IEmailSender emailSender)
        {
            _context = context;
            _emailSender = emailSender;
        }

        public IActionResult Index()
        {
            ViewData["ActiveTab"] = "invite";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendInvite(string residentName, string emailAddress, string wing, string flatNo)
        {
            var societyIdClaim = User.Claims.FirstOrDefault(c => c.Type == "SocietyId")?.Value;
            if (string.IsNullOrEmpty(societyIdClaim) || !int.TryParse(societyIdClaim, out int societyId))
            {
                TempData["ErrorMessage"] = "Could not identify your society.";
                return RedirectToAction("Index");
            }

            var existingUser = _context.Users.FirstOrDefault(u => u.Email.ToLower() == emailAddress.ToLower());
            if (existingUser != null)
            {
                TempData["ErrorMessage"] = "User with this email already exists.";
                return RedirectToAction("Index");
            }

            var newUser = new User
            {
                Name = residentName,
                Email = emailAddress,
                Role = "resident",
                Wing = wing,
                FlatNumber = flatNo,
                SocietyId = societyId,
                PasswordHash = "", // Can be empty until setup
                IsSetup = false
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            // Send actual email using MailKit
            var request = HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";
            var setupLink = $"{baseUrl}/Account/SetupPassword?token={emailAddress}";
            
            var subject = "You're Invited to Smart Society!";
            var htmlMessage = $@"
                <div style='font-family: Arial, sans-serif; padding: 20px; background-color: #f8fafc; border-radius: 10px;'>
                    <h2 style='color: #1e293b;'>Welcome to Smart Society Smart Society</h2>
                    <p style='color: #475569; font-size: 16px;'>Hello {residentName},</p>
                    <p style='color: #475569; font-size: 16px;'>You have been invited by the administrator to join the Smart Society Management Portal.</p>
                    <p style='color: #475569; font-size: 16px;'>Please click the button below to set up your password and access your account.</p>
                    <div style='text-align: center; margin: 30px 0;'>
                        <a href='{setupLink}' style='background-color: #2563eb; color: white; padding: 12px 24px; text-decoration: none; border-radius: 6px; font-weight: bold; font-size: 16px;'>Set Up My Password</a>
                    </div>
                    <p style='color: #94a3b8; font-size: 14px;'>If the button doesn't work, copy and paste this link into your browser:<br>{setupLink}</p>
                </div>";

            try
            {
                await _emailSender.SendEmailAsync(emailAddress, subject, htmlMessage);
                TempData["SuccessMessage"] = $"Invitation successful! An email has been sent to {emailAddress} with setup instructions.";
            }
            catch (Exception ex)
            {
                TempData["SuccessMessage"] = $"Invitation saved, but we failed to send the email due to an SMTP error. They can still use the setup link manually. Error: {ex.Message}";
            }

            return RedirectToAction("Index");
        }
    }
}

