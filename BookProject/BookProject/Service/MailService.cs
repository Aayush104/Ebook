using BookProject.IService;
using System.Net.Mail;
using System.Net;
using System.Text;
using BookProject.Dto;

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
            <body style='font-family: Arial, sans-serif; color: #333; line-height: 1.6;'>
                <h2 style='color: #0056b3;'>Dear {fullName},</h2>
                <p>We are pleased to inform you that you have been successfully registered as staff.</p>
                <p>Your login details are as follows:</p>
                <ul>
                    <li><strong>Password:</strong> <span style='font-size: 18px; font-weight: bold; color: #e60000;'>Itahari@2025</span></li>
                    <li><strong>Email:</strong> {toEmail}</li>
                </ul>
                <p>Please use the provided email and password to log in to the system.</p>
                <p>If you encounter any issues, feel free to reach out to our support team.</p>
                <p>Best regards,</p>
                <p><strong>EBook</strong><br/>Support Team</p>
            </body>
        </html>";

            var message = new MailMessage
            {
                From = new MailAddress(_fromEmail),
                Subject = "Welcome to the Team! Staff Registration Successful",
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

        public async Task SendOrder(MailDto mailDto)
        {
            if (mailDto == null) throw new ArgumentNullException(nameof(mailDto));
            if (string.IsNullOrWhiteSpace(mailDto.ToEmail)) throw new ArgumentException("Recipient email is required.", nameof(mailDto.ToEmail));

            var subject = "Your EBook Order Confirmation & Claim Code";

            // Build HTML content
            var bodyBuilder = new StringBuilder();
            bodyBuilder.Append(@"
        <html>
        <head>
            <style>
                body { font-family: Arial, sans-serif; color: #333; }
                .greeting { font-size: 18px; margin-bottom: 10px; }
                .message { margin: 15px 0; }
                .order-summary { background: #f9f9f9; padding: 15px; border-radius: 5px; }
                .summary-title { margin-bottom: 10px; font-size: 16px; }
                .summary-item { display: flex; justify-content: space-between; margin-bottom: 5px; }
                .total-row { font-weight: bold; }
                .highlight-section { margin-top: 20px; padding: 10px; background-color: #e6f7ff; border-left: 4px solid #1890ff; }
                .highlight-code { font-size: 20px; font-weight: bold; margin: 10px 0; }
                .cta-button { display: inline-block; margin-top: 20px; padding: 10px 20px; background-color: #007bff; color: white; text-decoration: none; border-radius: 4px; }
            </style>
        </head>
        <body>");

            bodyBuilder.Append($"<div class='greeting'>Hi {mailDto.FullName},</div>");
            bodyBuilder.Append("<div class='message'>Thank you for your order at <strong>BookVault</strong>! We're delighted to confirm that your literary treasures are on their way to you.</div>");

            // Order Summary
            bodyBuilder.Append("<div class='order-summary'>");
            bodyBuilder.Append("<h3 class='summary-title'>Order Summary</h3>");
            bodyBuilder.Append($"<div class='summary-item'><span>Order Date:</span><span>{mailDto.OrderDate:MMMM d, yyyy}</span></div>");
            bodyBuilder.Append($"<div class='summary-item'><span>Total Books:</span><span>{mailDto.TotalBooks}</span></div>");
            bodyBuilder.Append($"<div class='summary-item'><span>Subtotal:</span><span>${mailDto.Subtotal:F2}</span></div>");
            bodyBuilder.Append($"<div class='summary-item'><span>Discount Applied:</span><span>${mailDto.Discount:F2}</span></div>");
            bodyBuilder.Append($"<div class='summary-item total-row'><span>Final Amount:</span><span>${mailDto.FinalAmount:F2}</span></div>");
            bodyBuilder.Append("</div>");

            // Claim Code
            bodyBuilder.Append("<div class='highlight-section'>");
            bodyBuilder.Append("<p><strong>Your Unique Claim Code</strong></p>");
            bodyBuilder.Append($"<div class='highlight-code'>{mailDto.ClaimCode}</div>");
            bodyBuilder.Append("<p>Please keep this code safe. You may need it to verify your order in future communications.</p>");
            bodyBuilder.Append("</div>");

            bodyBuilder.Append("<a href='#' class='cta-button'>Track Your Order</a>");
            bodyBuilder.Append("<div class='message'>Happy reading!<br>The Ebook Team</div>");
            bodyBuilder.Append("</body></html>");

            var body = bodyBuilder.ToString();

            var message = new MailMessage
            {
                From = new MailAddress(_fromEmail, "EBook"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            message.To.Add(mailDto.ToEmail);

            try
            {
                await _smtpClient.SendMailAsync(message);
            }
            catch (SmtpException smtpEx)
            {
                throw new InvalidOperationException("SMTP error occurred while sending the email.", smtpEx);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while sending the order confirmation email.", ex);
            }
            finally
            {
                message.Dispose();
            }
        }


    }
}
