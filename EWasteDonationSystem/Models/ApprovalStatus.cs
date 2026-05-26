using System;

namespace EWasteDonationSystem.Models
{
    /// <summary>
    /// Admin moderation status for donors and student applications.
    /// </summary>
    public enum ApprovalStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2
    }
}
