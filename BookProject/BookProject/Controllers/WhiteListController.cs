using BookProject.Data;
using BookProject.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using BookProject.Model;
using Microsoft.EntityFrameworkCore;


namespace BookProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WishlistController : ControllerBase
    {
        private readonly AppDbContext _context;

        public WishlistController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("AddToBookMark/{bookId:long}")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> AddToWishlist(long bookId)
        {
            var userId = HttpContext.User.FindFirst("userId")?.Value;
            if (userId == null)
                return Unauthorized("User not found");

            var bookmark = new Bookmark
            {
                UserId = userId,
                BookId = bookId,
                BookmarkedOn = DateTime.UtcNow
            };

            await _context.BookMarks.AddAsync(bookmark);
            await _context.SaveChangesAsync();

            return StatusCode(201, new ApiResponseDto
            {
                IsSuccess = true,
                Message = "Bookmark added successfully.",
                StatusCode = 201
            });
        }

        [HttpGet("GetBookMarks")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> GetWishlist()
        {
            var userId = HttpContext.User.FindFirst("userId")?.Value;
            if (userId == null)
                return Unauthorized("User not found");

            var bookmarks = await _context.BookMarks
                .Where(b => b.UserId == userId)
                .Include(b => b.Book)
                .ToListAsync();

            if (!bookmarks.Any())
            {
                return NotFound(new ApiResponseDto
                {
                    IsSuccess = false,
                    Message = "No bookmarks found.",
                    StatusCode = 404
                });
            }

            var result = bookmarks.Select(b => new
            {
                b.Id,
                b.BookId,
                b.BookmarkedOn,
                BookTitle = b.Book?.Title,
                BookAuthor = b.Book?.Author,
                ISBN = b.Book?.ISBN,
                BookPhoto = b.Book?.BookPhoto,
                Description = b.Book?.Description,
                Genre = b.Book?.Genre,
                Language = b.Book?.Language,
                PublicationDate = b.Book?.PublicationDate,
                Price = b.Book?.Price,
                Stock = b.Book?.Stock
            }).ToList();

            return Ok(new ApiResponseDto
            {
                IsSuccess = true,
                Message = "Bookmarks retrieved successfully.",
                StatusCode = 200,
                Data = result
            });
        }

        [HttpDelete("RemoveBookMark/{bookId:long}")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> RemoveBookmark(long bookId)
        {
            var userId = HttpContext.User.FindFirst("userId")?.Value;
            if (userId == null)
                return Unauthorized("User not found");

            var bookmark = await _context.BookMarks
                .FirstOrDefaultAsync(b => b.UserId == userId && b.BookId == bookId);

            if (bookmark == null)
            {
                return NotFound(new ApiResponseDto
                {
                    IsSuccess = false,
                    Message = "Bookmark not found.",
                    StatusCode = 404
                });
            }

            _context.BookMarks.Remove(bookmark);
            await _context.SaveChangesAsync();

            return Ok(new ApiResponseDto
            {
                IsSuccess = true,
                Message = "Bookmark removed successfully.",
                StatusCode = 200
            });
        }
    }
}
