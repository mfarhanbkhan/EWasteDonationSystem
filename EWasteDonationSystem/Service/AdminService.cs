// Project documentation note: This file contains commented code for easier understanding.
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web.Configuration;
using System.Web;
using EWasteDonationSystem.Models;

namespace EWasteDonationSystem.Service
{
    /// <summary>
    /// Contains admin dashboard business logic and session-backed helper operations.
    /// </summary>
    public class AdminService
    {
        private readonly AppDbContext _db;

        public AdminService(AppDbContext db)
        {
            _db = db;
        }

        public bool IsAdminLoggedIn(HttpSessionStateBase session)
        {
            return session["AdminLoggedIn"] is bool && (bool)session["AdminLoggedIn"];
        }

        public AdminDashboardVm BuildDashboard(HttpSessionStateBase session, HttpSessionStateBase tempDataSession, string chatTarget, int? chatPersonId, bool resetAssignFields)
        {
            var totalDonors = _db.Donors.AsNoTracking().Count();
            var totalStudents = _db.Students.AsNoTracking().Count();
            var pendingDonors = _db.DonationItems.AsNoTracking().Count(d => d.Status == ApprovalStatus.Pending);
            var pendingStudents = _db.StudentApplications.AsNoTracking().Count(s => s.Status == ApprovalStatus.Pending);

            var donors = _db.Donors.AsNoTracking().OrderByDescending(d => d.Id).ToList();
            var students = _db.Students.AsNoTracking().OrderByDescending(s => s.Id).ToList();
            var donationItems = _db.DonationItems.AsNoTracking().Include(i => i.Donor).OrderByDescending(i => i.Id).ToList();
            var studentApplications = _db.StudentApplications.AsNoTracking().Include(a => a.Student).OrderByDescending(a => a.Id).Take(20).ToList();

            var normalizedChatTarget = string.Equals(chatTarget, "Student", StringComparison.OrdinalIgnoreCase) ? "Student" : "Donor";
            var selectedDonor = normalizedChatTarget == "Donor"
                ? donors.FirstOrDefault(d => !chatPersonId.HasValue || d.Id == chatPersonId.Value) ?? donors.FirstOrDefault()
                : donors.FirstOrDefault();
            var selectedStudent = normalizedChatTarget == "Student"
                ? students.FirstOrDefault(s => !chatPersonId.HasValue || s.Id == chatPersonId.Value) ?? students.FirstOrDefault()
                : students.FirstOrDefault();
            var selectedDonorId = normalizedChatTarget == "Donor" && selectedDonor != null ? (int?)selectedDonor.Id : null;
            var selectedStudentId = normalizedChatTarget == "Student" && selectedStudent != null ? (int?)selectedStudent.Id : null;

            var hiddenMessageIds = normalizedChatTarget == "Student" && selectedStudentId.HasValue
                ? GetHiddenMessageIds(session, normalizedChatTarget, selectedStudentId.Value)
                : (selectedDonorId.HasValue ? GetHiddenMessageIds(session, normalizedChatTarget, selectedDonorId.Value) : new HashSet<int>());

            var previewMessages = _db.ChatMessages.AsNoTracking()
                .Where(x => (((selectedDonorId.HasValue && x.DonorId == selectedDonorId.Value) || (selectedStudentId.HasValue && x.StudentId == selectedStudentId.Value))) && !hiddenMessageIds.Contains(x.Id))
                .OrderByDescending(x => x.SentAtUtc)
                .Take(20)
                .ToList()
                .OrderBy(x => x.SentAtUtc)
                .ToList();

            var pickupAgents = GetPickupAgents(session);
            var pickupAssignments = GetPickupAssignments(session);

            return new AdminDashboardVm
            {
                TotalDonors = totalDonors,
                TotalStudents = totalStudents,
                PendingDonors = pendingDonors,
                PendingStudents = pendingStudents,
                TotalPickupAssignments = pickupAssignments.Count,
                RecentDonorCount = donors.Count,
                RecentStudentCount = students.Count,
                Donors = donors,
                Students = students,
                DonationItems = donationItems,
                StudentApplications = studentApplications,
                PickupAgents = pickupAgents,
                PickupAssignments = pickupAssignments.OrderByDescending(x => x.AssignedAtUtc).ToList(),
                SelectedDonorId = resetAssignFields ? string.Empty : (selectedDonor != null ? selectedDonor.Id.ToString() : string.Empty),
                SelectedAgentCode = resetAssignFields ? string.Empty : pickupAgents.Select(x => x.AgentCode).FirstOrDefault(),
                SelectedChatTarget = normalizedChatTarget,
                SelectedChatPerson = normalizedChatTarget == "Student"
                    ? (selectedStudent != null ? selectedStudent.FullName + " (" + selectedStudent.Status.ToString().ToLower() + ")" : "No students yet")
                    : (selectedDonor != null ? selectedDonor.FullName + " (" + selectedDonor.Status.ToString().ToLower() + ")" : "No donors yet"),
                SelectedChatPersonId = normalizedChatTarget == "Student"
                    ? (selectedStudent != null ? selectedStudent.Id.ToString() : string.Empty)
                    : (selectedDonor != null ? selectedDonor.Id.ToString() : string.Empty),
                PreviewMessages = previewMessages
            };
        }

