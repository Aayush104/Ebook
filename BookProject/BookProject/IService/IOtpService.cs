namespace BookProject.IService
{
    public interface IOtpService
    {
        Task StoreOtpAsync(string userId, string purpose, string otp);
        Task<bool> verifyOtpAsync(string UserId, string Otp, string Purpose);
    }
}
