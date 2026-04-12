using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System.Threading.Tasks;

namespace SmartSocietyMVC.Services
{
    public class EmailSender : IEmailSender
    {
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("Smart Society Admin", "gamingajjubhaitotal797@gmail.com"));
            emailMessage.To.Add(new MailboxAddress("", email));
            emailMessage.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = htmlMessage };
            emailMessage.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            try
            {
                // Connect to Gmail SMTP server
                await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                
                // Authenticate using App Password
                await client.AuthenticateAsync("gamingajjubhaitotal797@gmail.com", "sjkk pchh kbdm fpdf");

                await client.SendAsync(emailMessage);
            }
            finally
            {
                await client.DisconnectAsync(true);
            }
        }
    }
}
