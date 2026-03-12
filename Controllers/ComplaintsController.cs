using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartSocietyMVC.Data;
using SmartSocietyMVC.Models;
using System.Security.Claims;

namespace SmartSocietyMVC.Controllers
{
    [Authorize]
    public class ComplaintsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ComplaintsController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        private int GetUserId()
        {
            var idClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(idClaim, out int userId)) return userId;
            return 0;
        }

        private int GetSocietyId()
        {
            var claim = User.Claims.FirstOrDefault(c => c.Type == "SocietyId")?.Value;
            if (int.TryParse(claim, out int societyId)) return societyId;
            return 0;
        }

        private string GetUserRole()
        {
            return User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value ?? "resident";
        }

        public async Task<IActionResult> Index()
        {
            ViewData["ActiveTab"] = "complaints";
            var role = GetUserRole();
            var societyId = GetSocietyId();
            var userId = GetUserId();

            IQueryable<Complaint> query = _context.Complaints
                .Include(c => c.User)
                .Where(c => c.SocietyId == societyId);

            if (role == "resident")
            {
                // Residents only see their own complaints
                query = query.Where(c => c.UserId == userId);
            }

            var complaints = await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
            ViewBag.Complaints = complaints;

            return View("~/Views/Complaints/Index.cshtml");
        }

        [HttpPost]
        [Authorize(Roles = "resident")]
        public async Task<IActionResult> Create(string Title, string Description, IFormFile ImageFile)
        {
            var userId = GetUserId();
            var societyId = GetSocietyId();

            if (userId == 0 || societyId == 0) return RedirectToAction(nameof(Index));

            var complaint = new Complaint
            {
                Title = Title,
                Description = Description,
                UserId = userId,
                SocietyId = societyId,
                Status = "pending",
                CreatedAt = DateTime.UtcNow
            };

            if (ImageFile != null && ImageFile.Length > 0)
            {
                // Store locally instead of cloudinary for now
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                Directory.CreateDirectory(uploadsFolder);
                
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + ImageFile.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(fileStream);
                }

                complaint.ImageUrl = "/uploads/" + uniqueFileName;
            }

            _context.Add(complaint);
            await _context.SaveChangesAsync();
            
            TempData["ComplaintSuccess"] = "Complaint filed successfully!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var complaint = await _context.Complaints.FindAsync(id);
            if (complaint != null && complaint.SocietyId == GetSocietyId())
            {
                complaint.Status = status;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var complaint = await _context.Complaints.FindAsync(id);
            if (complaint != null && complaint.SocietyId == GetSocietyId())
            {
                _context.Complaints.Remove(complaint);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
