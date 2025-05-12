using BookProject.Data;
using BookProject.Dto;
using BookProject.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReviewController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet("CheckEligibilityForReview/{bookId}")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> CheckEligibilityForReview(long bookId)
        {
            var userId = User.FindFirst("userId")?.Value;
            if (userId == null)
                return Unauthorized(new ApiResponseDto { IsSuccess = false, Message = "User not found", StatusCode = 401 });

            try
            {
                var isEligible = await _context.OrderItems.AnyAsync(x =>
                    x.BookId == bookId &&
                    x.Order != null &&
                    x.Order.UserId == userId &&
                    x.Order.Status == "Completed");

                if (!isEligible)
                    return StatusCode(403, new ApiResponseDto { IsSuccess = false, Message = "You are not eligible to review this book. Please complete a purchase first.", StatusCode = 403 });

                return Ok(new ApiResponseDto { IsSuccess = true, Message = "You are eligible to review this book.", StatusCode = 200 });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDto { IsSuccess = false, Message = $"An error occurred while checking eligibility: {ex.Message}", StatusCode = 500 });
            }
        }

        [HttpPost("DoReview")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> DoReview([FromBody] PostReviewDto postReviewDto)
        {
            var userId = User.FindFirst("userId")?.Value;
            if (userId == null)
                return Unauthorized(new ApiResponseDto { IsSuccess = false, Message = "User not found", StatusCode = 401 });

            try
            {
                var review = new Review
                {
                    BookId = postReviewDto.BookId,
                    UserId = userId,
                    Comment = postReviewDto.Comment,
                    Star = postReviewDto.Star,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Reviews.Add(review);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponseDto { IsSuccess = true, Message = "Review posted successfully.", StatusCode = 200 });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDto { IsSuccess = false, Message = $"An error occurred while posting the review: {ex.Message}", StatusCode = 500 });
            }
        }

        [HttpGet("GetReview/{bookId}")]
        public async Task<IActionResult> GetReview(long bookId)
        {
            try
            {
                var reviews = await _context.Reviews.Where(x => x.BookId == bookId).ToListAsync();

                if (!reviews.Any())
                    return Ok(new ApiResponseDto { IsSuccess = true, Message = "No reviews found for this book.", StatusCode = 200, Data = new List<GetReviewDto>() });

                var reviewDtos = new List<GetReviewDto>();
                double totalStars = 0;
                int starCount = 0;
                foreach (var review in reviews)
                {
                    var user = await _userManager.FindByIdAsync(review.UserId);

                    if (review.Star.HasValue)
                    {
                        totalStars += review.Star.Value;
                        starCount++;
                    }

                    reviewDtos.Add(new GetReviewDto
                    {
                        ReviewId = review.Id,
                        UserId = review.UserId,
                        FullName = user?.FullName ?? "Unknown User",
                        Comment = review.Comment,
                        CreatedTime = review.CreatedAt
                    });
                }

                double averageStar = starCount > 0 ? totalStars / starCount : 0;

                return Ok(new ApiResponseDto { IsSuccess = true, Message = "Reviews fetched successfully.", StatusCode = 200,
                    Data = new{ reviewDtos, AverageStar = Math.Round(averageStar)} });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDto { IsSuccess = false, Message = $"An error occurred while fetching the reviews: {ex.Message}", StatusCode = 500 });
            }
        }

        [HttpDelete("DeleteReview/{reviewId}")]
        public async Task<IActionResult> DeleteReview(int reviewId)
        {
            try
            {
                var review = await _context.Reviews.FindAsync(reviewId);
                if (review == null)
                    return NotFound(new ApiResponseDto { IsSuccess = false, Message = "Review not found.", StatusCode = 404 });

                _context.Reviews.Remove(review);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponseDto { IsSuccess = true, Message = "Review deleted successfully.", StatusCode = 200 });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDto { IsSuccess = false, Message = $"An error occurred while deleting the review: {ex.Message}", StatusCode = 500 });
            }
        }
    }
}
