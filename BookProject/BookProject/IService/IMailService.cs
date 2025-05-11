using BookProject.Dto;

namespace BookProject.IService
{
    public interface IMailService
    {
        Task SendMail(string toEmail, string fullName, string Otp);
        Task SendStaffMail(string toEmail, string fullName);
        Task SendOrder(MailDto mailDto);
    }
}
