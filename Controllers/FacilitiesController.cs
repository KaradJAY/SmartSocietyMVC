using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SmartSocietyMVC.Controllers
{
    [Authorize(Roles = "admin")]
    public class FacilitiesController : Controller
    {
        public IActionResult Index()
        {
            ViewData["ActiveTab"] = "facilities";
            return View();
        }
    }
}
