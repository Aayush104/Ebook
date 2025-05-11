using BookProject.Data;
using BookProject.Dto;
using BookProject.Hubss;
using BookProject.IService;
using BookProject.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace BookProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHubContext<NotificationHub> _hub;
        private readonly IMailService _mailService;

        public OrdersController(
            AppDbContext db,
            UserManager<ApplicationUser> userManager,
            IHubContext<NotificationHub> hub,
            IMailService mailService)
        {
            _db = db;
            _userManager = userManager;
            _hub = hub;
            _mailService = mailService;
        }

        
        [HttpPost("CreateOrder")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> CreateOrder([FromBody] PlaceOrderDto request)
        {
            var userId = User.FindFirst("userId")?.Value;
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized(new ApiResponseDto { IsSuccess = false, Message = "User not found", StatusCode = 401 });

            if (request?.Items == null || !request.Items.Any())
                return BadRequest(new ApiResponseDto { IsSuccess = false, Message = "Invalid order request.", StatusCode = 400 });

            // Generate claim code
            var claimCode = Guid.NewGuid().ToString().Substring(0, 8);
            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                Status = "Pending",
                ClaimCode = claimCode,
                OrderItems = new List<OrderItem>()
            };

            decimal total = 0;
            int quantity = 0;
            // Build items
            foreach (var it in request.Items)
            {
                if (it.BookId <= 0)
                    return BadRequest(new ApiResponseDto { IsSuccess = false, Message = "Invalid BookId.", StatusCode = 400 });

                order.OrderItems.Add(new OrderItem { BookId = it.BookId, Quantity = it.Quantity, UnitPrice = it.UnitPrice });
                total += it.Quantity * it.UnitPrice;
                quantity += it.Quantity;
            }

            // Discounts
            decimal discountFactor = 0;
            var messages = new List<string>();
            if (quantity >= 5)
            {
                discountFactor += 0.05m;
                messages.Add("5% quantity discount");
            }
            var prevCount = await _db.Orders.CountAsync(o => o.UserId == userId && o.Status == "Completed");
            if (prevCount > 0 && (prevCount + 1) % 10 == 0)
            {
                discountFactor += 0.10m;
                messages.Add("10% loyalty discount");
            }
            var discountAmt = total * discountFactor;
            order.TotalAmount = total - discountAmt;
            order.DiscountApplied = discountAmt;

            // Save order
            _db.Orders.Add(order);
            await _db.SaveChangesAsync();

            // Send email
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                var mail = new MailDto
                {
                    ToEmail = user.Email,
                    FullName = user.FullName,
                    ClaimCode = claimCode,
                    OrderDate = order.OrderDate,
                    TotalBooks = quantity,
                    Subtotal = total,
                    Discount = discountAmt,
                    FinalAmount = order.TotalAmount
                };

             
                await _mailService.SendOrder(mail);
            }

            // Clear cart
            var cartItems = await _db.Carts.Where(c => c.UserId == userId && order.OrderItems.Select(x => x.BookId).Contains(c.BookId)).ToListAsync();
            _db.Carts.RemoveRange(cartItems);
            await _db.SaveChangesAsync();

            return Ok(new ApiResponseDto { IsSuccess = true, StatusCode = 200, Message = $"Order placed successfully. {(messages.Any() ? string.Join(" and ", messages) : "No discount applied.")}", Data = new { order.ClaimCode, Discount = discountAmt } });
        }

     
        [HttpGet("ListPendingOrders")]
        public async Task<IActionResult> ListPendingOrders()
        {
            var list = await _db.Orders
                .Where(o => o.Status == "Pending")
                .Include(o => o.OrderItems)
                .ToListAsync();
            return Ok(new ApiResponseDto { IsSuccess = true, StatusCode = 200, Message = "Pending orders.", Data = list });
        }

       
        [HttpGet("ListCompletedOrders")]
        public async Task<IActionResult> ListCompletedOrders()
        {
            var list = await _db.Orders
                .Where(o => o.Status == "Completed")
                .Include(o => o.OrderItems)
                .ToListAsync();
            return Ok(new ApiResponseDto { IsSuccess = true, StatusCode = 200, Message = "Completed orders.", Data = list });
        }

        [HttpGet("GetOrderById")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> GetOrderById()
        {
            var userId = User.FindFirst("userId")?.Value;
            if (userId == null)
                return Unauthorized(new ApiResponseDto { IsSuccess = false, Message = "User not found", StatusCode = 401 });

            var orders = await _db.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderItems)
                .ToListAsync();

            return Ok(new ApiResponseDto { IsSuccess = true, StatusCode = 200, Message = "User orders.", Data = orders });
        }

       
        [HttpGet("GetOrderByCode/{claimCode}")]
        public async Task<IActionResult> GetOrderByCode(string claimCode)
        {
            var order = await _db.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.ClaimCode == claimCode);
            if (order == null)
                return NotFound(new ApiResponseDto { IsSuccess = false, Message = "Order not found.", StatusCode = 404 });
            return Ok(new ApiResponseDto { IsSuccess = true, StatusCode = 200, Message = "Order details.", Data = order });
        }

       
        [HttpPut("CancelOrder/{orderId}")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> CancelOrder(int orderId)
        {
            var userId = User.FindFirst("userId")?.Value;
            if (userId == null)
                return Unauthorized(new ApiResponseDto { IsSuccess = false, Message = "User not found", StatusCode = 401 });

            var order = await _db.Orders.FindAsync(orderId);
            if (order == null || order.UserId != userId)
                return NotFound(new ApiResponseDto { IsSuccess = false, Message = "Order not found or access denied.", StatusCode = 404 });

            order.Status = "Cancelled";
            await _db.SaveChangesAsync();

            return Ok(new ApiResponseDto { IsSuccess = true, StatusCode = 200, Message = "Order cancelled.", Data = orderId });
        }

       
        [HttpPost("CompleteOrderByClaimCode")]
        public async Task<IActionResult> CompleteOrder([FromBody] CompleteOrderDto dto)
        {
            var order = await _db.Orders.FirstOrDefaultAsync(o => o.ClaimCode == dto.ClaimCode);
            if (order == null)
                return NotFound(new ApiResponseDto { IsSuccess = false, Message = "Order not found.", StatusCode = 404 });

            order.Status = "Completed";
            order.OrderCompletedDate = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            // Notify
            var user = await _userManager.FindByIdAsync(order.UserId);
            var notif = new { type = "Order", content = "Order Completed", id = Guid.NewGuid().ToString(), timestamp = DateTime.UtcNow, title = "Order Completed", description = $"Order for {user.FullName} completed." };
            await _hub.Clients.All.SendAsync("ReceiveNotification", notif);

            return Ok(new ApiResponseDto { IsSuccess = true, StatusCode = 200, Message = "Order completed.", Data = order.UserId });
        }

      
        [HttpGet("GetOrderNotification")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> GetOrderNotification()
        {
            var userId = User.FindFirst("userId")?.Value;
            if (userId == null)
                return Unauthorized(new ApiResponseDto { IsSuccess = false, Message = "User not found", StatusCode = 401 });

            var current = await _userManager.FindByIdAsync(userId);
            if (current == null || current.CreatedAt == null)
                return NotFound(new ApiResponseDto { IsSuccess = false, Message = "User data incomplete.", StatusCode = 404 });

            var since = current.CreatedAt.Value;
            var completed = await _db.Orders.Where(o => o.Status == "Completed" && o.OrderCompletedDate > since && o.UserId != userId).ToListAsync();

            var notes = completed.Select(o => new {
                type = "Order",
                content = "Order Completed",
                id = Guid.NewGuid().ToString(),
                timestamp = o.OrderCompletedDate,
                title = "Order Completed",
                description = $"Order by {o.UserId} completed at {o.OrderCompletedDate:G}."
            });

            return Ok(new ApiResponseDto { IsSuccess = true, StatusCode = 200, Message = "Notifications.", Data = notes });
        }
    }
}
   

