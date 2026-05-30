using System.Web.Mvc;
using EWasteDonationSystem.Models;
using EWasteDonationSystem.Service;

namespace EWasteDonationSystem.Controllers
{
    /// <summary>
    /// Public landing pages. Business/data logic is delegated to HomeService.
    /// </summary>
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            using (var db = new AppDbContext())
            {
                var service = new HomeService(db);
                var stats = service.GetIndexStats();
                ViewBag.DonorCount = stats.DonorCount;
                ViewBag.StudentCount = stats.StudentCount;
                ViewBag.ItemCount = stats.ItemCount;

                ViewBag.UserCount = stats.UserCount;
                ViewBag.PendingItems = stats.PendingItems;
                ViewBag.ApprovedDonorCount = stats.ApprovedDonorCount;
                ViewBag.RejectedDonationItems = stats.RejectedDonationItems;
                ViewBag.PendingStudentApplications = stats.PendingStudentApplications;
                ViewBag.ApprovedStudentApplications = stats.ApprovedStudentApplications;
                ViewBag.RejectedStudentApplications = stats.RejectedStudentApplications;

            }

            return View();
        }

        public ActionResult ChooseRole(string role = "donor", string mode = "login", bool showOtp = false, string otpEmail = null, string otpRole = "donor")
        {
            using (var db = new AppDbContext())
            {
                var service = new HomeService(db);
                var stats = service.GetChooseRoleStats();
                ViewBag.DonorCount = stats.DonorCount;
                ViewBag.StudentCount = stats.StudentCount;
                ViewBag.PendingItems = stats.PendingItems;

                ViewBag.UserCount = stats.UserCount;
                ViewBag.ApprovedDonorCount = stats.ApprovedDonorCount;
                ViewBag.PendingStudentApplications = stats.PendingStudentApplications;
                ViewBag.ApprovedStudentApplications = stats.ApprovedStudentApplications;
            }

            ViewBag.ShowOtpDialog = showOtp;
            ViewBag.OtpEmail = otpEmail ?? string.Empty;
            ViewBag.OtpRole = (otpRole ?? role ?? "donor").ToLower();

            var vm = new ChooseRoleViewModel
            {
                SelectedRole = (role ?? "donor").ToLower(),
                Mode = (mode ?? "login").ToLower()
            };

            return View(vm);
        }
    }
}
