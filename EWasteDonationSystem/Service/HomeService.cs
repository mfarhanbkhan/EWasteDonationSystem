using System.Linq;
using EWasteDonationSystem.Models;

namespace EWasteDonationSystem.Service
{
    /// <summary>
    /// Supplies lightweight data for public pages such as counts and defaults.
    /// </summary>
    public class HomeService
    {
        private readonly AppDbContext _db;

        public HomeService(AppDbContext db)
        {
            _db = db;
        }

        public HomeStatsVm GetIndexStats()
        {
            return new HomeStatsVm
            {
                DonorCount = _db.Donors.Count(),
                ItemCount = _db.DonationItems.Count(),
                ApprovedDonorCount = _db.DonationItems.Count(x => x.Status == ApprovalStatus.Approved),
                PendingItems = _db.DonationItems.Count(x => x.Status == ApprovalStatus.Pending),
                RejectedDonationItems = _db.DonationItems.Count(x => x.Status == ApprovalStatus.Rejected),

                StudentCount = _db.Students.Count(),
                ApprovedStudentApplications = _db.StudentApplications.Count(x => x.Status == ApprovalStatus.Approved),
                PendingStudentApplications = _db.StudentApplications.Count(x => x.Status == ApprovalStatus.Pending),
                RejectedStudentApplications = _db.StudentApplications.Count(x => x.Status == ApprovalStatus.Rejected),
            };
        }

        public HomeStatsVm GetChooseRoleStats()
        {
            return new HomeStatsVm
            {
                DonorCount = _db.Donors.Count(),
                StudentCount = _db.Students.Count(),
                PendingItems = _db.DonationItems.Count(x => x.Status == ApprovalStatus.Pending)
            };
        }
    }

    /// <summary>
    /// Small DTO used to send summary counts back to the controller.
    /// </summary>
    public class HomeStatsVm
    {
        public int DonorCount { get; set; }
        public int StudentCount { get; set; }
        public int ItemCount { get; set; }
        public int PendingItems { get; set; }
        public int UserCount { get; set; }
        public int ApprovedDonorCount { get; set; }
        public int RejectedDonationItems { get; set; }
        public int PendingStudentApplications { get; set; }
        public int ApprovedStudentApplications { get; set; }
        public int RejectedStudentApplications { get; set; }
    }
}
