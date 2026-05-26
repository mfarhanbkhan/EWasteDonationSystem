// Project documentation note: This file contains commented code for easier understanding.
using System.ComponentModel.DataAnnotations;

namespace EWasteDonationSystem.Models
{
    public class ChooseRoleViewModel
    {
        public string SelectedRole { get; set; }
        public string Mode { get; set; }

        public DonorLoginInput DonorLogin { get; set; }
        public DonorSignUpInput DonorSignUp { get; set; }
        public StudentLoginInput StudentLogin { get; set; }
        public StudentSignUpInput StudentSignUp { get; set; }
        public AdminLoginInput AdminLogin { get; set; }

        public ChooseRoleViewModel()
        {
            SelectedRole = "donor";
            Mode = "login";
            DonorLogin = new DonorLoginInput();
            DonorSignUp = new DonorSignUpInput();
            StudentLogin = new StudentLoginInput();
            StudentSignUp = new StudentSignUpInput();
            AdminLogin = new AdminLoginInput();
        }
    }

    public class DonorLoginInput
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class DonorSignUpInput
    {
        public string UserName { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }

    public class StudentLoginInput
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class StudentSignUpInput
    {
        public string UserName { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }

    public class AdminLoginInput
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
