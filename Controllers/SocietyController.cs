using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SmartSocietyMVC.Controllers
{
    [Authorize(Roles = "admin,resident")]
    public class SocietyController : Controller
    {
        public IActionResult Index()
        {
            ViewData["ActiveTab"] = "society";
            return View();
        }
    }
}
