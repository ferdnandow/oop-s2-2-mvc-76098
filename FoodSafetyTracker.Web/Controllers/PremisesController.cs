using FoodSafetyTracker.Web.Data;
using FoodSafetyTracker.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodSafetyTracker.Web.Controllers
{
    [Authorize]
    public class PremisesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PremisesController> _logger;

        public PremisesController(ApplicationDbContext context, ILogger<PremisesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Premises
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Premises list accessed by {User}", User.Identity!.Name);
            return View(await _context.Premises.ToListAsync());
        }

        // GET: Premises/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Premises Details called with null id");
                return NotFound();
            }

            var premises = await _context.Premises
                .Include(p => p.Inspections)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (premises == null)
            {
                _logger.LogWarning("Premises with id {Id} not found", id);
                return NotFound();
            }

            return View(premises);
        }

        // GET: Premises/Create
        [Authorize(Roles = "Admin,Inspector")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Premises/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Inspector")]
        public async Task<IActionResult> Create([Bind("Id,Name,Address,Town,RiskRating")] Premises premises)
        {
            if (ModelState.IsValid)
            {
                _context.Add(premises);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Premises created: {Name} in {Town} by {User}", premises.Name, premises.Town, User.Identity!.Name);
                return RedirectToAction(nameof(Index));
            }
            return View(premises);
        }

        // GET: Premises/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var premises = await _context.Premises.FindAsync(id);
            if (premises == null) return NotFound();

            return View(premises);
        }

        // POST: Premises/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Address,Town,RiskRating")] Premises premises)
        {
            if (id != premises.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(premises);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Premises updated: {Id} - {Name} by {User}", premises.Id, premises.Name, User.Identity!.Name);
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    _logger.LogError(ex, "Error updating Premises {Id}", id);
                    if (!PremisesExists(premises.Id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(premises);
        }

        // GET: Premises/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var premises = await _context.Premises.FirstOrDefaultAsync(m => m.Id == id);
            if (premises == null) return NotFound();

            return View(premises);
        }

        // POST: Premises/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var premises = await _context.Premises.FindAsync(id);
            if (premises != null)
            {
                _context.Premises.Remove(premises);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Premises deleted: {Id} by {User}", id, User.Identity!.Name);
            }
            return RedirectToAction(nameof(Index));
        }

        private bool PremisesExists(int id)
        {
            return _context.Premises.Any(e => e.Id == id);
        }
    }
}