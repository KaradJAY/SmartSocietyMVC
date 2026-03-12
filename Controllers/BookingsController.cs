using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SmartSocietyMVC.Controllers
{
    [Authorize(Roles = "admin")]
    public class BookingsController : Controller
    {
        public IActionResult Index()
        {
            ViewData["ActiveTab"] = "bookings";
            return View();
        }
    }
}
