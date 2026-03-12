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

        public InviteController(ApplicationDbContext context)
        {
            _context = context;
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

            var existingUser = _context.Users.FirstOrDefault(u => u.Email == emailAddress);
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

            TempData["SuccessMessage"] = $"Invitation successful! {residentName} can now set up their account using their email.";
            return RedirectToAction("Index");
        }
    }
}
