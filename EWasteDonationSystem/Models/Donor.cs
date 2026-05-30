using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EWasteDonationSystem.Models
{
    /// <summary>
    /// A person who donates items.
    /// NOTE: Location/lat-lon removed as requested.
    /// </summary>
    public class Donor
    {
        public Donor()
        {
            // Defaults for older C# language versions (MVC5 templates often use C# 5)
            Status = ApprovalStatus.Pending;
            CreatedAtUtc = DateTime.UtcNow;

            DonationItems = new List<DonationItem>();
            ChatMessages = new List<ChatMessage>();
        }

        public int Id { get; set; }

        [Required, StringLength(80)]
        public string FullName { get; set; }

        [StringLength(30)]
        public string UserName { get; set; }

        [EmailAddress, StringLength(120)]
        public string Email { get; set; }

        [Required, StringLength(200)]
        public string Password { get; set; }

        [StringLength(60)]
        public string City { get; set; }

        [StringLength(200)]
        public string Address { get; set; }

        public ApprovalStatus Status { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        //new fields
        [StringLength(200)]
        public string EmailOtp { get; set; }
        public DateTime? OtpExpiresAt { get; set; } = DateTime.UtcNow;
        public bool IsEmailVerified { get; set; }


        // Navigation
        public virtual ICollection<DonationItem> DonationItems { get; set; }
        public virtual ICollection<ChatMessage> ChatMessages { get; set; }
    }
}
