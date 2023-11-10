using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using FileMover;

namespace TVEpisodeChecker
{
    internal class Email
    {
        public void SendEmail(string gName, string gKey, string body)
        {
            // Create a MailMessage object
            MailMessage mailMessage = new MailMessage(gName, gName);
            mailMessage.Subject = "TV Check";
            mailMessage.Body = body;

            // Create a SmtpClient object
            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com");

            smtpClient.Port = 587; // Gmail SMTP port
            smtpClient.Credentials = new NetworkCredential(gName, gKey);
            smtpClient.EnableSsl = true; // Enable SSL/TLS

            try
            {
                // Send the email
                smtpClient.Send(mailMessage);
                Console.WriteLine("Email sent successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error sending email: " + ex.Message);
            }
        }
    }
}