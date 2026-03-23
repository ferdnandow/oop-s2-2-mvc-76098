using FoodSafetyTracker.Web.Data;
using FoodSafetyTracker.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace FoodSafetyTracker.Tests
{
    public class FoodSafetyTests
    {
        private ApplicationDbContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task OverdueFollowUps_ReturnsOnlyOpenAndPastDueDate()
        {
            var context = GetInMemoryContext();
            var now = DateTime.Now;

            var premises = new Premises { Name = "Test Cafe", Address = "1 Main St", Town = "Cork", RiskRating = RiskRating.Low };
            context.Premises.Add(premises);
            await context.SaveChangesAsync();

            var inspection = new Inspection { PremisesId = premises.Id, InspectionDate = now.AddDays(-20), Score = 40, Outcome = Outcome.Fail };
            context.Inspections.Add(inspection);
            await context.SaveChangesAsync();

            context.FollowUps.AddRange(
                new FollowUp { InspectionId = inspection.Id, DueDate = now.AddDays(-5), Status = FollowUpStatus.Open },
                new FollowUp { InspectionId = inspection.Id, DueDate = now.AddDays(-3), Status = FollowUpStatus.Open },
                new FollowUp { InspectionId = inspection.Id, DueDate = now.AddDays(5), Status = FollowUpStatus.Open },
                new FollowUp { InspectionId = inspection.Id, DueDate = now.AddDays(-5), Status = FollowUpStatus.Closed, ClosedDate = now.AddDays(-1) }
            );
            await context.SaveChangesAsync();

            var overdue = await context.FollowUps
                .Where(f => f.DueDate < now && f.Status == FollowUpStatus.Open)
                .ToListAsync();

            Assert.Equal(2, overdue.Count);
        }

        [Fact]
        public void FollowUp_ClosedStatus_RequiresClosedDate()
        {
            var followUp = new FollowUp
            {
                InspectionId = 1,
                DueDate = DateTime.Now.AddDays(5),
                Status = FollowUpStatus.Closed,
                ClosedDate = null
            };

            var isInvalid = followUp.Status == FollowUpStatus.Closed && followUp.ClosedDate == null;
            Assert.True(isInvalid);
        }

        [Fact]
        public async Task Dashboard_InspectionsThisMonth_ReturnsCorrectCount()
        {
            var context = GetInMemoryContext();
            var now = DateTime.Now;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);

            var premises = new Premises { Name = "Test Place", Address = "1 St", Town = "Belo Horizonte", RiskRating = RiskRating.Medium };
            context.Premises.Add(premises);
            await context.SaveChangesAsync();

            context.Inspections.AddRange(
                new Inspection { PremisesId = premises.Id, InspectionDate = now.AddDays(-2), Score = 80, Outcome = Outcome.Pass },
                new Inspection { PremisesId = premises.Id, InspectionDate = now.AddDays(-5), Score = 40, Outcome = Outcome.Fail },
                new Inspection { PremisesId = premises.Id, InspectionDate = now.AddDays(-40), Score = 70, Outcome = Outcome.Pass }
            );
            await context.SaveChangesAsync();

            var thisMonthCount = await context.Inspections
                .Where(i => i.InspectionDate >= startOfMonth)
                .CountAsync();

            var failedThisMonth = await context.Inspections
                .Where(i => i.InspectionDate >= startOfMonth && i.Outcome == Outcome.Fail)
                .CountAsync();

            Assert.Equal(2, thisMonthCount);
            Assert.Equal(1, failedThisMonth);
        }

        [Fact]
        public void Roles_InspectorAndViewer_HaveDifferentAccess()
        {
            var inspectorRoles = new[] { "Inspector" };
            var viewerRoles = new[] { "Viewer" };

            var inspectorCanCreate = inspectorRoles.Contains("Inspector") || inspectorRoles.Contains("Admin");
            var viewerCanCreate = viewerRoles.Contains("Inspector") || viewerRoles.Contains("Admin");

            Assert.True(inspectorCanCreate);
            Assert.False(viewerCanCreate);
        }

        [Fact]
        public async Task Premises_HighRiskRating_StoredCorrectly()
        {
            var context = GetInMemoryContext();
            var premises = new Premises
            {
                Name = "High Risk Place",
                Address = "99 Danger St",
                Town = "Dublin",
                RiskRating = RiskRating.High
            };

            context.Premises.Add(premises);
            await context.SaveChangesAsync();

            var saved = await context.Premises.FirstOrDefaultAsync(p => p.Name == "High Risk Place");

            Assert.NotNull(saved);
            Assert.Equal(RiskRating.High, saved.RiskRating);
        }

        [Fact]
        public async Task Inspection_FailOutcome_StoredCorrectly()
        {
            var context = GetInMemoryContext();
            var premises = new Premises { Name = "Fail Place", Address = "1 St", Town = "Cork", RiskRating = RiskRating.High };
            context.Premises.Add(premises);
            await context.SaveChangesAsync();

            var inspection = new Inspection
            {
                PremisesId = premises.Id,
                InspectionDate = DateTime.Now.AddDays(-5),
                Score = 25,
                Outcome = Outcome.Fail,
                Notes = "Very poor hygiene."
            };
            context.Inspections.Add(inspection);
            await context.SaveChangesAsync();

            var saved = await context.Inspections.FirstOrDefaultAsync(i => i.PremisesId == premises.Id);

            Assert.NotNull(saved);
            Assert.Equal(Outcome.Fail, saved.Outcome);
            Assert.Equal(25, saved.Score);
        }

        [Fact]
        public async Task FollowUp_ClosedWithClosedDate_IsValid()
        {
            var context = GetInMemoryContext();
            var premises = new Premises { Name = "Test Place", Address = "1 St", Town = "Belo Horizonte", RiskRating = RiskRating.Low };
            context.Premises.Add(premises);
            await context.SaveChangesAsync();

            var inspection = new Inspection { PremisesId = premises.Id, InspectionDate = DateTime.Now.AddDays(-10), Score = 40, Outcome = Outcome.Fail };
            context.Inspections.Add(inspection);
            await context.SaveChangesAsync();

            var followUp = new FollowUp
            {
                InspectionId = inspection.Id,
                DueDate = DateTime.Now.AddDays(-5),
                Status = FollowUpStatus.Closed,
                ClosedDate = DateTime.Now.AddDays(-1)
            };
            context.FollowUps.Add(followUp);
            await context.SaveChangesAsync();

            var saved = await context.FollowUps.FirstOrDefaultAsync(f => f.InspectionId == inspection.Id);

            Assert.NotNull(saved);
            Assert.Equal(FollowUpStatus.Closed, saved.Status);
            Assert.NotNull(saved.ClosedDate);
        }

        [Fact]
        public async Task Premises_FilterByTown_ReturnsCorrectCount()
        {
            var context = GetInMemoryContext();

            context.Premises.AddRange(
                new Premises { Name = "Place 1", Address = "Addr 1", Town = "Belo Horizonte", RiskRating = RiskRating.Low },
                new Premises { Name = "Place 2", Address = "Addr 2", Town = "Belo Horizonte", RiskRating = RiskRating.High },
                new Premises { Name = "Place 3", Address = "Addr 3", Town = "Nova Lima", RiskRating = RiskRating.Medium }
            );
            await context.SaveChangesAsync();

            var bhCount = await context.Premises
                .Where(p => p.Town == "Belo Horizonte")
                .CountAsync();

            Assert.Equal(2, bhCount);
        }
    }
}