using System.ComponentModel.DataAnnotations;

namespace Calory.Domain
{
    public class HealthGoal
    {
        [Key]
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public decimal DailyCalorieTarget { get; set; }

        public decimal ProteinTarget { get; set; }

        public decimal CarbTarget { get; set; }

        public decimal FatTarget { get; set; }

        public decimal WeightTarget { get; set; }

        public DateOnly StartDate { get; set; }

        public DateOnly? EndDate { get; set; } 

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
