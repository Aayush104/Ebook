using BookProject.Dto;
using BookProject.IService;
using BookProject.Model;
using BookProject.Service;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace BookProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IMailService _mailService;
        private readonly IOtpService _otpService;
      

        public UserController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IMailService mailService,
            IOtpService otpService
           )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _mailService = mailService;
            _otpService = otpService;
            
        }

        [HttpPost("UserRegistration")]
        public async Task<IActionResult> RegisterUser([FromBody] RegistrationDto registrationDto)
        {
            var existingUser = await _userManager.FindByEmailAsync(registrationDto.Email);
            if (existingUser != null)
            {
                return Conflict(new ApiResponseDto
                {
                    IsSuccess = false,
                    Message = "User already exists.",
                    StatusCode = 409
                });
            }

            var user = new ApplicationUser
            {
                FullName = registrationDto.FullName,
                Email = registrationDto.Email,
                UserName = registrationDto.Email,
                PhoneNumber = registrationDto.PhoneNumber,
                Address = registrationDto.Address
            };

            var result = await _userManager.CreateAsync(user, registrationDto.Password);

            if (!result.Succeeded)
            {
                string errors = string.Join("; ", result.Errors.Select(e => e.Description));
                return BadRequest(new ApiResponseDto
                {
                    IsSuccess = false,
                    Message = $"Registration failed. {errors}",
                    StatusCode = 400
                });
            }

            await _userManager.AddToRoleAsync(user, "PublicUser");

            var otp = OtpGenerate.GenerateOtp();
            await _otpService.StoreOtpAsync(user.Id, "Registration", otp);
            await _mailService.SendMail(user.Email, user.FullName, otp);

            return Ok(new ApiResponseDto
            {
                IsSuccess = true,
                Message = "User registered successfully. Verify OTP.",
                StatusCode = 200,
                Data = user.Id
            });
        }

        [HttpPost("OtpVerification")]
        public async Task<IActionResult> VerifyOtp([FromBody] OtpDto otpVerification)
        {
            var isValid = await _otpService.verifyOtpAsync(otpVerification.UserId, otpVerification.Otp, otpVerification.Purpose);
            if (isValid)
            {
                return Ok(new ApiResponseDto
                {
                    IsSuccess = true,
                    Message = "OTP verified successfully.",
                    StatusCode = 200
                });
            }

            return BadRequest(new ApiResponseDto
            {
                IsSuccess = false,
                Message = "Invalid or expired OTP.",
                StatusCode = 400
            });
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
            {
                return NotFound(new ApiResponseDto
                {
                    IsSuccess = false,
                    Message = "User not found.",
                    StatusCode = 404
                });
            }

            var isPasswordCorrect = await _userManager.CheckPasswordAsync(user, loginDto.Password);
            if (!isPasswordCorrect)
            {
                return Unauthorized(new ApiResponseDto
                {
                    IsSuccess = false,
                    Message = "Incorrect password.",
                    StatusCode = 401
                });
            }

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault();

            if (!user.EmailConfirmed && role == "PublicUser")
            {
                var otp = OtpGenerate.GenerateOtp();
                await _otpService.StoreOtpAsync(user.Id, "Registration", otp);
                await _mailService.SendMail(user.Email, user.FullName, otp);

                return Unauthorized(new ApiResponseDto
                {
                    IsSuccess = false,
                    Message = "Email not confirmed. OTP sent again.",
                    StatusCode = 401,
                    Data = user.Id
                });
            }

            var token = GenerateToken(user, roles.ToList());


            return Ok(new ApiResponseDto
            {
                IsSuccess = true,
                Message = "Login successful.",
                StatusCode = 200,
                Data = token
            });
        }

        [HttpPost("StaffCreation")]
        public async Task<IActionResult> CreateStaff([FromBody] StaffRegistrationDto staffDto)
        {
            var existingUser = await _userManager.FindByEmailAsync(staffDto.Email);
            if (existingUser != null)
            {
                return Conflict(new ApiResponseDto
                {
                    IsSuccess = false,
                    Message = "Staff already exists.",
                    StatusCode = 409
                });
            }

            var user = new ApplicationUser
            {
                FullName = staffDto.FullName,
                Email = staffDto.Email,
                UserName = staffDto.Email,
                PhoneNumber = staffDto.PhoneNumber
                
            };

            const string staticPassword = "Itahari@2025";

            var result = await _userManager.CreateAsync(user, staticPassword);

            if (!result.Succeeded)
            {
                string errors = string.Join("; ", result.Errors.Select(e => e.Description));
                return BadRequest(new ApiResponseDto
                {
                    IsSuccess = false,
                    Message = $"Staff creation failed. {errors}",
                    StatusCode = 400
                });
            }

            await _userManager.AddToRoleAsync(user, "Staff");


            return Ok(new ApiResponseDto
            {
                IsSuccess = true,
                Message = "Staff created with default password.",
                StatusCode = 200
            });
        }

        private string GenerateToken(ApplicationUser user, List<string> roles)
        {
            var claims = new List<Claim>
    {
        new Claim("userId", user.Id.ToString()),
        new Claim("Name", user.FullName),
        new Claim("Email", user.Email),
    };

            foreach (var role in roles)
            {
                claims.Add(new Claim("Role", role));
            }

            var JWT_SECRET = Environment.GetEnvironmentVariable("JWT_SECRET");
            var JWT_ISSUER = Environment.GetEnvironmentVariable("JWT_ISSUER");
            var JWT_AUDIENCE = Environment.GetEnvironmentVariable("JWT_AUDIENCE");

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JWT_SECRET));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: JWT_ISSUER,
                audience: JWT_AUDIENCE,
                claims: claims,
                expires: DateTime.Now.AddMonths(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}
