using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace FoodSafetyTracker.Web.Models
{
    public enum FollowUpStatus { Open, Closed }

    public class FollowUp
    {
        public int Id { get; set; }
        public int InspectionId { get; set; }
        public DateTime DueDate { get; set; }
        public FollowUpStatus Status { get; set; }
        public DateTime? ClosedDate { get; set; }

        [ValidateNever]
        public Inspection Inspection { get; set; } = null!;
    }
}