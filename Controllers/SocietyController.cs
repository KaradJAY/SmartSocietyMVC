using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartSocietyMVC.Data;
using System.Security.Claims;

namespace SmartSocietyMVC.Controllers
{
    [Authorize]
    public class SocietyController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SocietyController(ApplicationDbContext context)
        {
            _context = context;
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

            // Pass real facility names for the Amenities section
            var facilityNames = await _context.Facilities
                .Where(f => f.SocietyId == societyId)
                .Select(f => f.Name)
                .ToListAsync();
            ViewBag.FacilityNames = facilityNames.Any() ? facilityNames : new List<string> { "Swimming Pool", "Gymnasium", "Club House", "24/7 Security" };

            return View("~/Views/Society/Index.cshtml", society);
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Update(string name, string address, string contactNumber)
        {
            var societyId = GetSocietyId();
            var society = await _context.Societies.FindAsync(societyId);
            if (society == null) return RedirectToAction(nameof(Index));

            society.Name = name ?? society.Name;
            society.Address = address;
            society.ContactNumber = contactNumber;

            _context.Societies.Update(society);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Society details updated!";
            return RedirectToAction(nameof(Index));
        }
    }
}
