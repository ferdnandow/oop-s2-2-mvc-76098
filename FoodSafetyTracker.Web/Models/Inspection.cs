using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace FoodSafetyTracker.Web.Models
{
    public enum Outcome { Pass, Fail }

    public class Inspection
    {
        public int Id { get; set; }
        public int PremisesId { get; set; }
        public DateTime InspectionDate { get; set; }
        public int Score { get; set; } // 0-100
        public Outcome Outcome { get; set; }
        public string? Notes { get; set; }

        [ValidateNever]
        public Premises Premises { get; set; } = null!;

        [ValidateNever]
        public ICollection<FollowUp> FollowUps { get; set; } = new List<FollowUp>();
    }
}