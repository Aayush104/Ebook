using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BookProject.Data; 
using BookProject.Model; // Your ApplicationUser and Otp models
using System;
using System.Linq;
using System.Threading.Tasks;
using BookProject.IService;
using static System.Net.WebRequestMethods;



namespace BookProject.Service
{
    public class OtpService : IOtpService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OtpService(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task StoreOtpAsync(string userId, string purpose, string otp)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            if (string.IsNullOrWhiteSpace(purpose))
                throw new ArgumentNullException(nameof(purpose));

            if (string.IsNullOrWhiteSpace(otp))
                throw new ArgumentNullException(nameof(otp));

            var otpEntity = new Otp
            {
                UserId = userId,
                Purpose = purpose,
                IsOtp = otp,
                IsUsed = false,
                ExpiresAt = DateTime.UtcNow.AddMinutes(5)
            };

            await _context.Otps.AddAsync(otpEntity);
            await _context.SaveChangesAsync();
        }

       
        public async Task<bool> verifyOtpAsync(string UserId, string Otp, string Purpose)
        {
            var latestOtp = await _context.Otps
                .Where(x => x.UserId == UserId && x.Purpose == Purpose && !x.IsUsed)
                .OrderByDescending(x => x.ExpiresAt)
                .FirstOrDefaultAsync();

            if (latestOtp == null || latestOtp.IsUsed || DateTime.UtcNow >= latestOtp.ExpiresAt)
                return false;

            if (latestOtp.IsOtp == Otp)
            {
                latestOtp.IsUsed = true;
                _context.Otps.Update(latestOtp);
                await _context.SaveChangesAsync();

                var user = await _userManager.FindByIdAsync(UserId);
                if (user != null && Purpose == "Registration")
                {
                    user.EmailConfirmed = true;
                    var result = await _userManager.UpdateAsync(user);
                    return result.Succeeded;
                }

                return true;
            }

            return false;
        }
    }
}
