using System.Web;
using System.Web.Mvc;
using EWasteDonationSystem.Models;
using EWasteDonationSystem.Service;

namespace EWasteDonationSystem.Controllers
{
    /// <summary>
    /// Thin donor controller. Data preparation and business logic live in DonorService.
    /// </summary>
    public class DonorController : Controller
    {
        private readonly AppDbContext _db = new AppDbContext();
        private readonly DonorService _donorService;

        public DonorController()
        {
            _donorService = new DonorService(_db);
        }

        [HttpGet]
        public ActionResult Dashboard(int? id)
        {
            var vm = _donorService.BuildDashboard(Session, id);
            return View(vm);
        }

        [HttpGet]
        public ActionResult Detail(int? id)
        {
            // IMPORTANT: Dashboard passes DonationItem.Id into this action.
            // So in this page, "id" means itemId (not donorId).
            var vm = _donorService.GetItemDetail(Session, id);
            if (!id.HasValue && Session["DonorId"] == null)
            {
                return RedirectToAction("Dashboard");
            }
            if (vm == null)
            {
                return HttpNotFound();
            }
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveProfile(Donor donor)
        {
            ModelState.Remove("Password");
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please fill required donor fields.";
                return RedirectToAction("Dashboard", new { id = donor != null ? donor.Id : 0 });
            }

            int donorId;
            string message;
            if (!_donorService.SaveProfile(Session, donor, out donorId, out message))
            {
                TempData["Error"] = message;
                return RedirectToAction("Dashboard", new { id = donorId });
            }

            return RedirectToAction("Dashboard", new { id = donorId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PostDonation(DonorDashboardVm vm, HttpPostedFileBase itemImage)
        {
            int donorId;
            string message;
            if (!_donorService.PostDonation(Session, Server, vm, itemImage, out donorId, out message))
            {
                TempData["Error"] = message;
                return RedirectToAction("Dashboard", new { id = donorId });
            }

            TempData["Success"] = "Successfully Submitted";
            return RedirectToAction("Dashboard", new { id = donorId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteMessageForMe(int donorId, int messageId)
        {
            _donorService.DeleteMessageForMe(Session, messageId, out donorId);
            return RedirectToAction("Dashboard", new { id = donorId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteMessageForEveryone(int donorId, int messageId)
        {
            _donorService.DeleteMessageForEveryone(Session, messageId, out donorId);
            return RedirectToAction("Dashboard", new { id = donorId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SendMessage(int donorId, string message)
        {
            _donorService.SendMessage(Session, message, out donorId);
            return RedirectToAction("Dashboard", new { id = donorId });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