        public bool SetDonorStatus(int itemId, ApprovalStatus status)
        {
            var record = _db.DonationItems.Find(itemId);
            if (record == null) return false;
            record.Status = status;
            _db.SaveChanges();
            //SendEmailIfConfigured(
            //    donor.Email,
            //    "E-Waste Donor Status Update",
            //    "Dear " + (string.IsNullOrWhiteSpace(donor.FullName) ? "Donor" : donor.FullName) + ",\n\nYour donor request status is now: " + status + ".\n\nThanks,\nE-Waste Team");
            return true;
        }

        public bool SetStudentStatus(int itemId, ApprovalStatus status)
        {
            var record = _db.StudentApplications.Find(itemId);
            if (record == null) return false;
            record.Status = status;
            _db.SaveChanges();

            //SendEmailIfConfigured(
            //    student.Email,
            //    "E-Waste Student Status Update",
            //    "Dear " + (string.IsNullOrWhiteSpace(student.FullName) ? "Student" : student.FullName) + ",\n\nYour student request status is now: " + status + ".\n\nThanks,\nE-Waste Team");
            return true;
        }

        public bool TryAddPickupAgent(HttpSessionStateBase session, string agentName, string phone, string area, string email, out string message)
        {
            if (string.IsNullOrWhiteSpace(agentName) || string.IsNullOrWhiteSpace(phone) || string.IsNullOrWhiteSpace(area) || string.IsNullOrWhiteSpace(email))
            {
                message = "Please fill agent name, phone, area and email.";
                return false;
            }

            var trimmedEmail = email.Trim();
            if (!trimmedEmail.Contains("@"))
            {
                message = "Please enter a valid pickup agent email.";
                return false;
            }

            var agents = GetPickupAgents(session);
            var nextNumber = agents.Select(a =>
            {
                var code = a.AgentCode ?? string.Empty;
                var digits = new string(code.Where(char.IsDigit).ToArray());
                int parsed;
                return int.TryParse(digits, out parsed) ? parsed : 100;
            }).DefaultIfEmpty(100).Max() + 1;

            agents.Add(new AdminPickupAgentVm
            {
                AgentCode = "AGT-" + nextNumber,
                AgentName = agentName.Trim(),
                Phone = phone.Trim(),
                Area = area.Trim(),
                Email = trimmedEmail
            });

            session["AdminPickupAgents"] = agents;
            message = "Pickup agent added successfully.";
            return true;
        }

