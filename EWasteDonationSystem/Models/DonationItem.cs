using System;
using System.ComponentModel.DataAnnotations;

namespace EWasteDonationSystem.Models
{
    /// <summary>
    /// Item posted by a donor.
    /// </summary>
    public class DonationItem
    {
        public DonationItem()
        {
            Quantity = 1;
            CreatedAtUtc = DateTime.UtcNow;
        }

        public int Id { get; set; }

        public int DonorId { get; set; }

        [Required, StringLength(120)]
        public string ItemName { get; set; }

        [Range(1, 100000)]
        public int Quantity { get; set; }

        [Range(1, 1000000)]
        public decimal Price { get; set; }

        [StringLength(80)]
        public string Category { get; set; }

        [StringLength(60)]
        public string Condition { get; set; }

        /// <summary>
        /// Relative path of uploaded image under ~/Content/uploads
        /// </summary>
        [StringLength(260)]
        public string ImagePath { get; set; }

        [StringLength(500)]
        public string Notes { get; set; }
        public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;

        public DateTime CreatedAtUtc { get; set; }

        // Navigation
        public virtual Donor Donor { get; set; }
    }
}
