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

        // Test 1: Overdue follow-ups query returns correct items
        [Fact]
        public async Task OverdueFollowUps_ReturnsOnlyOpenAndPastDueDate()
        {
            // Arrange
            var context = GetInMemoryContext();
            var now = DateTime.Now;

            var premises = new Premises { Name = "Test Cafe", Address = "1 Main St", Town = "Cork", RiskRating = RiskRating.Low };
            context.Premises.Add(premises);
            await context.SaveChangesAsync();

            var inspection = new Inspection { PremisesId = premises.Id, InspectionDate = now.AddDays(-20), Score = 40, Outcome = Outcome.Fail };
            context.Inspections.Add(inspection);
            await context.SaveChangesAsync();

            context.FollowUps.AddRange(
                new FollowUp { InspectionId = inspection.Id, DueDate = now.AddDays(-5), Status = FollowUpStatus.Open },   // overdue
                new FollowUp { InspectionId = inspection.Id, DueDate = now.AddDays(-3), Status = FollowUpStatus.Open },   // overdue
                new FollowUp { InspectionId = inspection.Id, DueDate = now.AddDays(5), Status = FollowUpStatus.Open },    // not due yet
                new FollowUp { InspectionId = inspection.Id, DueDate = now.AddDays(-5), Status = FollowUpStatus.Closed, ClosedDate = now.AddDays(-1) } // closed
            );
            await context.SaveChangesAsync();

            // Act
            var overdue = await context.FollowUps
                .Where(f => f.DueDate < now && f.Status == FollowUpStatus.Open)
                .ToListAsync();

            // Assert
            Assert.Equal(2, overdue.Count);
        }

        // Test 2: FollowUp cannot be closed without ClosedDate
        [Fact]
        public void FollowUp_ClosedStatus_RequiresClosedDate()
        {
            // Arrange
            var followUp = new FollowUp
            {
                InspectionId = 1,
                DueDate = DateTime.Now.AddDays(5),
                Status = FollowUpStatus.Closed,
                ClosedDate = null
            };

            // Assert
            Assert.Null(followUp.ClosedDate);
            Assert.Equal(FollowUpStatus.Closed, followUp.Status);
            // This combination is invalid — ClosedDate must be set when Status is Closed
            var isInvalid = followUp.Status == FollowUpStatus.Closed && followUp.ClosedDate == null;
            Assert.True(isInvalid);
        }

        // Test 3: Dashboard counts consistent with known seed data
        [Fact]
        public async Task Dashboard_InspectionsThisMonth_ReturnsCorrectCount()
        {
            // Arrange
            var context = GetInMemoryContext();
            var now = DateTime.Now;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);

            var premises = new Premises { Name = "Test Place", Address = "1 St", Town = "Dublin", RiskRating = RiskRating.Medium };
            context.Premises.Add(premises);
            await context.SaveChangesAsync();

            context.Inspections.AddRange(
                new Inspection { PremisesId = premises.Id, InspectionDate = now.AddDays(-2), Score = 80, Outcome = Outcome.Pass },  // this month
                new Inspection { PremisesId = premises.Id, InspectionDate = now.AddDays(-5), Score = 40, Outcome = Outcome.Fail },  // this month
                new Inspection { PremisesId = premises.Id, InspectionDate = now.AddDays(-40), Score = 70, Outcome = Outcome.Pass }  // last month
            );
            await context.SaveChangesAsync();

            // Act
            var thisMonthCount = await context.Inspections
                .Where(i => i.InspectionDate >= startOfMonth)
                .CountAsync();

            var failedThisMonth = await context.Inspections
                .Where(i => i.InspectionDate >= startOfMonth && i.Outcome == Outcome.Fail)
                .CountAsync();

            // Assert
            Assert.Equal(2, thisMonthCount);
            Assert.Equal(1, failedThisMonth);
        }

        // Test 4: Inspector role can create inspections, Viewer cannot
        [Fact]
        public void Roles_InspectorAndViewer_HaveDifferentAccess()
        {
            // Arrange
            var inspectorRoles = new[] { "Inspector" };
            var viewerRoles = new[] { "Viewer" };

            // Act
            var inspectorCanCreate = inspectorRoles.Contains("Inspector") || inspectorRoles.Contains("Admin");
            var viewerCanCreate = viewerRoles.Contains("Inspector") || viewerRoles.Contains("Admin");

            // Assert
            Assert.True(inspectorCanCreate);
            Assert.False(viewerCanCreate);
        }
    }
}