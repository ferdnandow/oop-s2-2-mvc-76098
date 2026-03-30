using FoodSafetyTracker.Web.Data;
using FoodSafetyTracker.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FoodSafetyTracker.Web.Controllers
{
    [Authorize]
    public class FollowUpController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FollowUpController> _logger;

        public FollowUpController(ApplicationDbContext context, ILogger<FollowUpController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("FollowUp list accessed by {User}", User.Identity!.Name);
            var followUps = await _context.FollowUps
                .Include(f => f.Inspection)
                .ThenInclude(i => i.Premises)
                .OrderByDescending(f => f.DueDate)
                .ToListAsync();
            return View(followUps);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var followUp = await _context.FollowUps
                .Include(f => f.Inspection)
                .ThenInclude(i => i.Premises)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (followUp == null) return NotFound();

            return View(followUp);
        }

        [Authorize(Roles = "Admin,Inspector")]
        public IActionResult Create()
        {
            ViewData["InspectionId"] = new SelectList(
                _context.Inspections.Include(i => i.Premises)
                    .Select(i => new { i.Id, Display = i.Premises.Name + " - " + i.InspectionDate.ToShortDateString() }),
                "Id", "Display");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Inspector")]
        public async Task<IActionResult> Create([Bind("Id,InspectionId,DueDate,Status,ClosedDate")] FollowUp followUp)
        {
            if (followUp.Status == FollowUpStatus.Closed && followUp.ClosedDate == null)
            {
                _logger.LogWarning("FollowUp creation failed: ClosedDate missing for InspectionId {InspectionId}", followUp.InspectionId);
                ModelState.AddModelError("ClosedDate", "ClosedDate is required when Status is Closed.");
            }

            var inspection = await _context.Inspections.FindAsync(followUp.InspectionId);
            if (inspection != null && followUp.DueDate < inspection.InspectionDate)
            {
                _logger.LogWarning("FollowUp creation failed: DueDate before InspectionDate for InspectionId {InspectionId}", followUp.InspectionId);
                ModelState.AddModelError("DueDate", "DueDate cannot be before the Inspection date.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(followUp);
                await _context.SaveChangesAsync();
                _logger.LogInformation("FollowUp created for InspectionId {InspectionId} by {User}", followUp.InspectionId, User.Identity!.Name);
                return RedirectToAction(nameof(Index));
            }

            ViewData["InspectionId"] = new SelectList(
                _context.Inspections.Include(i => i.Premises)
                    .Select(i => new { i.Id, Display = i.Premises.Name + " - " + i.InspectionDate.ToShortDateString() }),
                "Id", "Display", followUp.InspectionId);
            return View(followUp);
        }

        [Authorize(Roles = "Admin,Inspector")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var followUp = await _context.FollowUps.FindAsync(id);
            if (followUp == null) return NotFound();

            ViewData["InspectionId"] = new SelectList(
                _context.Inspections.Include(i => i.Premises)
                    .Select(i => new { i.Id, Display = i.Premises.Name + " - " + i.InspectionDate.ToShortDateString() }),
                "Id", "Display", followUp.InspectionId);
            return View(followUp);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Inspector")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,InspectionId,DueDate,Status,ClosedDate")] FollowUp followUp)
        {
            if (id != followUp.Id) return NotFound();

            if (followUp.Status == FollowUpStatus.Closed && followUp.ClosedDate == null)
            {
                _logger.LogWarning("FollowUp edit failed: ClosedDate missing for FollowUpId {Id}", followUp.Id);
                ModelState.AddModelError("ClosedDate", "ClosedDate is required when Status is Closed.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(followUp);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("FollowUp updated: {Id} by {User}", followUp.Id, User.Identity!.Name);
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    _logger.LogError(ex, "Error updating FollowUp {Id}", id);
                    if (!FollowUpExists(followUp.Id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["InspectionId"] = new SelectList(
                _context.Inspections.Include(i => i.Premises)
                    .Select(i => new { i.Id, Display = i.Premises.Name + " - " + i.InspectionDate.ToShortDateString() }),
                "Id", "Display", followUp.InspectionId);
            return View(followUp);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var followUp = await _context.FollowUps
                .Include(f => f.Inspection)
                .ThenInclude(i => i.Premises)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (followUp == null) return NotFound();

            return View(followUp);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var followUp = await _context.FollowUps.FindAsync(id);
            if (followUp != null)
            {
                _context.FollowUps.Remove(followUp);
                await _context.SaveChangesAsync();
                _logger.LogInformation("FollowUp deleted: {Id} by {User}", id, User.Identity!.Name);
            }
            return RedirectToAction(nameof(Index));
        }

        private bool FollowUpExists(int id)
        {
            return _context.FollowUps.Any(e => e.Id == id);
        }
    }
}