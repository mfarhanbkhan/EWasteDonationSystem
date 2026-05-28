using System;
using System.ComponentModel.DataAnnotations;

namespace EWasteDonationSystem.Models
{
    public class User
    {
        public User()
        {
            CreatedAtUtc = DateTime.UtcNow;
        }

        public int Id { get; set; }

        [Required, StringLength(80)]
        public string FullName { get; set; }

        [EmailAddress, StringLength(120)]
        public string Email { get; set; }

        [Required, StringLength(200)]
        public string Password { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAtUtc { get; set; }

        //new fields
        [StringLength(200)]
        public string EmailOtp { get; set; }
        public DateTime? OtpExpiresAt { get; set; } = DateTime.UtcNow;
        public bool IsEmailVerified { get; set; }

    }
}
