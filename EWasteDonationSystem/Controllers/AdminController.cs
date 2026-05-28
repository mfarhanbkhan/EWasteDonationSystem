using System.Web.Mvc;
using EWasteDonationSystem.Models;
using EWasteDonationSystem.Service;

namespace EWasteDonationSystem.Controllers
{
    /// <summary>
    /// Thin admin controller. Services now hold the heavier dashboard logic.
    /// </summary>
    public class AdminController : Controller
    {
        private readonly AppDbContext _db = new AppDbContext();
        private readonly AdminService _adminService;

        public AdminController()
        {
            _adminService = new AdminService(_db);
        }

        private ActionResult RedirectToAdminLogin()
        {
            TempData["Error"] = "Admin login required.";
            return RedirectToAction("ChooseRole", "Home", new { role = "admin", mode = "login" });
        }

        [HttpGet]
        public ActionResult Dashboard(string chatTarget, int? chatPersonId)
        {
            if (!_adminService.IsAdminLoggedIn(Session)) return RedirectToAdminLogin();
            var resetAssignFields = TempData["ResetAssignFields"] is bool && (bool)TempData["ResetAssignFields"];
            var vm = _adminService.BuildDashboard(Session, Session, chatTarget, chatPersonId, resetAssignFields);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SetDonorStatus(int id, ApprovalStatus status)
        {
            if (!_adminService.IsAdminLoggedIn(Session)) return RedirectToAdminLogin();
            if (!_adminService.SetDonorStatus(id, status)) return HttpNotFound();
            TempData["Success"] = "Donor status updated successfully.";
            return RedirectToAction("Dashboard");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SetStudentStatus(int id, ApprovalStatus status)
        {
            if (!_adminService.IsAdminLoggedIn(Session)) return RedirectToAdminLogin();
            if (!_adminService.SetStudentStatus(id, status)) return HttpNotFound();
            TempData["Success"] = "Student status updated successfully.";
            return RedirectToAction("Dashboard");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddPickupAgent(string agentName, string phone, string area, string email)
        {
            if (!_adminService.IsAdminLoggedIn(Session)) return RedirectToAdminLogin();
            string message;
            if (!_adminService.TryAddPickupAgent(Session, agentName, phone, area, email, out message))
            {
                TempData["Error"] = message;
                return RedirectToAction("Dashboard");
            }
            TempData["Success"] = message;
            return RedirectToAction("Dashboard");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AssignPickupAgent(int? donorId, string agentCode, string location)
        {
            if (!_adminService.IsAdminLoggedIn(Session)) return RedirectToAdminLogin();
            string message;
            if (!_adminService.TryAssignPickupAgent(Session, donorId, agentCode, location, out message))
            {
                TempData["Error"] = message;
                TempData["ResetAssignFields"] = false;
                return RedirectToAction("Dashboard");
            }
            TempData["ResetAssignFields"] = true;
            TempData["Success"] = "Pickup agent assigned successfully.";
            return RedirectToAction("Dashboard");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteDashboardMessageForMe(string chatTarget, int? chatPersonId, int messageId)
        {
            if (!_adminService.IsAdminLoggedIn(Session)) return RedirectToAdminLogin();
            var normalizedChatTarget = string.Equals(chatTarget, "Student", System.StringComparison.OrdinalIgnoreCase) ? "Student" : "Donor";
            if (!chatPersonId.HasValue) return RedirectToAction("Dashboard", new { chatTarget = normalizedChatTarget });
            _adminService.HideDashboardMessageForMe(Session, normalizedChatTarget, chatPersonId.Value, messageId);
            return RedirectToAction("Dashboard", new { chatTarget = normalizedChatTarget, chatPersonId = chatPersonId.Value });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteDashboardMessageForEveryone(string chatTarget, int? chatPersonId, int messageId)
        {
            if (!_adminService.IsAdminLoggedIn(Session)) return RedirectToAdminLogin();
            var normalizedChatTarget = string.Equals(chatTarget, "Student", System.StringComparison.OrdinalIgnoreCase) ? "Student" : "Donor";
            if (!chatPersonId.HasValue) return RedirectToAction("Dashboard", new { chatTarget = normalizedChatTarget });
            _adminService.DeleteDashboardMessageForEveryone(normalizedChatTarget, chatPersonId.Value, messageId);
            return RedirectToAction("Dashboard", new { chatTarget = normalizedChatTarget, chatPersonId = chatPersonId.Value });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SendDashboardMessage(string chatTarget, int? chatPersonId, string message)
        {
            if (!_adminService.IsAdminLoggedIn(Session)) return RedirectToAdminLogin();
            var normalizedChatTarget = string.Equals(chatTarget, "Student", System.StringComparison.OrdinalIgnoreCase) ? "Student" : "Donor";
            if (!chatPersonId.HasValue || string.IsNullOrWhiteSpace(message)) return RedirectToAction("Dashboard", new { chatTarget = normalizedChatTarget, chatPersonId = chatPersonId });
            _adminService.SendDashboardMessage(normalizedChatTarget, chatPersonId.Value, message);
            return RedirectToAction("Dashboard", new { chatTarget = normalizedChatTarget, chatPersonId = chatPersonId.Value });
        }

        [HttpGet]
        public ActionResult DonorDetail(int? id, int? donorId)
        {
            if (!_adminService.IsAdminLoggedIn(Session)) return RedirectToAdminLogin();
            // id = DonationItem.Id from dashboard; donorId = optional fallback when opening by donor only.
            var vm = _adminService.GetDonorItemDetail(id, donorId);
            if (vm == null) return HttpNotFound();
            return View(vm);
        }

        [HttpGet]
        public ActionResult StudentDetail(int id)
        {
            if (!_adminService.IsAdminLoggedIn(Session)) return RedirectToAdminLogin();
            var student = _adminService.GetStudentDetail(id);
            if (student == null) return HttpNotFound();
            return View(student);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SendMessageToDonor(int donorId, string message, int? itemId)
        {
            if (!_adminService.IsAdminLoggedIn(Session)) return RedirectToAdminLogin();
            _adminService.SendMessageToDonor(donorId, message);
            if (itemId.HasValue)
                return RedirectToAction("DonorDetail", new { id = itemId });
            return RedirectToAction("DonorDetail", new { donorId = donorId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SendMessageToStudent(int studentId, string message)
        {
            if (!_adminService.IsAdminLoggedIn(Session)) return RedirectToAdminLogin();
            _adminService.SendMessageToStudent(studentId, message);
            return RedirectToAction("StudentDetail", new { id = studentId });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
