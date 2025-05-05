using BookProject.IService;
using System.Net.Mail;
using System.Net;

namespace BookProject.Service
{
    public class MailService : IMailService
    {
        private readonly SmtpClient _smtpClient;
        private readonly string _fromEmail;

        public MailService()
        {
            _smtpClient = new SmtpClient
            {
                Host = Environment.GetEnvironmentVariable("SMTP_HOST"),
                Port = int.Parse(Environment.GetEnvironmentVariable("SMTP_PORT")),
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(
                    Environment.GetEnvironmentVariable("SMTP_USERNAME"),
                    Environment.GetEnvironmentVariable("SMTP_PASSWORD")),
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network
            };

            _fromEmail = Environment.GetEnvironmentVariable("FROM_EMAIL") ;
        }

        public async Task SendMail(string toEmail, string fullName, string Otp)
        {
            var body = $"<html><body><p>Hello {fullName},</p><p>Your OTP for verification is: <strong>{Otp}</strong></p></body></html>";

            var message = new MailMessage
            {
                From = new MailAddress(_fromEmail),
                Subject = "Get Your Registration OTP",
                Body = body,
                IsBodyHtml = true
            };

            message.To.Add(toEmail);

            try
            {
                await _smtpClient.SendMailAsync(message);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error sending email", ex);
            }
            finally
            {
                message.Dispose();
            }
        }
    }
}
