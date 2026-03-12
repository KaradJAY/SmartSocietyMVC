using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SmartSocietyMVC.Controllers
{
    [Authorize(Roles = "resident")]
    public class EventsController : Controller
    {
        public IActionResult Index()
        {
            ViewData["ActiveTab"] = "events";
            return View();
        }
    }
}
