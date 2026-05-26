using System;
using System.Net;
using System.Net.Mail;

namespace EWasteDonationSystem.Service
{
    public class EmailService
    {
    // "EmailFrom": "ryley.davis@ethereal.email",
    //"SmtpHost": "smtp.ethereal.email",
    //"SmtpPort": 587,
    //"SmtpUser": "ryley.davis@ethereal.email",
    //"SmtpPass": "GuGRgXRdeVZK4nycF7",
    //"DisplayName": "E Waste"

        private readonly string _smtpHost = "smtp.gmail.com";
        private readonly int _smtpPort = 587;
        private readonly string _smtpUser = "codewithhanif@gmail.com";
        private readonly string _smtpPass = "bvee vyop htvn gpdp";
        private readonly bool _enableSsl = true;
        private readonly string _fromAddress = "codewithhanif@gmail.com";
        public bool SendEmail(string toAddress, string subject, string body, bool isHtml = true)
        {
            try
            {
                using (var message = new MailMessage(_fromAddress, toAddress))
                {
                    message.Subject = subject;
                    message.Body = body;
                    message.IsBodyHtml = isHtml;

                    using (var client = new SmtpClient(_smtpHost, _smtpPort))
                    {
                        client.Credentials = new NetworkCredential(_smtpUser, _smtpPass);
                        client.EnableSsl = _enableSsl;
                        client.Send(message);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                // Log or handle the exception as needed
                Console.WriteLine("Email sending failed: " + ex.Message);
                return false;
            }
        }
    }
}