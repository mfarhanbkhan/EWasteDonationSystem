using EWasteDonationSystem.Models;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Configuration;
using static System.Net.WebRequestMethods;

namespace EWasteDonationSystem.Service
{
    /// <summary>
    /// Handles all account-related business logic so the controller stays thin.
    /// </summary>
    public class AccountService
    {
        private readonly AppDbContext _db;
        private readonly string[] _adminEmails;
        private readonly string[] _adminUserNames;
        private readonly string[] _adminPasswords;
        private readonly string _primaryAdminEmail;

        private const string PasswordHashPrefix = "PBKDF2$";
        private const int PasswordSaltSize = 16;
        private const int PasswordKeySize = 32;
        private const int PasswordIterations = 10000;
        private readonly EmailService _emailService;
        public AccountService(AppDbContext db)
        {
            _db = db;
            _adminEmails = LoadConfiguredList("AdminEmails", "9448845@gmail.com,admin@gmail.com");
            _adminUserNames = LoadConfiguredList("AdminUserNames", "admin");
            _adminPasswords = LoadConfiguredList("AdminPasswords", "Admin@123,Secure@2026,78692,admin123");
            _primaryAdminEmail = _adminEmails.FirstOrDefault() ?? "admin@localhost";
            _emailService = new EmailService();
        }

        /// <summary>
        /// Validate admin credentials and update session state.
        /// </summary>
        public bool TryAdminLogin(HttpSessionStateBase session, string email, string password, out string errorMessage)
        {
            var trimmedEmail = (email ?? string.Empty).Trim();
            var trimmedPassword = (password ?? string.Empty).Trim();

            var isValidAdminIdentity = _adminEmails.Any(x => string.Equals(trimmedEmail, x, StringComparison.OrdinalIgnoreCase))
                || _adminUserNames.Any(x => string.Equals(trimmedEmail, x, StringComparison.OrdinalIgnoreCase));

            var recoveredAdminPassword = (Convert.ToString(session["RecoveredAdminPassword"]) ?? string.Empty).Trim();
            var isValidAdminPassword = (!string.IsNullOrWhiteSpace(recoveredAdminPassword) && string.Equals(trimmedPassword, recoveredAdminPassword, StringComparison.Ordinal))
                || _adminPasswords.Any(x => string.Equals(trimmedPassword, x.Trim(), StringComparison.OrdinalIgnoreCase));

            if (!isValidAdminIdentity || !isValidAdminPassword)
            {
                errorMessage = "Invalid email and password";
                return false;
            }

            session["AdminLoggedIn"] = true;
            session["AdminName"] = "Administrator";
            session["AdminEmail"] = trimmedEmail;
            errorMessage = null;
            return true;
        }

