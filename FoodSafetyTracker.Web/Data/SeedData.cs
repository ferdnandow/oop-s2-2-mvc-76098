using FoodSafetyTracker.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FoodSafetyTracker.Web.Data
{
    public static class SeedData
    {
        public static async Task InitialiseAsync(IServiceProvider serviceProvider)
        {
            using var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>());

            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            // Seed Roles
            string[] roles = { "Admin", "Inspector", "Viewer" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // Seed Admin user
            if (await userManager.FindByEmailAsync("assignment@dorset.ie") == null)
            {
                var admin = new IdentityUser { UserName = "assignment@dorset.ie", Email = "assignment@dorset.ie", EmailConfirmed = true };
                await userManager.CreateAsync(admin, "Dorset123!");
                await userManager.AddToRoleAsync(admin, "Admin");
            }

            // Seed Inspector user
            if (await userManager.FindByEmailAsync("inspector@dorset.ie") == null)
            {
                var inspector = new IdentityUser { UserName = "inspector@dorset.ie", Email = "inspector@dorset.ie", EmailConfirmed = true };
                await userManager.CreateAsync(inspector, "Dorset123!");
                await userManager.AddToRoleAsync(inspector, "Inspector");
            }

            // Seed Viewer user
            if (await userManager.FindByEmailAsync("viewer@dorset.ie") == null)
            {
                var viewer = new IdentityUser { UserName = "viewer@dorset.ie", Email = "viewer@dorset.ie", EmailConfirmed = true };
                await userManager.CreateAsync(viewer, "Dorset123!");
                await userManager.AddToRoleAsync(viewer, "Viewer");
            }

            // Seed Inspector user
            if (await userManager.FindByEmailAsync("inspector@food.ie") == null)
            {
                var inspector = new IdentityUser { UserName = "inspector@food.ie", Email = "inspector@food.ie", EmailConfirmed = true };
                await userManager.CreateAsync(inspector, "Inspector@1234");
                await userManager.AddToRoleAsync(inspector, "Inspector");
            }

            // Seed Viewer user
            if (await userManager.FindByEmailAsync("viewer@food.ie") == null)
            {
                var viewer = new IdentityUser { UserName = "viewer@food.ie", Email = "viewer@food.ie", EmailConfirmed = true };
                await userManager.CreateAsync(viewer, "Viewer@1234");
                await userManager.AddToRoleAsync(viewer, "Viewer");
            }

            // Seed Premises
            if (!context.Premises.Any())
            {
                var premises = new List<Premises>
                {
                    new Premises { Name = "The Cosy Kitchen", Address = "12 Main St", Town = "Cork", RiskRating = RiskRating.Low },
                    new Premises { Name = "Burger Barn", Address = "45 High St", Town = "Cork", RiskRating = RiskRating.Medium },
                    new Premises { Name = "Pizza Palace", Address = "78 Church Rd", Town = "Cork", RiskRating = RiskRating.High },
                    new Premises { Name = "The Hungry Fox", Address = "3 Bridge St", Town = "Cork", RiskRating = RiskRating.Low },
                    new Premises { Name = "Sushi Spot", Address = "22 River Rd", Town = "Dublin", RiskRating = RiskRating.Medium },
                    new Premises { Name = "The Noodle House", Address = "67 Grafton St", Town = "Dublin", RiskRating = RiskRating.High },
                    new Premises { Name = "Green Leaf Cafe", Address = "10 Baggot St", Town = "Dublin", RiskRating = RiskRating.Low },
                    new Premises { Name = "The Spice Garden", Address = "5 Camden St", Town = "Dublin", RiskRating = RiskRating.Medium },
                    new Premises { Name = "Ocean Breeze", Address = "1 Strand Rd", Town = "Galway", RiskRating = RiskRating.Low },
                    new Premises { Name = "The Fisherman", Address = "9 Dock St", Town = "Galway", RiskRating = RiskRating.High },
                    new Premises { Name = "West Coast Diner", Address = "33 Shop St", Town = "Galway", RiskRating = RiskRating.Medium },
                    new Premises { Name = "The Atlantic Grill", Address = "14 Sea Rd", Town = "Galway", RiskRating = RiskRating.Low },
                };
                context.Premises.AddRange(premises);
                await context.SaveChangesAsync();
            }

            // Seed Inspections
            if (!context.Inspections.Any())
            {
                var premisesList = context.Premises.ToList();
                var now = DateTime.Now;

                var inspections = new List<Inspection>
                {
                    new Inspection { PremisesId = premisesList[0].Id, InspectionDate = now.AddDays(-5), Score = 92, Outcome = Outcome.Pass, Notes = "Excellent hygiene standards." },
                    new Inspection { PremisesId = premisesList[1].Id, InspectionDate = now.AddDays(-10), Score = 45, Outcome = Outcome.Fail, Notes = "Kitchen found dirty." },
                    new Inspection { PremisesId = premisesList[2].Id, InspectionDate = now.AddDays(-15), Score = 70, Outcome = Outcome.Pass, Notes = "Generally satisfactory." },
                    new Inspection { PremisesId = premisesList[3].Id, InspectionDate = now.AddDays(-3), Score = 38, Outcome = Outcome.Fail, Notes = "Pest evidence found." },
                    new Inspection { PremisesId = premisesList[4].Id, InspectionDate = now.AddDays(-7), Score = 88, Outcome = Outcome.Pass, Notes = "Good food storage." },
                    new Inspection { PremisesId = premisesList[5].Id, InspectionDate = now.AddDays(-20), Score = 55, Outcome = Outcome.Fail, Notes = "Temperature issues." },
                    new Inspection { PremisesId = premisesList[6].Id, InspectionDate = now.AddDays(-2), Score = 95, Outcome = Outcome.Pass, Notes = "Outstanding cleanliness." },
                    new Inspection { PremisesId = premisesList[7].Id, InspectionDate = now.AddDays(-12), Score = 60, Outcome = Outcome.Pass, Notes = "Adequate standards." },
                    new Inspection { PremisesId = premisesList[8].Id, InspectionDate = now.AddDays(-8), Score = 42, Outcome = Outcome.Fail, Notes = "Poor waste management." },
                    new Inspection { PremisesId = premisesList[9].Id, InspectionDate = now.AddDays(-1), Score = 78, Outcome = Outcome.Pass, Notes = "Good overall." },
                    new Inspection { PremisesId = premisesList[10].Id, InspectionDate = now.AddDays(-25), Score = 33, Outcome = Outcome.Fail, Notes = "Serious hygiene violations." },
                    new Inspection { PremisesId = premisesList[11].Id, InspectionDate = now.AddDays(-4), Score = 85, Outcome = Outcome.Pass, Notes = "Well maintained." },
                    new Inspection { PremisesId = premisesList[0].Id, InspectionDate = now.AddDays(-35), Score = 80, Outcome = Outcome.Pass, Notes = "Good standards maintained." },
                    new Inspection { PremisesId = premisesList[1].Id, InspectionDate = now.AddDays(-40), Score = 50, Outcome = Outcome.Fail, Notes = "Recurring cleanliness issues." },
                    new Inspection { PremisesId = premisesList[2].Id, InspectionDate = now.AddDays(-45), Score = 72, Outcome = Outcome.Pass, Notes = "Improved since last visit." },
                    new Inspection { PremisesId = premisesList[3].Id, InspectionDate = now.AddDays(-50), Score = 65, Outcome = Outcome.Pass, Notes = "Acceptable." },
                    new Inspection { PremisesId = premisesList[4].Id, InspectionDate = now.AddDays(-55), Score = 40, Outcome = Outcome.Fail, Notes = "Food labelling missing." },
                    new Inspection { PremisesId = premisesList[5].Id, InspectionDate = now.AddDays(-60), Score = 90, Outcome = Outcome.Pass, Notes = "Much improved." },
                    new Inspection { PremisesId = premisesList[6].Id, InspectionDate = now.AddDays(-65), Score = 88, Outcome = Outcome.Pass, Notes = "Consistent high standards." },
                    new Inspection { PremisesId = premisesList[7].Id, InspectionDate = now.AddDays(-70), Score = 35, Outcome = Outcome.Fail, Notes = "Staff hygiene poor." },
                    new Inspection { PremisesId = premisesList[8].Id, InspectionDate = now.AddDays(-75), Score = 75, Outcome = Outcome.Pass, Notes = "Good improvement." },
                    new Inspection { PremisesId = premisesList[9].Id, InspectionDate = now.AddDays(-80), Score = 68, Outcome = Outcome.Pass, Notes = "Satisfactory." },
                    new Inspection { PremisesId = premisesList[10].Id, InspectionDate = now.AddDays(-85), Score = 55, Outcome = Outcome.Fail, Notes = "Storage issues." },
                    new Inspection { PremisesId = premisesList[11].Id, InspectionDate = now.AddDays(-90), Score = 82, Outcome = Outcome.Pass, Notes = "Well run premises." },
                    new Inspection { PremisesId = premisesList[0].Id, InspectionDate = now.AddDays(-95), Score = 77, Outcome = Outcome.Pass, Notes = "Good." },
                };
                context.Inspections.AddRange(inspections);
                await context.SaveChangesAsync();
            }

            // Seed FollowUps
            if (!context.FollowUps.Any())
            {
                var inspectionsList = context.Inspections.ToList();
                var now = DateTime.Now;

                var followUps = new List<FollowUp>
                {
                    new FollowUp { InspectionId = inspectionsList[1].Id, DueDate = now.AddDays(-5), Status = FollowUpStatus.Open },
                    new FollowUp { InspectionId = inspectionsList[3].Id, DueDate = now.AddDays(-10), Status = FollowUpStatus.Open },
                    new FollowUp { InspectionId = inspectionsList[5].Id, DueDate = now.AddDays(-3), Status = FollowUpStatus.Open },
                    new FollowUp { InspectionId = inspectionsList[8].Id, DueDate = now.AddDays(-15), Status = FollowUpStatus.Open },
                    new FollowUp { InspectionId = inspectionsList[10].Id, DueDate = now.AddDays(-20), Status = FollowUpStatus.Open },
                    new FollowUp { InspectionId = inspectionsList[13].Id, DueDate = now.AddDays(7), Status = FollowUpStatus.Open },
                    new FollowUp { InspectionId = inspectionsList[16].Id, DueDate = now.AddDays(-8), Status = FollowUpStatus.Closed, ClosedDate = now.AddDays(-2) },
                    new FollowUp { InspectionId = inspectionsList[19].Id, DueDate = now.AddDays(-30), Status = FollowUpStatus.Closed, ClosedDate = now.AddDays(-25) },
                    new FollowUp { InspectionId = inspectionsList[22].Id, DueDate = now.AddDays(-40), Status = FollowUpStatus.Closed, ClosedDate = now.AddDays(-35) },
                    new FollowUp { InspectionId = inspectionsList[1].Id, DueDate = now.AddDays(14), Status = FollowUpStatus.Open },
                };
                context.FollowUps.AddRange(followUps);
                await context.SaveChangesAsync();
            }
        }
    }
}