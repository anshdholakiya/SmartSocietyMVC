using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartSocietyMVC.Data;
using System.Security.Claims;

using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace SmartSocietyMVC.Controllers
{
    [Authorize]
    public class SocietyController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;

        public SocietyController(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        private int GetSocietyId()
        {
            var claim = User.Claims.FirstOrDefault(c => c.Type == "SocietyId")?.Value;
            if (int.TryParse(claim, out int societyId)) return societyId;
            return 0;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["ActiveTab"] = "society";
            var societyId = GetSocietyId();
            var society = await _context.Societies.FindAsync(societyId);

            var amenitiesList = System.Text.Json.JsonSerializer.Deserialize<List<string>>(society.Amenities ?? "[]");
            if (amenitiesList == null || !amenitiesList.Any())
            {
                amenitiesList = new List<string> { "Swimming Pool", "Gymnasium", "Club House", "24/7 Security" };
            }
            ViewBag.FacilityNames = amenitiesList;

            var galleryList = System.Text.Json.JsonSerializer.Deserialize<List<string>>(society.Gallery ?? "[]");
            ViewBag.GalleryImages = galleryList ?? new List<string>();

            return View("~/Views/Society/Index.cshtml", society);
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Update(string name, string address, string contactNumber, string amenities)
        {
            var societyId = GetSocietyId();
            var society = await _context.Societies.FindAsync(societyId);
            if (society == null) return RedirectToAction(nameof(Index));

            society.Name = name ?? society.Name;
            society.Address = address;
            society.ContactNumber = contactNumber;

            if (!string.IsNullOrEmpty(amenities))
            {
                var amList = amenities.Split(',').Select(a => a.Trim()).Where(a => !string.IsNullOrEmpty(a)).ToList();
                society.Amenities = System.Text.Json.JsonSerializer.Serialize(amList);
            }

            _context.Societies.Update(society);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Society details updated!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> AddGalleryImage(IFormFile ImageFile)
        {
            var societyId = GetSocietyId();
            var society = await _context.Societies.FindAsync(societyId);
            if (society == null || ImageFile == null || ImageFile.Length == 0) return RedirectToAction(nameof(Index));

            var account = new Account(
                _config["Cloudinary:CloudName"],
                _config["Cloudinary:ApiKey"],
                _config["Cloudinary:ApiSecret"]
            );
            var cloudinary = new Cloudinary(account);

            using var stream = ImageFile.OpenReadStream();
            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(ImageFile.FileName, stream),
                Folder = "smartsociety/gallery"
            };

            var uploadResult = await cloudinary.UploadAsync(uploadParams);
            
            var galleryImages = System.Text.Json.JsonSerializer.Deserialize<List<string>>(society.Gallery ?? "[]") ?? new List<string>();
            galleryImages.Add(uploadResult.SecureUrl.ToString());
            society.Gallery = System.Text.Json.JsonSerializer.Serialize(galleryImages);
            
            _context.Societies.Update(society);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Gallery image added successfully!";
            
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteGalleryImage(string imageUrl)
        {
            var societyId = GetSocietyId();
            var society = await _context.Societies.FindAsync(societyId);
            if (society == null || string.IsNullOrEmpty(imageUrl)) return RedirectToAction(nameof(Index));

            // Remove from gallery list
            var galleryImages = System.Text.Json.JsonSerializer.Deserialize<List<string>>(society.Gallery ?? "[]") ?? new List<string>();
            galleryImages.Remove(imageUrl);
            society.Gallery = System.Text.Json.JsonSerializer.Serialize(galleryImages);

            _context.Societies.Update(society);
            await _context.SaveChangesAsync();

            // Delete from Cloudinary
            try
            {
                var account = new Account(
                    _config["Cloudinary:CloudName"],
                    _config["Cloudinary:ApiKey"],
                    _config["Cloudinary:ApiSecret"]
                );
                var cloudinary = new Cloudinary(account);

                // Extract public_id from URL (e.g. smartsociety/gallery/abc123)
                var uri = new Uri(imageUrl);
                var segments = uri.AbsolutePath.Split('/');
                // The public_id is everything after /upload/vXXXXXX/ 
                var uploadIndex = Array.IndexOf(segments, "upload");
                if (uploadIndex >= 0 && uploadIndex + 2 < segments.Length)
                {
                    // Skip the version segment (vXXXXXX)
                    var publicIdParts = segments.Skip(uploadIndex + 2).ToArray();
                    var publicId = string.Join("/", publicIdParts);
                    // Remove file extension
                    var dotIndex = publicId.LastIndexOf('.');
                    if (dotIndex > 0) publicId = publicId[..dotIndex];

                    await cloudinary.DestroyAsync(new DeletionParams(publicId));
                }
            }
            catch { /* If Cloudinary deletion fails, we still removed from DB */ }

            TempData["SuccessMessage"] = "Gallery image deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}
