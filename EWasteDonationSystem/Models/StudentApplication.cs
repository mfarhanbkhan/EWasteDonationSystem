using System;
using System.ComponentModel.DataAnnotations;

namespace EWasteDonationSystem.Models
{
    /// <summary>
    /// An application submitted by a student, requesting items.
    /// </summary>
    public class StudentApplication
    {
        public StudentApplication()
        {
            CreatedAtUtc = DateTime.UtcNow;
        }

        public int Id { get; set; }

        public int StudentId { get; set; }

        [Required, StringLength(500)]
        public string ItemsNeeded { get; set; }

        [StringLength(800)]
        public string Reason { get; set; }

        public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;

        public DateTime CreatedAtUtc { get; set; }

        // Navigation
        public virtual Student Student { get; set; }
    }
}
