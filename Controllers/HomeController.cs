using Microsoft.AspNetCore.Mvc;

namespace SmartSocietyMVC.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            // If the user happens to be authenticated and goes to the root "/", 
            // redirect them straight to their dashboard.
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Dashboard");
            }
            
            // Otherwise, show the landing page
            return View();
        }
    }
}