        /// <summary>
        /// Change the admin password stored in session-based recovery flow.
        /// </summary>
        public bool TryRecoverAdminPassword(HttpSessionStateBase session, string email, string newPassword, string confirmPassword, out string message)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword))
            {
                message = "Please fill all admin forgotten password fields.";
                return false;
            }

            var normalizedAdminEmail = email.Trim();
            var adminExists = _adminEmails.Any(x => string.Equals(normalizedAdminEmail, x, StringComparison.OrdinalIgnoreCase))
                || _adminUserNames.Any(x => string.Equals(normalizedAdminEmail, x, StringComparison.OrdinalIgnoreCase));

            if (!adminExists)
            {
                message = "Admin email not found.";
                return false;
            }

            if (!string.Equals(newPassword, confirmPassword, StringComparison.Ordinal))
            {
                message = "Password and confirm password do not match.";
                return false;
            }

            if (!IsStrongPassword(newPassword))
            {
                message = "Password must include one uppercase letter, one lowercase letter, one digit and one special character.";
                return false;
            }

            session["RecoveredAdminPassword"] = newPassword.Trim();
            session["RecoveredAdminEmail"] = _primaryAdminEmail;
            message = "Admin password recovered successfully. Please login with your new password.";
            return true;
        }

        /// <summary>
        /// Create a donor account if email is unique across the system.
        /// </summary>
        public bool TryDonorSignUp(string userName, string name, string email, string password, string confirmPassword, out string message)
        {
            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                message = "Please fill all donor sign up fields.";
                return false;
            }

            if (!string.Equals(password, confirmPassword, StringComparison.Ordinal))
            {
                message = "Password and confirm password do not match.";
                return false;
            }

            if (!IsStrongPassword(password))
            {
                message = "Password must include one uppercase letter, one lowercase letter, one digit and one special character.";
                return false;
            }

            var normalizedEmail = (email ?? string.Empty).Trim();
            var gmailAlreadyExists = EmailExists(normalizedEmail);
            if (gmailAlreadyExists)
            {
                message = "Gmail already existed";
                return false;
            }

            var otp = GenerateOtp();

            var donor = new Donor
            {
                Phone = userName.Trim(),
                FullName = name.Trim(),
                Email = normalizedEmail,
                Password = HashPassword(password.Trim()),
                Status = ApprovalStatus.Pending,
                EmailOtp = otp,
                OtpExpiresAt = DateTime.UtcNow.AddMinutes(10), // OTP valid for 10 minutes
                IsEmailVerified = false,
                CreatedAtUtc = DateTime.UtcNow
            };

            _db.Donors.Add(donor);
            _db.SaveChanges();

            // Send OTP email
            bool isSent = _emailService.SendEmail(
                toAddress: normalizedEmail,
                subject: "Your OTP for Donor Sign-Up",
                body: $"Your OTP is: <b>{otp}</b>. It is valid for 10 minutes.",
                isHtml: true
            );

            message = "Donor sign up successful. Please login now.";
            return true;
        }

        /// <summary>
        /// Generate and email an OTP for donor password reset.
        /// </summary>
        public bool TrySendDonorPasswordResetOtp(string email, out string message)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                message = "Please enter your donor email.";
                return false;
            }

            var normalizedEmail = email.Trim();
            var donor = _db.Donors.FirstOrDefault(x => x.Email != null && x.Email.Equals(normalizedEmail, StringComparison.OrdinalIgnoreCase));
            if (donor == null)
            {
                message = "Donor email not found.";
                return false;
            }

            if (!donor.IsEmailVerified)
            {
                message = "Email not verified. Please verify your account before resetting your password.";
                return false;
            }

            var otp = GenerateOtp();
            donor.EmailOtp = otp;
            donor.OtpExpiresAt = DateTime.UtcNow.AddMinutes(10);
            _db.SaveChanges();

            _emailService.SendEmail(
                toAddress: normalizedEmail,
                subject: "Your OTP for Donor Password Reset",
                body: $"Your password reset OTP is: <b>{otp}</b>. It is valid for 10 minutes.",
                isHtml: true
            );

            message = "OTP sent to your email.";
            return true;
        }

        /// <summary>
        /// Update donor password by email after OTP verification.
        /// </summary>
        public bool TryRecoverDonorPassword(string email, string otp, string newPassword, string confirmPassword, out string message)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(otp) || string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword))
            {
                message = "Please fill all donor forgotten password fields.";
                return false;
            }

            if (!string.Equals(newPassword, confirmPassword, StringComparison.Ordinal))
            {
                message = "Password and confirm password do not match.";
                return false;
            }

            if (!IsStrongPassword(newPassword))
            {
                message = "Password must include one uppercase letter, one lowercase letter, one digit and one special character.";
                return false;
            }

            var normalizedEmail = (email ?? string.Empty).Trim();
            var donor = _db.Donors.FirstOrDefault(x => x.Email != null && x.Email.Equals(normalizedEmail, StringComparison.OrdinalIgnoreCase));
            if (donor == null)
            {
                message = "Donor email not found.";
                return false;
            }

            if (donor.OtpExpiresAt == null || donor.OtpExpiresAt < DateTime.UtcNow)
            {
                message = "OTP expired. Please request a new one.";
                return false;
            }

            if (!string.Equals(donor.EmailOtp, otp.Trim(), StringComparison.Ordinal))
            {
                message = "Invalid OTP.";
                return false;
            }

            donor.Password = HashPassword(newPassword.Trim());
            donor.EmailOtp = null;
            donor.OtpExpiresAt = null;
            _db.SaveChanges();
            message = "Donor password recovered successfully. Please login now.";
            return true;
        }

        /// <summary>
        /// Authenticate donor and store session information.
        /// </summary>
        public bool TryDonorLogin(HttpSessionStateBase session, string email, string password, out string message)
        {
            var trimmedEmail = (email ?? string.Empty).Trim();
            var trimmedPassword = (password ?? string.Empty).Trim();
            var donor = _db.Donors.FirstOrDefault(x => x.Email != null && x.Email.Equals(trimmedEmail, StringComparison.OrdinalIgnoreCase));
            if (donor == null)
            {
                message = "Invalid email and password";
                return false;
            }

            string upgradedDonorPassword;
            if (!TryVerifyAndUpgradePassword(donor.Password, trimmedPassword, out upgradedDonorPassword))
            {
                message = "Invalid email and password";
                return false;
            }

            if (!donor.IsEmailVerified)
            {
                message = "Email not verified, Please verify your email and try again!";
                return false;
            }

            if (!string.IsNullOrEmpty(upgradedDonorPassword))
            {
                donor.Password = upgradedDonorPassword;
                _db.SaveChanges();
            }

            session["DonorId"] = donor.Id;
            session["DonorName"] = donor.FullName;
            session["DonorEmail"] = donor.Email;

            session["AdminLoggedIn"] = false;

            message = null;
            return true;
        }

        /// <summary>
        /// Create a student account if email is unique across the system.
        /// </summary>
        public bool TryStudentSignUp(string userName, string name, string email, string password, string confirmPassword, out string message)
        {
            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                message = "Please fill all student sign up fields.";
                return false;
            }

            if (!string.Equals(password, confirmPassword, StringComparison.Ordinal))
            {
                message = "Password and confirm password do not match.";
                return false;
            }

            if (!IsStrongPassword(password))
            {
                message = "Password must include one uppercase letter, one lowercase letter, one digit and one special character.";
                return false;
            }

            var normalizedEmail = (email ?? string.Empty).Trim();
            var gmailAlreadyExists = EmailExists(normalizedEmail);
            if (gmailAlreadyExists)
            {
                message = "Gmail already existed";
                return false;
            }

            var otp = GenerateOtp();
            _db.Students.Add(new Student
            {
                Phone = userName.Trim(),
                FullName = name.Trim(),
                Email = normalizedEmail,
                Password = HashPassword(password.Trim()),
                Status = ApprovalStatus.Pending,
                EmailOtp = otp,
                OtpExpiresAt = DateTime.UtcNow.AddMinutes(10), // OTP valid for 10 minutes
                IsEmailVerified = false,
                CreatedAtUtc = DateTime.UtcNow
            });
            _db.SaveChanges();

            _emailService.SendEmail(
                toAddress: normalizedEmail,
                subject: "Your OTP for Student Sign-Up",
                body: $"Your OTP is: <b>{otp}</b>. It is valid for 10 minutes.",
                isHtml: true
            );

            message = "Student sign up successful. Please verify your email before logging in.";
            return true;
        }

        /// <summary>
        /// Generate and email an OTP for student password reset.
        /// </summary>
        public bool TrySendStudentPasswordResetOtp(string email, out string message)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                message = "Please enter your email.";
                return false;
            }

            var normalizedEmail = email.Trim();
            var student = _db.Students.FirstOrDefault(x => x.Email != null && x.Email.Equals(normalizedEmail, StringComparison.OrdinalIgnoreCase));
            if (student == null)
            {
                message = "Student email not found.";
                return false;
            }

            if (!student.IsEmailVerified)
            {
                message = "Email not verified. Please verify your account before resetting your password.";
                return false;
            }

            var otp = GenerateOtp();
            student.EmailOtp = otp;
            student.OtpExpiresAt = DateTime.UtcNow.AddMinutes(10);
            _db.SaveChanges();

            _emailService.SendEmail(
                toAddress: normalizedEmail,
                subject: "Your OTP for Student Password Reset",
                body: $"Your password reset OTP is: <b>{otp}</b>. It is valid for 10 minutes.",
                isHtml: true
            );

            message = "OTP sent to your email.";
            return true;
        }

        /// <summary>
        /// Update student password by email after OTP verification.
        /// </summary>
        public bool TryRecoverStudentPassword(string email, string otp, string newPassword, string confirmPassword, out string message)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(otp) || string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword))
            {
                message = "Please fill all student forgotten password fields.";
                return false;
            }

            if (!string.Equals(newPassword, confirmPassword, StringComparison.Ordinal))
            {
                message = "Password and confirm password do not match.";
                return false;
            }

            if (!IsStrongPassword(newPassword))
            {
                message = "Password must include one uppercase letter, one lowercase letter, one digit and one special character.";
                return false;
            }

            var normalizedEmail = (email ?? string.Empty).Trim();
            var student = _db.Students.FirstOrDefault(x => x.Email != null && x.Email.Equals(normalizedEmail, StringComparison.OrdinalIgnoreCase));
            if (student == null)
            {
                message = "Student email not found.";
                return false;
            }

            if (student.OtpExpiresAt == null || student.OtpExpiresAt < DateTime.UtcNow)
            {
                message = "OTP expired. Please request a new one.";
                return false;
            }

            if (!string.Equals(student.EmailOtp, otp.Trim(), StringComparison.Ordinal))
            {
                message = "Invalid OTP.";
                return false;
            }

            student.Password = HashPassword(newPassword.Trim());
            student.EmailOtp = null;
            student.OtpExpiresAt = null;
            _db.SaveChanges();
            message = "Student password recovered successfully. Please login now.";
            return true;
        }

        /// <summary>
        /// Authenticate student and store session information.
        /// </summary>
        public bool TryStudentLogin(HttpSessionStateBase session, string email, string password, out string message)
        {
            var trimmedEmail = (email ?? string.Empty).Trim();
            var trimmedPassword = (password ?? string.Empty).Trim();
            var student = _db.Students.FirstOrDefault(x => x.Email != null && x.Email.Equals(trimmedEmail, StringComparison.OrdinalIgnoreCase));
            if (student == null)
            {
                message = "Invalid email and password";
                return false;
            }

            string upgradedStudentPassword;
            if (!TryVerifyAndUpgradePassword(student.Password, trimmedPassword, out upgradedStudentPassword))
            {
                message = "Invalid email and password";
                return false;
            }

            if (!student.IsEmailVerified)
            {
                message = "Email not verified. Please verify your email and try again!";
                return false;
            }

            if (!string.IsNullOrEmpty(upgradedStudentPassword))
            {
                //student.Password = upgradedStudentPassword;
                _db.SaveChanges();
            }

            session["StudentId"] = student.Id;
            session["StudentName"] = student.FullName;
            session["StudentEmail"] = student.Email;

            session["AdminLoggedIn"] = false;

            message = null;
            return true;
        }

        private bool EmailExists(string email)
        {
            return _adminEmails.Any(x => string.Equals(email, x, StringComparison.OrdinalIgnoreCase))
                || _db.Donors.Any(x => x.Email != null && x.Email.Equals(email, StringComparison.OrdinalIgnoreCase))
                || _db.Students.Any(x => x.Email != null && x.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        }

        private static string[] LoadConfiguredList(string key, string fallbackCsv)
        {
            var rawValue = WebConfigurationManager.AppSettings[key];
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                rawValue = fallbackCsv;
            }

            return rawValue
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        private static string HashPassword(string password)
        {
            var salt = new byte[PasswordSaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            using (var deriveBytes = new Rfc2898DeriveBytes(password, salt, PasswordIterations))
            {
                var key = deriveBytes.GetBytes(PasswordKeySize);
                return PasswordHashPrefix + PasswordIterations + "$" + Convert.ToBase64String(salt) + "$" + Convert.ToBase64String(key);
            }
        }

        private static bool TryVerifyAndUpgradePassword(string storedValue, string enteredPassword, out string upgradedPasswordHash)
        {
            upgradedPasswordHash = null;
            var safeStoredValue = storedValue ?? string.Empty;

            if (safeStoredValue.StartsWith(PasswordHashPrefix, StringComparison.Ordinal))
            {
                return VerifyHashedPassword(safeStoredValue, enteredPassword);
            }

            var isLegacyPasswordMatch = string.Equals(safeStoredValue, enteredPassword, StringComparison.Ordinal);
            if (!isLegacyPasswordMatch)
            {
                return false;
            }

            upgradedPasswordHash = HashPassword(enteredPassword);
            return true;
        }

        private static bool VerifyHashedPassword(string storedHash, string enteredPassword)
        {
            var parts = storedHash.Split('$');
            if (parts.Length != 4)
            {
                return false;
            }

            int iterations;
            if (!int.TryParse(parts[1], out iterations) || iterations <= 0)
            {
                return false;
            }

            byte[] salt;
            byte[] expectedKey;
            try
            {
                salt = Convert.FromBase64String(parts[2]);
                expectedKey = Convert.FromBase64String(parts[3]);
            }
            catch (FormatException)
            {
                return false;
            }

            using (var deriveBytes = new Rfc2898DeriveBytes(enteredPassword, salt, iterations))
            {
                var actualKey = deriveBytes.GetBytes(expectedKey.Length);
                return FixedTimeEquals(expectedKey, actualKey);
            }
        }

        private static bool FixedTimeEquals(byte[] left, byte[] right)
        {
            if (left == null || right == null || left.Length != right.Length)
            {
                return false;
            }

            var diff = 0;
            for (var i = 0; i < left.Length; i++)
            {
                diff |= left[i] ^ right[i];
            }

            return diff == 0;
        }

        private static bool IsStrongPassword(string password)
        {
            var value = password ?? string.Empty;
            if (value.Length < 8) return false;
            return Regex.IsMatch(value, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,}$");
        }

        private string GenerateOtp()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString(); // 6-digit OTP
        }

        public bool VerifyOtp(string email, string otp, out string message)
        {
            var donor = _db.Donors.FirstOrDefault(x => x.Email != null && x.Email.Equals(email.Trim(), StringComparison.OrdinalIgnoreCase));
            if (donor == null)
            {
                message = "Email not found.";
                return false;
            }

            if (donor.IsEmailVerified)
            {
                message = "Email already verified.";
                return false;
            }

            if (donor.OtpExpiresAt < DateTime.UtcNow)
            {
                message = "OTP expired. Please request a new one.";
                return false;
            }

            if (donor.EmailOtp != otp)
            {
                message = "Invalid OTP.";
                return false;
            }

            donor.IsEmailVerified = true;
            donor.Status = ApprovalStatus.Approved; // Or whatever you use for active accounts
            donor.EmailOtp = null;
            donor.OtpExpiresAt = null;
            _db.SaveChanges();

            message = "Email verified successfully!";
            return true;
        }

        public bool VerifyStudentOtp(string email, string otp, out string message)
        {
            var student = _db.Students.FirstOrDefault(x => x.Email != null && x.Email.Equals(email.Trim(), StringComparison.OrdinalIgnoreCase));
            if (student == null)
            {
                message = "Email not found.";
                return false;
            }

            if (student.IsEmailVerified)
            {
                message = "Email already verified.";
                return false;
            }

            if (student.OtpExpiresAt == null || student.OtpExpiresAt < DateTime.UtcNow)
            {
                message = "OTP expired. Please request a new one.";
                return false;
            }

            if (!string.Equals(student.EmailOtp, otp.Trim(), StringComparison.Ordinal))
            {
                message = "Invalid OTP.";
                return false;
            }

            student.IsEmailVerified = true;
            student.Status = ApprovalStatus.Approved;
            student.EmailOtp = null;
            student.OtpExpiresAt = null;
            _db.SaveChanges();

            message = "Email verified successfully!";
            return true;
        }
    }
}
