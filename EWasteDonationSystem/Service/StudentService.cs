// Project documentation note: This file contains commented code for easier understanding.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EWasteDonationSystem.Models;

namespace EWasteDonationSystem.Service
{
    /// <summary>
    /// Handles student dashboard data loading, application submission, and chat actions.
    /// </summary>
    public class StudentService
    {
        private readonly AppDbContext _db;

        public StudentService(AppDbContext db)
        {
            _db = db;
        }

        public StudentDashboardVm BuildDashboard(HttpSessionStateBase session, int? id)
        {
            var vm = new StudentDashboardVm();
            vm.Students = _db.Students.OrderByDescending(x => x.Id).ToList();

            if (!id.HasValue && session["StudentId"] != null)
            {
                id = (int)session["StudentId"];
            }
            else if (!id.HasValue && session["AdminLoggedIn"] is bool && (bool)session["AdminLoggedIn"])
            {
                var latestStudent = vm.Students.FirstOrDefault();
                if (latestStudent != null) id = latestStudent.Id;
            }

            if (id.HasValue)
            {
                vm.Student = _db.Students.Find(id.Value) ?? new Student();
                var hiddenMessageIds = GetHiddenMessageIds(session, id.Value);
                vm.Chat = _db.ChatMessages.Where(m => m.StudentId == id.Value && !hiddenMessageIds.Contains(m.Id)).OrderBy(m => m.SentAtUtc).ToList();
                vm.LatestApplications = _db.StudentApplications.Where(a => a.StudentId == id.Value).OrderByDescending(a => a.Id).Take(20).ToList();
                vm.Application.StudentId = id.Value;
            }
            return vm;
        }

        public Student GetDetail(HttpSessionStateBase session, int? id)
        {
            if (!id.HasValue && session["StudentId"] != null)
            {
                id = (int)session["StudentId"];
            }
            if (!id.HasValue) return null;

            var student = _db.Students.Find(id.Value);
            if (student == null) return null;
            student.Applications = _db.StudentApplications.Where(x => x.StudentId == student.Id).OrderByDescending(x => x.Id).ToList();
            student.ChatMessages = _db.ChatMessages.Where(x => x.StudentId == student.Id).OrderBy(x => x.SentAtUtc).ToList();
            return student;
        }

        public bool SaveStudentPersonForm(HttpSessionStateBase session, StudentDashboardVm vm, out int studentId, out string message)
        {
            studentId = session["StudentId"] == null ? 0 : (int)session["StudentId"];
            if (studentId == 0)
            {
                message = "Please login as student first.";
                return false;
            }

            var existing = _db.Students.Find(studentId);
            if (existing == null)
            {
                message = "Student not found.";
                return false;
            }

            existing.FullName = (vm.Student.FullName ?? string.Empty).Trim();
            existing.Phone = (vm.Student.Phone ?? string.Empty).Trim();
            existing.Institute = (vm.Student.Institute ?? string.Empty).Trim();
            existing.City = (vm.Student.City ?? string.Empty).Trim();
            existing.Address = (vm.Student.Address ?? string.Empty).Trim();
            session["StudentName"] = existing.FullName;

            var itemsNeeded = vm.Application != null ? (vm.Application.ItemsNeeded ?? string.Empty).Trim() : string.Empty;
            var reason = vm.Application != null ? (vm.Application.Reason ?? string.Empty).Trim() : string.Empty;
            if (string.IsNullOrWhiteSpace(existing.FullName))
            {
                message = "Student/Needy Name is required.";
                return false;
            }
            if (string.IsNullOrWhiteSpace(itemsNeeded))
            {
                message = "Item Needed is required.";
                return false;
            }

            _db.StudentApplications.Add(new StudentApplication
            {
                StudentId = studentId,
                ItemsNeeded = itemsNeeded,
                Reason = reason,
                CreatedAtUtc = DateTime.UtcNow
            });
            _db.SaveChanges();
            message = "Student/Needy Person form submitted successfully.";
            return true;
        }

        public bool DeleteMessageForMe(HttpSessionStateBase session, int messageId, out int studentId)
        {
            studentId = session["StudentId"] == null ? 0 : (int)session["StudentId"];
            if (studentId == 0) return false;
            var currentStudentId = studentId;
            var message = _db.ChatMessages.FirstOrDefault(m => m.Id == messageId && m.StudentId == currentStudentId);
            if (message != null)
            {
                var hiddenIds = GetHiddenMessageIds(session, studentId);
                hiddenIds.Add(messageId);
                session["StudentHiddenChatMessageIds_" + studentId] = hiddenIds;
            }
            return true;
        }

        public bool DeleteMessageForEveryone(HttpSessionStateBase session, int messageId, out int studentId)
        {
            studentId = session["StudentId"] == null ? 0 : (int)session["StudentId"];
            if (studentId == 0) return false;
            var currentStudentId = studentId;
            var message = _db.ChatMessages.FirstOrDefault(m => m.Id == messageId && m.StudentId == currentStudentId && m.SenderRole == "Student");
            if (message != null)
            {
                _db.ChatMessages.Remove(message);
                _db.SaveChanges();
            }
            return true;
        }

        public bool SendMessage(HttpSessionStateBase session, string messageText, out int studentId)
        {
            studentId = session["StudentId"] == null ? 0 : (int)session["StudentId"];
            if (studentId == 0 || string.IsNullOrWhiteSpace(messageText)) return false;
            _db.ChatMessages.Add(new ChatMessage { StudentId = studentId, SenderRole = "Student", Message = messageText.Trim(), SentAtUtc = DateTime.UtcNow });
            _db.SaveChanges();
            return true;
        }

        private HashSet<int> GetHiddenMessageIds(HttpSessionStateBase session, int studentId)
        {
            var key = "StudentHiddenChatMessageIds_" + studentId;
            var ids = session[key] as HashSet<int>;
            if (ids == null)
            {
                ids = new HashSet<int>();
                session[key] = ids;
            }
            return ids;
        }
    }
}
