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

            _fromEmail = Environment.GetEnvironmentVariable("FROM_EMAIL");
        }

        public async Task SendMail(string toEmail, string fullName, string otp)
        {
            var body = $@"
                <html>
                    <body style='font-family: Arial, sans-serif; color: #333;'>
                        <h2 style='color: #0056b3;'>Hello {fullName},</h2>
                        <p>Your OTP for verification is:</p>
                        <p style='font-size: 18px; font-weight: bold; color: #e60000;'>{otp}</p>
                        <p>Thank you for using our service.</p>
                    </body>
                </html>";

            var message = new MailMessage
            {
                From = new MailAddress(_fromEmail),
                Subject = "Your OTP for Registration",
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
                throw new InvalidOperationException("Error sending OTP email", ex);
            }
            finally
            {
                message.Dispose();
            }
        }

        public async Task SendStaffMail(string toEmail, string fullName)
        {
            var body = $@"
                <html>
                    <body style='font-family: Arial, sans-serif; color: #333;'>
                        <h2 style='color: #0056b3;'>Hello {fullName},</h2>
                        <p>You have been registered as staff.</p>
                        <p>Your temporary password is: 
                            <span style='font-size: 18px; font-weight: bold; color: #e60000;'>Itahari@2025</span>
                        </p>
                        <p>Please change your password after first login.</p>
                    </body>
                </html>";

            var message = new MailMessage
            {
                From = new MailAddress(_fromEmail),
                Subject = "Staff Registration",
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
                throw new InvalidOperationException("Error sending staff registration email", ex);
            }
            finally
            {
                message.Dispose();
            }
        }
    }
}
