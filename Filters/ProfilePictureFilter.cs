using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SmartSocietyMVC.Data;
using System.Security.Claims;

namespace SmartSocietyMVC.Filters
{
    /// <summary>
    /// Injects the logged-in user's profile picture path into ViewBag.ProfilePicture
    /// for every authenticated page, so the TopBar can display it.
    /// </summary>
    public class ProfilePictureFilter : IAsyncActionFilter
    {
        private readonly ApplicationDbContext _context;

        public ProfilePictureFilter(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context.Controller is Controller controller && controller.User.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = controller.User.Claims
                    .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

                if (int.TryParse(userIdClaim, out int userId))
                {
                    var user = await _context.Users.FindAsync(userId);
                    if (user != null)
                    {
                        controller.ViewBag.ProfilePicture = user.ProfilePicture;
                    }
                }
            }

            await next();
        }
    }
}
