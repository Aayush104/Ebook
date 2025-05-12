using BookProject.Data;
using BookProject.Dto;
using BookProject.IService;
using BookProject.Model;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IFileService _fileService;
        public AdminController(AppDbContext context, IWebHostEnvironment env, IFileService fileService)
        {
            _context = context;
            _env = env;
            _fileService = fileService;
        }

        [HttpPost("AddBook")]
        public async Task<IActionResult> AddBook(AddBookDto addBookDto)
        {
            try
            {
                // Generate BookId
                long bookId;
                if (await _context.Books.AnyAsync())
                {
                    var maxId = await _context.Books.MaxAsync(e => e.BookId);
                    bookId = maxId + 1;
                }
                else
                {
                    bookId = 1;
                }

                // Save Book Photo

                var filepath = await _fileService.SaveFileAsync(addBookDto.BookPhotoFile, "Photos");
                // Create Book entity
                var book = new Book
                {
                    BookId = bookId,
                    Title = addBookDto.Title,
                    ISBN = addBookDto.ISBN,
                    BookPhoto = filepath,
                    Description = addBookDto.Description,
                    Author = addBookDto.Author,
                    Genre = addBookDto.Genre,
                    Language = addBookDto.Language,
                    Format = addBookDto.Format,
                    Publisher = addBookDto.Publisher,
                    PublicationDate = addBookDto.PublicationDate,
                    Price = addBookDto.Price,
                    Stock = addBookDto.Stock,
                    IsAvailableInLibrary = addBookDto.IsAvailableInLibrary,
                    OnSale = addBookDto.OnSale,
                    DiscountPercentage = addBookDto.OnSale ? addBookDto.DiscountPercentage : null,
                    DiscountStartDate = addBookDto.OnSale ? addBookDto.DiscountStartDate : null,
                    DiscountEndDate = addBookDto.OnSale ? addBookDto.DiscountEndDate : null,
                    ExclusiveEdition = addBookDto.ExclusiveEdition,
                    AddedDate = DateTime.UtcNow
                };

                // Save to database
                _context.Books.Add(book);
                await _context.SaveChangesAsync();

                return StatusCode(201, new ApiResponseDto
                {
                    IsSuccess = true,
                    Message = "Book added successfully.",
                    StatusCode = 201,
                    Data = book
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDto
                {
                    IsSuccess = false,
                    Message = $"An error occurred while adding the book: {ex.Message}",
                    StatusCode = 500
                });
            }
        }

        [HttpPut("UpdateBook/{id}")]
        public async Task<IActionResult> UpdateBook(long id, [FromForm] UpdateBookDto updateBookDto)
        {
            try
            {
                var existingBook = await _context.Books.FindAsync(id);
                if (existingBook == null)
                    return NotFound(new { Message = "Book not found." });

                if (updateBookDto.BookPhotoFile != null)
                {
                    existingBook.BookPhoto = await _fileService.SaveFileAsync(updateBookDto.BookPhotoFile, "Photos");
                }

                // Update fields
                existingBook.Title = updateBookDto.Title;
                existingBook.ISBN = updateBookDto.ISBN;
                existingBook.Description = updateBookDto.Description;
                existingBook.Author = updateBookDto.Author;
                existingBook.Genre = updateBookDto.Genre;
                existingBook.Language = updateBookDto.Language;
                existingBook.Format = updateBookDto.Format;
                existingBook.Publisher = updateBookDto.Publisher;
                existingBook.PublicationDate = updateBookDto.PublicationDate;
                existingBook.Price = updateBookDto.Price;
                existingBook.Stock = updateBookDto.Stock;
                existingBook.IsAvailableInLibrary = updateBookDto.IsAvailableInLibrary;
                existingBook.OnSale = updateBookDto.OnSale;
                existingBook.DiscountPercentage = updateBookDto.OnSale ? updateBookDto.DiscountPercentage : null;
                existingBook.DiscountStartDate = updateBookDto.OnSale ? updateBookDto.DiscountStartDate : null;
                existingBook.DiscountEndDate = updateBookDto.OnSale ? updateBookDto.DiscountEndDate : null;
                existingBook.ExclusiveEdition = updateBookDto.ExclusiveEdition;

                _context.Books.Update(existingBook);
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Book updated successfully.", Data = existingBook });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"An error occurred while updating the book. {ex.Message}" });
            }
        }

        [HttpDelete("DeleteBook/{id}")]
        public async Task<IActionResult> DeleteBook(long id)
        {
            try
            {
                var book = await _context.Books.FindAsync(id);
                if (book == null)
                    return NotFound(new { Message = "Book not found." });

                _context.Books.Remove(book);
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Book deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"An error occurred while deleting the book. {ex.Message}" });
            }
        }


        [HttpGet("DashboardStats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            try
            {
                var totalBooks = await _context.Books.CountAsync();
                var totalOrders = await _context.Orders.CountAsync();

                // Get all roles
                var userRoles = await _context.UserRoles.ToListAsync();
                var roles = await _context.Roles.ToListAsync();
                var users = await _context.Users.ToListAsync();

                var staffRoleIds = roles
                    .Where(r =>  r.Name == "Staff")
                    .Select(r => r.Id)
                    .ToList();

                var publicUserRoleIds = roles
                    .Where(r =>  r.Name == "PublicUser")
                    .Select(r => r.Id)
                    .ToList();

                var totalStaff = userRoles.Count(ur => staffRoleIds.Contains(ur.RoleId));
                var totalPublicUsers = userRoles.Count(ur => publicUserRoleIds.Contains(ur.RoleId));

                return Ok(new
                {
                    TotalBooks = totalBooks,
                    TotalOrders = totalOrders,
                    TotalStaff = totalStaff,
                    TotalPublicUsers = totalPublicUsers
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"An error occurred: {ex.Message}" });

            }
        }
    }
}