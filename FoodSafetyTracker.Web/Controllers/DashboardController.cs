using FoodSafetyTracker.Web.Data;
using FoodSafetyTracker.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FoodSafetyTracker.Web.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(ApplicationDbContext context, ILogger<DashboardController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string? town, RiskRating? riskRating)
        {
            _logger.LogInformation("Dashboard accessed by {User} with filters Town={Town}, RiskRating={RiskRating}",
                User.Identity!.Name, town, riskRating);

            var now = DateTime.Now;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);

            // Base query for inspections
            var inspectionsQuery = _context.Inspections
                .Include(i => i.Premises)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(town))
                inspectionsQuery = inspectionsQuery.Where(i => i.Premises.Town == town);

            if (riskRating.HasValue)
                inspectionsQuery = inspectionsQuery.Where(i => i.Premises.RiskRating == riskRating);

            // Dashboard counts
            var inspectionsThisMonth = await inspectionsQuery
                .Where(i => i.InspectionDate >= startOfMonth)
                .CountAsync();

            var failedInspectionsThisMonth = await inspectionsQuery
                .Where(i => i.InspectionDate >= startOfMonth && i.Outcome == Outcome.Fail)
                .CountAsync();

            // Overdue follow-ups query
            var followUpsQuery = _context.FollowUps
                .Include(f => f.Inspection)
                .ThenInclude(i => i.Premises)
                .AsQueryable();

            if (!string.IsNullOrEmpty(town))
                followUpsQuery = followUpsQuery.Where(f => f.Inspection.Premises.Town == town);

            if (riskRating.HasValue)
                followUpsQuery = followUpsQuery.Where(f => f.Inspection.Premises.RiskRating == riskRating);

            var overdueFollowUps = await followUpsQuery
                .Where(f => f.DueDate < now && f.Status == FollowUpStatus.Open)
                .CountAsync();

            // Filter options
            var towns = await _context.Premises
                .Select(p => p.Town)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();

            ViewBag.Towns = new SelectList(towns);
            ViewBag.SelectedTown = town;
            ViewBag.SelectedRiskRating = riskRating;
            ViewBag.InspectionsThisMonth = inspectionsThisMonth;
            ViewBag.FailedInspectionsThisMonth = failedInspectionsThisMonth;
            ViewBag.OverdueFollowUps = overdueFollowUps;

            _logger.LogInformation("Dashboard stats: InspectionsThisMonth={Count1}, Failed={Count2}, OverdueFollowUps={Count3}",
                inspectionsThisMonth, failedInspectionsThisMonth, overdueFollowUps);

            return View();
        }
    }
}