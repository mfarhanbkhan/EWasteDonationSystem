using System.Web.Mvc;
using EWasteDonationSystem.Models;
using EWasteDonationSystem.Service;

namespace EWasteDonationSystem.Controllers
{
    /// <summary>
    /// Thin controller that forwards account requests to AccountService.
    /// </summary>
    public class AccountController : Controller
    {
        private readonly AppDbContext _db = new AppDbContext();
        private readonly AccountService _accountService;

        public AccountController()
        {
            _accountService = new AccountService(_db);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AdminLogin(string email, string password)
        {
            string errorMessage;
            if (_accountService.TryAdminLogin(Session, email, password, out errorMessage))
            {
                TempData["Success"] = "Successfully login";
                return RedirectToAction("Dashboard", "Admin");
            }

            TempData["Error"] = errorMessage;
            return RedirectToAction("ChooseRole", "Home", new { role = "admin", mode = "login" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AdminForgotPassword(string email, string newPassword, string confirmPassword)
        {
            string message;
            if (_accountService.TryRecoverAdminPassword(Session, email, newPassword, confirmPassword, out message))
            {
                TempData["Success"] = message;
            }
            else
            {
                TempData["Error"] = message;
            }

            return RedirectToAction("ChooseRole", "Home", new { role = "admin", mode = "login" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DonorSignUp(string userName, string name, string email, string password, string confirmPassword)
        {
            string message;
            if (_accountService.TryDonorSignUp(userName, name, email, password, confirmPassword, out message))
            {
                TempData["Success"] = "OTP sent to your email. Please verify your account before logging in.";
                return RedirectToAction("ChooseRole", "Home", new { role = "donor", mode = "login", showOtp = true, otpEmail = email?.Trim(), otpRole = "donor" });
            }

            TempData["Error"] = message;
            return RedirectToAction("ChooseRole", "Home", new { role = "donor", mode = "signup" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult VerifyDonorOtp(string email, string otp)
        {
            string message;
            if (_accountService.VerifyOtp(email, otp, out message))
            {
                TempData["Success"] = message;
                return RedirectToAction("ChooseRole", "Home", new { role = "donor", mode = "login" });
            }

            TempData["Error"] = message;
            return RedirectToAction("ChooseRole", "Home", new { role = "donor", mode = "login", showOtp = true, otpEmail = email?.Trim(), otpRole = "donor" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult SendDonorPasswordResetOtp(string email)
        {
            string message;
            var success = _accountService.TrySendDonorPasswordResetOtp(email, out message);
            return Json(new { success, message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DonorForgotPassword(string email, string otp, string newPassword, string confirmPassword)
        {
            string message;
            if (_accountService.TryRecoverDonorPassword(email, otp, newPassword, confirmPassword, out message))
            {
                TempData["Success"] = message;
            }
            else
            {
                TempData["Error"] = message;
            }

            return RedirectToAction("ChooseRole", "Home", new { role = "donor", mode = "login" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DonorLogin(string email, string password)
        {
            string message;
            if (_accountService.TryDonorLogin(Session, email, password, out message))
            {
                TempData["Success"] = "Successfully login";
                return RedirectToAction("Dashboard", "Donor");
            }

            TempData["Error"] = message;
            return RedirectToAction("ChooseRole", "Home", new { role = "donor", mode = "login" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult StudentSignUp(string userName, string name, string email, string password, string confirmPassword)
        {
            string message;
            if (_accountService.TryStudentSignUp(userName, name, email, password, confirmPassword, out message))
            {
                TempData["Success"] = "OTP sent to your email. Please verify your account before logging in.";
                return RedirectToAction("ChooseRole", "Home", new { role = "student", mode = "login", showOtp = true, otpEmail = email?.Trim(), otpRole = "student" });
            }

            TempData["Error"] = message;
            return RedirectToAction("ChooseRole", "Home", new { role = "student", mode = "signup" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult VerifyStudentOtp(string email, string otp)
        {
            string message;
            if (_accountService.VerifyStudentOtp(email, otp, out message))
            {
                TempData["Success"] = message;
                return RedirectToAction("ChooseRole", "Home", new { role = "student", mode = "login" });
            }

            TempData["Error"] = message;
            return RedirectToAction("ChooseRole", "Home", new { role = "student", mode = "login", showOtp = true, otpEmail = email?.Trim(), otpRole = "student" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult SendStudentPasswordResetOtp(string email)
        {
            string message;
            var success = _accountService.TrySendStudentPasswordResetOtp(email, out message);
            return Json(new { success, message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult StudentForgotPassword(string email, string otp, string newPassword, string confirmPassword)
        {
            string message;
            if (_accountService.TryRecoverStudentPassword(email, otp, newPassword, confirmPassword, out message))
            {
                TempData["Success"] = message;
            }
            else
            {
                TempData["Error"] = message;
            }

            return RedirectToAction("ChooseRole", "Home", new { role = "student", mode = "login" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult StudentLogin(string email, string password)
        {
            string message;
            if (_accountService.TryStudentLogin(Session, email, password, out message))
            {
                TempData["Success"] = "Successfully login";
                return RedirectToAction("Dashboard", "Student");
            }

            TempData["Error"] = message;
            return RedirectToAction("ChooseRole", "Home", new { role = "student", mode = "login" });
        }

        [HttpGet]
        public ActionResult Logout(string role = "donor")
        {
            var normalizedRole = (role ?? "donor").Trim().ToLower();
            if (normalizedRole != "admin" && normalizedRole != "student" && normalizedRole != "donor")
            {
                normalizedRole = "donor";
            }

            Session.Clear();
            Session.Abandon();

            TempData["Success"] = "Logged out successfully.";
            return RedirectToAction("ChooseRole", "Home", new { role = normalizedRole, mode = "login" });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
