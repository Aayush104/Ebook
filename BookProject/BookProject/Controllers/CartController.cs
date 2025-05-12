using BookProject.Data;
using BookProject.Dto;
using BookProject.Model;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CartController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("AddinCart")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> AddToCart(AddToCartRequestDto request)
        {
            var userIdClaim = HttpContext.User.FindFirst("userId");
            if (userIdClaim == null) return Unauthorized("User not found");
            var userId = userIdClaim.Value;

            if (request.BookId <= 0)
            {
                return BadRequest(new ApiResponseDto
                {
                    IsSuccess = false,
                    Message = "Book ID is required and must be greater than zero.",
                    StatusCode = 400
                });
            }

            var existingItem = await _context.Carts.FirstOrDefaultAsync(c => c.UserId == userId && c.BookId == request.BookId);

            if (existingItem != null)
            {
                existingItem.Quantity = request.Quantity;
                _context.Carts.Update(existingItem);
                await _context.SaveChangesAsync();

                return StatusCode(200, new ApiResponseDto
                {
                    IsSuccess = true,
                    Message = "Quantity updated successfully for the book in your cart.",
                    StatusCode = 200
                });
            }
            else
            {
                var newItem = new Cart
                {
                    UserId = userId,
                    BookId = request.BookId,
                    Quantity = request.Quantity,
                    AddedDate = DateTime.UtcNow
                };

                _context.Carts.Add(newItem);
                await _context.SaveChangesAsync();

                return StatusCode(201, new ApiResponseDto
                {
                    IsSuccess = true,
                    Message = "Book successfully added to your cart.",
                    StatusCode = 201
                });
            }
        }

        [HttpGet("GetCart")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> GetMyCart()
        {
            var userIdClaim = HttpContext.User.FindFirst("userId");
            if (userIdClaim == null) return Unauthorized("User not found");
            var userId = userIdClaim.Value;

            var cartItems = await _context.Carts
                .Include(c => c.Book)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            var result = cartItems.Select(item => new CartItemResponseDto
            {
                BookId = item.BookId,
                Title = item.Book.Title,
                Author = item.Book.Author,
                ImageUrl = item.Book.BookPhoto,
                Price = item.Book.Price,
                Quantity = item.Quantity,
                AddedDate = item.AddedDate
            }).ToList();

            return Ok(new ApiResponseDto
            {
                IsSuccess = true,
                Message = "Cart items fetched successfully",
                StatusCode = 200,
                Data = result
            });
        }

        [HttpDelete("RemoveCart")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> RemoveFromCart(RemoveCartItemRequestDto request)
        {
            var userIdClaim = HttpContext.User.FindFirst("userId");
            if (userIdClaim == null) return Unauthorized("User not found");
            var userId = userIdClaim.Value;

            if (request.BookId <= 0)
            {
                return BadRequest(new ApiResponseDto
                {
                    IsSuccess = false,
                    Message = "Book ID must be greater than zero.",
                    StatusCode = 400
                });
            }
            

            

            var existingItem = await _context.Carts.FirstOrDefaultAsync(c => c.UserId == userId && c.BookId == request.BookId);

            if (existingItem == null)
            {
                return NotFound(new ApiResponseDto
                {
                    IsSuccess = false,
                    Message = "Cart item not found.",
                    StatusCode = 404
                });
            }

            _context.Carts.Remove(existingItem);
            await _context.SaveChangesAsync();

            return Ok(new ApiResponseDto
            {
                IsSuccess = true,
                Message = "Book successfully removed from your cart.",
                StatusCode = 200
            });
        }
    }
}
