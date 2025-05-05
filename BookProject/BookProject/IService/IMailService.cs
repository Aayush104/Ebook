namespace BookProject.IService
{
    public interface IMailService
    {
        Task SendMail(string toEmail, string fullName, string Otp);
    }
}