        public bool TryAssignPickupAgent(HttpSessionStateBase session, int? donorId, string agentCode, string location, out string message)
        {
            if (!donorId.HasValue || string.IsNullOrWhiteSpace(agentCode) || string.IsNullOrWhiteSpace(location))
            {
                message = "Please select donor, pickup agent and location.";
                return false;
            }

            var donor = _db.Donors.AsNoTracking().FirstOrDefault(d => d.Id == donorId.Value);
            var agent = GetPickupAgents(session).FirstOrDefault(a => a.AgentCode == agentCode);
            if (donor == null || agent == null)
            {
                message = "Selected donor or pickup agent was not found.";
                return false;
            }

            var assignments = GetPickupAssignments(session);
            assignments.Add(new AdminPickupAssignmentVm
            {
                DonorName = donor.FullName + " (DNR-" + donor.Id.ToString("000000") + ")",
                AgentName = agent.AgentName + " (" + agent.Area + ")",
                Location = location.Trim(),
                AssignedAtUtc = DateTime.UtcNow
            });
            session["AdminPickupAssignments"] = assignments;
            SendEmailIfConfigured(
                agent.Email,
                "Pickup Assignment Notification",
                "Hello " + agent.AgentName + ",\n\nYou have a new pickup assignment.\nDonor: " + donor.FullName + " (DNR-" + donor.Id.ToString("000000") + ")\nLocation: " + location.Trim() + "\n\nThanks,\nAdmin Team");
            message = null;
            return true;
        }

        public void HideDashboardMessageForMe(HttpSessionStateBase session, string chatTarget, int chatPersonId, int messageId)
        {
            var normalizedChatTarget = string.Equals(chatTarget, "Student", StringComparison.OrdinalIgnoreCase) ? "Student" : "Donor";
            var message = _db.ChatMessages.AsNoTracking().FirstOrDefault(m =>
                m.Id == messageId && ((normalizedChatTarget == "Student" && m.StudentId == chatPersonId) || (normalizedChatTarget == "Donor" && m.DonorId == chatPersonId)));
            if (message == null) return;

            var hiddenIds = GetHiddenMessageIds(session, normalizedChatTarget, chatPersonId);
            hiddenIds.Add(messageId);
            session["AdminHiddenChatMessageIds_" + normalizedChatTarget + "_" + chatPersonId] = hiddenIds;
        }

        public void DeleteDashboardMessageForEveryone(string chatTarget, int chatPersonId, int messageId)
        {
            var normalizedChatTarget = string.Equals(chatTarget, "Student", StringComparison.OrdinalIgnoreCase) ? "Student" : "Donor";
            var message = _db.ChatMessages.FirstOrDefault(m =>
                m.Id == messageId && m.SenderRole == "Admin" && ((normalizedChatTarget == "Student" && m.StudentId == chatPersonId) || (normalizedChatTarget == "Donor" && m.DonorId == chatPersonId)));
            if (message == null) return;

            _db.ChatMessages.Remove(message);
            _db.SaveChanges();
        }

        public bool SendDashboardMessage(string chatTarget, int chatPersonId, string message)
        {
            var normalizedChatTarget = string.Equals(chatTarget, "Student", StringComparison.OrdinalIgnoreCase) ? "Student" : "Donor";
            if (string.IsNullOrWhiteSpace(message)) return false;

            var chatMessage = new ChatMessage
            {
                SenderRole = "Admin",
                Message = message.Trim(),
                SentAtUtc = DateTime.UtcNow
            };

            if (normalizedChatTarget == "Student")
            {
                if (!_db.Students.AsNoTracking().Any(s => s.Id == chatPersonId)) return false;
                chatMessage.StudentId = chatPersonId;
            }
            else
            {
                if (!_db.Donors.AsNoTracking().Any(d => d.Id == chatPersonId)) return false;
                chatMessage.DonorId = chatPersonId;
            }

            _db.ChatMessages.Add(chatMessage);
            _db.SaveChanges();
            return true;
        }

        public Donor GetDonorDetail(int id)
        {
            return _db.Donors.Include(d => d.DonationItems).Include(d => d.ChatMessages).FirstOrDefault(d => d.Id == id);
        }

        /// <summary>
        /// Loads donor profile + selected donation item for the admin detail page.
        /// id = DonationItem.Id; donorId = fallback when opening by donor only (latest item).
        /// </summary>
        public DonorItemDetailVm GetDonorItemDetail(int? itemId, int? donorId)
        {
            if (!itemId.HasValue && donorId.HasValue)
            {
                var latest = _db.DonationItems
                    .Where(x => x.DonorId == donorId.Value)
                    .OrderByDescending(x => x.Id)
                    .FirstOrDefault();
                if (latest != null) itemId = latest.Id;
            }

            if (!itemId.HasValue) return null;

            var item = _db.DonationItems.Find(itemId.Value);
            if (item == null) return null;

            var donor = _db.Donors.Find(item.DonorId);
            if (donor == null) return null;

            return new DonorItemDetailVm
            {
                Donor = donor,
                SelectedItem = item,
                OtherItems = _db.DonationItems
                    .Where(x => x.DonorId == donor.Id && x.Id != item.Id)
                    .OrderByDescending(x => x.Id)
                    .Take(50)
                    .ToList(),
                Chat = _db.ChatMessages
                    .Where(x => x.DonorId == donor.Id)
                    .OrderBy(x => x.SentAtUtc)
                    .ToList()
            };
        }

