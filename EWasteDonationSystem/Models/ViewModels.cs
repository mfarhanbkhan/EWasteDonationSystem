using System;
using System.Collections.Generic;

namespace EWasteDonationSystem.Models
{
    /// <summary>
    /// View models used by dashboard pages to keep UI clean.
    /// </summary>
    public class DonorDashboardVm
    {
        public DonorDashboardVm()
        {
            Donor = new Donor();
            DonationItem = new DonationItem();

            Donors = new List<Donor>();
            LatestItems = new List<DonationItem>();
            Chat = new List<ChatMessage>();
        }

        public Donor Donor { get; set; }
        public DonationItem DonationItem { get; set; }

        public List<Donor> Donors { get; set; }
        public List<DonationItem> LatestItems { get; set; }

        public List<ChatMessage> Chat { get; set; }
    }

    public class StudentDashboardVm
    {
        public StudentDashboardVm()
        {
            Student = new Student();
            Application = new StudentApplication();

            Students = new List<Student>();
            LatestApplications = new List<StudentApplication>();
            Chat = new List<ChatMessage>();
        }

        public Student Student { get; set; }
        public StudentApplication Application { get; set; }

        public List<Student> Students { get; set; }
        public List<StudentApplication> LatestApplications { get; set; }

        public List<ChatMessage> Chat { get; set; }
    }

    public class AdminDashboardVm
    {
        public AdminDashboardVm()
        {
            Donors = new List<Donor>();
            Students = new List<Student>();
            DonationItems = new List<DonationItem>();
            StudentApplications = new List<StudentApplication>();
            PickupAgents = new List<AdminPickupAgentVm>();
            PickupAssignments = new List<AdminPickupAssignmentVm>();
            PreviewMessages = new List<ChatMessage>();
        }

        public int TotalDonors { get; set; }
        public int TotalStudents { get; set; }
        public int PendingDonors { get; set; }
        public int PendingStudents { get; set; }
        public int TotalPickupAssignments { get; set; }
        public int RecentDonorCount { get; set; }
        public int RecentStudentCount { get; set; }

        public List<Donor> Donors { get; set; }
        public List<Student> Students { get; set; }
        public List<DonationItem> DonationItems { get; set; }
        public List<StudentApplication> StudentApplications { get; set; }
        public List<AdminPickupAgentVm> PickupAgents { get; set; }
        public List<AdminPickupAssignmentVm> PickupAssignments { get; set; }
        public string SelectedDonorId { get; set; }
        public string SelectedAgentCode { get; set; }
        public string SelectedChatTarget { get; set; }
        public string SelectedChatPerson { get; set; }
        public string SelectedChatPersonId { get; set; }
        public List<ChatMessage> PreviewMessages { get; set; }
    }

    public class AdminPickupAgentVm
    {
        public string AgentCode { get; set; }
        public string AgentName { get; set; }
        public string Phone { get; set; }
        public string Area { get; set; }
        public string Email { get; set; }
    }

    public class AdminPickupAssignmentVm
    {
        public string DonorName { get; set; }
        public string AgentName { get; set; }
        public string Location { get; set; }
        public DateTime AssignedAtUtc { get; set; }
    }
}
