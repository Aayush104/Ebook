using BookProject.Data;
using BookProject.Dto;
using BookProject.Hubss;
using BookProject.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace BookProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnnouncementController : ControllerBase
    {
        private readonly AppDbContext _context;
      

        public AnnouncementController(AppDbContext context)
        {
            _context = context;
           
        }

        [HttpPost("SetAnnouncement")]
        public async Task<IActionResult> CreateAnnouncement([FromBody] DoAnnouncementDto announcementDto)
        {
            if (announcementDto == null)
            {
                return BadRequest(new ApiResponseDto
                {
                    IsSuccess = false,
                    Message = "Invalid announcement data.",
                    StatusCode = 400
                });
            }

            var announce = new Announce
            {
                Title = announcementDto.Title,
                Description = announcementDto.Description,
                AnnouncemnetDateTime = announcementDto.AnnouncementDateTime,
                AnnouncementEndDateTime = announcementDto.AnnouncementEndDateTime
            };

            // Save to DB
            await _context.Announces.AddAsync(announce);
            await _context.SaveChangesAsync();

            

            return StatusCode(200, new ApiResponseDto
            {
                IsSuccess = true,
                Message = "Announcement created successfully.",
                StatusCode = 200
            });
        }

        [HttpGet("active-announcements")]
        public async Task<IActionResult> GetActiveAnnouncements()
        {
            try
            {
                DateTime now = DateTime.UtcNow;

                var activeAnnouncements = await _context.Announces
                    .Where(a => a.AnnouncemnetDateTime <= now &&
                                a.AnnouncementEndDateTime >= now &&
                                a.IsAnnounced)
                    .ToListAsync();

                var response = new ApiResponseDto
                {
                    IsSuccess = true,
                    Message = "Fetched active announcements successfully.",
                    StatusCode = 200,
                    Data = activeAnnouncements
                };

                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                var errorResponse = new ApiResponseDto
                {
                    IsSuccess = false,
                    Message = $"An error occurred: {ex.Message}",
                    StatusCode = 500
                };

                return StatusCode(errorResponse.StatusCode, errorResponse);
            }
        }
    }
}