        public Student GetStudentDetail(int id)
        {
            return _db.Students.Include(s => s.Applications).Include(s => s.ChatMessages).FirstOrDefault(s => s.Id == id);
        }

        public void SendMessageToDonor(int donorId, string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return;
            _db.ChatMessages.Add(new ChatMessage { DonorId = donorId, SenderRole = "Admin", Message = message.Trim(), SentAtUtc = DateTime.UtcNow });
            _db.SaveChanges();
        }

        public void SendMessageToStudent(int studentId, string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return;
            _db.ChatMessages.Add(new ChatMessage { StudentId = studentId, SenderRole = "Admin", Message = message.Trim(), SentAtUtc = DateTime.UtcNow });
            _db.SaveChanges();
        }

        private List<AdminPickupAgentVm> GetPickupAgents(HttpSessionStateBase session)
        {
            var agents = session["AdminPickupAgents"] as List<AdminPickupAgentVm>;
            if (agents != null) return agents;

            agents = new List<AdminPickupAgentVm>
            {
            };
            session["AdminPickupAgents"] = agents;
            return agents;
        }

        private List<AdminPickupAssignmentVm> GetPickupAssignments(HttpSessionStateBase session)
        {
            var assignments = session["AdminPickupAssignments"] as List<AdminPickupAssignmentVm>;
            if (assignments == null)
            {
                assignments = new List<AdminPickupAssignmentVm>();
                session["AdminPickupAssignments"] = assignments;
            }
            return assignments;
        }

        private HashSet<int> GetHiddenMessageIds(HttpSessionStateBase session, string chatTarget, int personId)
        {
            var normalizedChatTarget = string.Equals(chatTarget, "Student", StringComparison.OrdinalIgnoreCase) ? "Student" : "Donor";
            var key = "AdminHiddenChatMessageIds_" + normalizedChatTarget + "_" + personId;
            var ids = session[key] as HashSet<int>;
            if (ids == null)
            {
                ids = new HashSet<int>();
                session[key] = ids;
            }
            return ids;
        }

        private void SendEmailIfConfigured(string toEmail, string subject, string body)
        {
            if (string.IsNullOrWhiteSpace(toEmail)) return;

            var smtpHost = WebConfigurationManager.AppSettings["SmtpHost"];
            var smtpFrom = WebConfigurationManager.AppSettings["SmtpFromEmail"];
            if (string.IsNullOrWhiteSpace(smtpHost) || string.IsNullOrWhiteSpace(smtpFrom))
            {
                return;
            }

            int smtpPort;
            if (!int.TryParse(WebConfigurationManager.AppSettings["SmtpPort"], out smtpPort))
            {
                smtpPort = 587;
            }

            bool enableSsl;
            if (!bool.TryParse(WebConfigurationManager.AppSettings["SmtpEnableSsl"], out enableSsl))
            {
                enableSsl = true;
            }

            var smtpUser = WebConfigurationManager.AppSettings["SmtpUser"];
            var smtpPass = WebConfigurationManager.AppSettings["SmtpPassword"];

            try
            {
                using (var client = new SmtpClient(smtpHost, smtpPort))
                {
                    client.EnableSsl = enableSsl;
                    if (!string.IsNullOrWhiteSpace(smtpUser))
                    {
                        client.Credentials = new NetworkCredential(smtpUser, smtpPass ?? string.Empty);
                    }

                    using (var mail = new MailMessage(smtpFrom, toEmail.Trim(), subject, body))
                    {
                        client.Send(mail);
                    }
                }
            }
            catch
            {
                // Email is optional. Do not break main admin actions if SMTP is not configured.
            }
        }
    }
}
