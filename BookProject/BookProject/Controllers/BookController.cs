using BookProject.Data;
using BookProject.Dto;
using BookProject.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BookController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("BookPagination")]
        public async Task<IActionResult> GetBooks([FromQuery] PaginationParams paginationParams)
        {
            if (paginationParams.Page <= 0 || paginationParams.PageSize <= 0)
            {
                return BadRequest(new ApiResponseDto
                {
                    IsSuccess = false,
                    Message = "Page and PageSize must be greater than 0.",
                    StatusCode = 400
                });
            }

            var query = _context.Books.OrderBy(b => b.BookId).AsQueryable();
            var totalItems = await query.CountAsync();

            var books = await query
                .Skip((paginationParams.Page - 1) * paginationParams.PageSize)
                .Take(paginationParams.PageSize)
                .ToListAsync();

            if (!books.Any())
            {
                return NotFound(new ApiResponseDto
                {
                    IsSuccess = false,
                    Message = "No books found.",
                    StatusCode = 404
                });
            }

            var result = new PagedResult<GetBookDto>
            {
                CurrentPage = paginationParams.Page,
                PageSize = paginationParams.PageSize,
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling(totalItems / (double)paginationParams.PageSize),
                Items = books.Select(book => new GetBookDto
                {
                    BookId = book.BookId,
                    Title = book.Title,
                    Isbn = book.ISBN,
                    Description = book.Description,
                    Author = book.Author,
                    Genre = book.Genre,
                    Language = book.Language,
                    BookPhoto = book.BookPhoto,
                    Format = book.Format,
                    Publisher = book.Publisher,
                    PublicationDate = book.PublicationDate,
                    Price = book.Price,
                    Stock = book.Stock,
                    IsAvailableInLibrary = book.IsAvailableInLibrary,
                    OnSale = book.OnSale,
                    DiscountPercentage = book.DiscountPercentage,
                    DiscountStartDate = book.DiscountStartDate,
                    DiscountEndDate = book.DiscountEndDate,
                    ExclusiveEdition = book.ExclusiveEdition,
                    AddedDate = book.AddedDate
                }).ToList()
            };

            return Ok(new ApiResponseDto
            {
                IsSuccess = true,
                Message = "Books fetched successfully.",
                StatusCode = 200,
                Data = result
            });
        }

        [HttpGet("GetBookById/{id}")]
        public async Task<IActionResult> GetBookById(long id)
        {
            var book = await _context.Books.FindAsync(id);

            if (book == null)
            {
                return NotFound(new ApiResponseDto
                {
                    IsSuccess = false,
                    Message = "Book not found.",
                    StatusCode = 404
                });
            }

            return Ok(new ApiResponseDto
            {
                IsSuccess = true,
                Message = "Book fetched successfully.",
                StatusCode = 200,
                Data = book
            });
        }

        [HttpGet("SearchBooks")]
        public async Task<IActionResult> FilterBooks([FromQuery] BookFilterParams filterParams)
        {
            var query = _context.Books.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filterParams.Search))
            {
                query = query.Where(b =>
                    b.Title.Contains(filterParams.Search) ||
                    b.Author.Contains(filterParams.Search) ||
                    b.ISBN.Contains(filterParams.Search) ||
                    b.Description.Contains(filterParams.Search));
            }

            if (!string.IsNullOrWhiteSpace(filterParams.Genre))
                query = query.Where(b => b.Genre == filterParams.Genre);

            if (!string.IsNullOrWhiteSpace(filterParams.Author))
                query = query.Where(b => b.Author == filterParams.Author);

            if (!string.IsNullOrWhiteSpace(filterParams.Publisher))
                query = query.Where(b => b.Publisher == filterParams.Publisher);

            if (!string.IsNullOrWhiteSpace(filterParams.Language))
                query = query.Where(b => b.Language == filterParams.Language);

            if (filterParams.InStock == true)
                query = query.Where(b => b.Stock > 0);

            if (filterParams.InLibrary.HasValue)
                query = query.Where(b => b.IsAvailableInLibrary == filterParams.InLibrary);

            if (filterParams.MinPrice.HasValue)
                query = query.Where(b => b.Price >= filterParams.MinPrice.Value);

            if (filterParams.MaxPrice.HasValue)
                query = query.Where(b => b.Price <= filterParams.MaxPrice.Value);

            if (!string.IsNullOrWhiteSpace(filterParams.Format))
                query = query.Where(b => b.Format == filterParams.Format);

            query = filterParams.SortBy?.ToLower() switch
            {
                "title" => filterParams.SortOrder == "desc" ? query.OrderByDescending(b => b.Title) : query.OrderBy(b => b.Title),
                "price" => filterParams.SortOrder == "desc" ? query.OrderByDescending(b => b.Price) : query.OrderBy(b => b.Price),
                "publicationdate" => filterParams.SortOrder == "desc" ? query.OrderByDescending(b => b.PublicationDate) : query.OrderBy(b => b.PublicationDate),
                _ => query.OrderBy(b => b.Title)
            };

            var books = await query
                .Skip((filterParams.Page - 1) * filterParams.PageSize)
                .Take(filterParams.PageSize)
                .ToListAsync();

            if (!books.Any())
            {
                return NotFound(new ApiResponseDto
                {
                    IsSuccess = false,
                    Message = "No books found.",
                    StatusCode = 404
                });
            }

            var result = books.Select(book => new GetBookDto
            {
                BookId = book.BookId,
                Title = book.Title,
                Isbn = book.ISBN,
                Description = book.Description,
                Author = book.Author,
                Genre = book.Genre,
                Language = book.Language,
                BookPhoto = book.BookPhoto,
                Format = book.Format,
                Publisher = book.Publisher,
                PublicationDate = book.PublicationDate,
                Price = book.Price,
                Stock = book.Stock,
                IsAvailableInLibrary = book.IsAvailableInLibrary,
                OnSale = book.OnSale,
                DiscountPercentage = book.DiscountPercentage,
                DiscountStartDate = book.DiscountStartDate,
                DiscountEndDate = book.DiscountEndDate,
                ExclusiveEdition = book.ExclusiveEdition,
                AddedDate = book.AddedDate
            }).ToList();

            return Ok(new ApiResponseDto
            {
                IsSuccess = true,
                Message = "Books fetched successfully.",
                StatusCode = 200,
                Data = result
            });
        }

        
        [HttpGet("NewReleases")]
        public async Task<IActionResult> GetNewReleasesBooks()
        {
            try
            {
                var threeMonthsAgo = DateTime.UtcNow.AddMonths(-3);
                var books = await _context.Books
                    .Where(b => b.PublicationDate >= threeMonthsAgo)
                    .OrderByDescending(b => b.PublicationDate)
                    .Take(4)
                    .ToListAsync();

                var result = books.Select(b => new GetBookDto
                {
                    BookId = b.BookId,
                    Title = b.Title,
                    Isbn = b.ISBN,
                    Description = b.Description,
                    Author = b.Author,
                    Genre = b.Genre,
                    Language = b.Language,
                    BookPhoto = b.BookPhoto,
                    Format = b.Format,
                    Publisher = b.Publisher,
                    PublicationDate = b.PublicationDate,
                    Price = b.Price,
                    Stock = b.Stock,
                    IsAvailableInLibrary = b.IsAvailableInLibrary,
                    OnSale = b.OnSale,
                    DiscountPercentage = b.DiscountPercentage,
                    DiscountStartDate = b.DiscountStartDate,
                    DiscountEndDate = b.DiscountEndDate,
                    ExclusiveEdition = b.ExclusiveEdition,
                    AddedDate = b.AddedDate
                }).ToList();

                return StatusCode(200, new ApiResponseDto
                {
                    IsSuccess = true,
                    Message = "New release books fetched successfully.",
                    StatusCode = 200,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDto
                {
                    IsSuccess = false,
                    Message = $"An error occurred while fetching new release books. {ex.Message}",
                    StatusCode = 500
                });
            }
        }

        
       [HttpGet("BestSellers")]
        public async Task<IActionResult> GetBestSellersBooks()
        {
            try
            {
                var topBookIds = await _context.OrderItems
                    .Where(oi => oi.Order != null && oi.Order.Status == "Completed")
                    .GroupBy(oi => oi.BookId)
                    .OrderByDescending(g => g.Sum(oi => oi.Quantity))
                    .Take(4)
                    .Select(g => g.Key)
                    .ToListAsync();

                var books = await _context.Books
                    .Where(b => topBookIds.Contains(b.BookId))
                    .ToListAsync();

             
                books = topBookIds.Select(id => books.First(b => b.BookId == id)).ToList();

                var result = books.Select(b => new GetBookDto
                {
                    BookId = b.BookId,
                    Title = b.Title,
                    Isbn = b.ISBN,
                    Description = b.Description,
                    Author = b.Author,
                    Genre = b.Genre,
                    Language = b.Language,
                    BookPhoto = b.BookPhoto,
                    Format = b.Format,
                    Publisher = b.Publisher,
                    PublicationDate = b.PublicationDate,
                    Price = b.Price,
                    Stock = b.Stock,
                    IsAvailableInLibrary = b.IsAvailableInLibrary,
                    OnSale = b.OnSale,
                    DiscountPercentage = b.DiscountPercentage,
                    DiscountStartDate = b.DiscountStartDate,
                    DiscountEndDate = b.DiscountEndDate,
                    ExclusiveEdition = b.ExclusiveEdition,
                    AddedDate = b.AddedDate
                }).ToList();

                return StatusCode(200, new ApiResponseDto
                {
                    IsSuccess = true,
                    Message = "Best seller books fetched successfully.",
                    StatusCode = 200,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDto
                {
                    IsSuccess = false,
                    Message = $"An error occurred while fetching best sellers books. {ex.Message}",
                    StatusCode = 500
                });
            }
        }

       
        [HttpGet("SpecialDeals")]
        public async Task<IActionResult> GetSpecialDealsBooks()
        {
            try
            {
                var books = await _context.Books
                    .Where(b => b.OnSale && b.DiscountPercentage.HasValue)
                    .OrderByDescending(b => b.DiscountPercentage)
                    .Take(4)
                    .ToListAsync();

                var result = books.Select(b => new GetBookDto
                {
                    BookId = b.BookId,
                    Title = b.Title,
                    Isbn = b.ISBN,
                    Description = b.Description,
                    Author = b.Author,
                    Genre = b.Genre,
                    Language = b.Language,
                    BookPhoto = b.BookPhoto,
                    Format = b.Format,
                    Publisher = b.Publisher,
                    PublicationDate = b.PublicationDate,
                    Price = b.Price,
                    Stock = b.Stock,
                    IsAvailableInLibrary = b.IsAvailableInLibrary,
                    OnSale = b.OnSale,
                    DiscountPercentage = b.DiscountPercentage,
                    DiscountStartDate = b.DiscountStartDate,
                    DiscountEndDate = b.DiscountEndDate,
                    ExclusiveEdition = b.ExclusiveEdition,
                    AddedDate = b.AddedDate
                }).ToList();

                return StatusCode(200, new ApiResponseDto
                {
                    IsSuccess = true,
                    Message = "Special deals books fetched successfully.",
                    StatusCode = 200,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDto
                {
                    IsSuccess = false,
                    Message = $"An error occurred while fetching special deals books. {ex.Message}",
                    StatusCode = 500
                });
            }
        }
    }
}
