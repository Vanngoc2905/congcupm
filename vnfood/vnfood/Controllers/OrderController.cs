using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vnfood.Data;
using vnfood.Models;
using vnfood.ViewModels;

namespace vnfood.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Checkout()
        {
            var userId = _userManager.GetUserId(User);
            var cartItems = await _context.CartItems
                .Include(c => c.Post)
                .Include(c => c.Product)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            if (!cartItems.Any()) return RedirectToAction("Index", "Cart");

            var model = new CheckoutViewModel
            {
                CartItems = cartItems,
                TotalAmount = cartItems.Sum(c => 
                    (c.Post != null ? (c.Post.Price ?? 0) : 0) * c.Quantity +
                    (c.Product != null ? c.Product.Price : 0) * c.Quantity)
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrder(CheckoutViewModel model)
        {
            var userId = _userManager.GetUserId(User);
            var cartItems = await _context.CartItems
                .Include(c => c.Post)
                .Include(c => c.Product)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            if (!cartItems.Any()) return RedirectToAction("Index", "Cart");

            if (ModelState.IsValid)
            {
                var order = new Order
                {
                    UserId = userId,
                    OrderDate = DateTime.UtcNow,
                    FullName = model.FullName,
                    Address = model.Address,
                    PhoneNumber = model.PhoneNumber,
                    PaymentMethod = model.PaymentMethod,
                    TotalAmount = cartItems.Sum(c => 
                        (c.Post != null ? (c.Post.Price ?? 0) : 0) * c.Quantity +
                        (c.Product != null ? c.Product.Price : 0) * c.Quantity),
                    Status = "Chờ xác nhận"
                };

                foreach (var item in cartItems)
                {
                    var orderItem = new OrderItem
                    {
                        PostId = item.PostId,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.Post != null ? (item.Post.Price ?? 0) : (item.Product?.Price ?? 0)
                    };
                    order.OrderItems.Add(orderItem);

                    // Stock reduction for physical products
                    if (item.Product != null)
                    {
                        item.Product.StockQuantity -= item.Quantity;
                        if (item.Product.StockQuantity < 0) item.Product.StockQuantity = 0;
                    }
                }

                _context.Orders.Add(order);
                _context.CartItems.RemoveRange(cartItems);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Success), new { orderId = order.Id });
            }

            model.CartItems = cartItems;
            model.TotalAmount = cartItems.Sum(c => 
                        (c.Post != null ? (c.Post.Price ?? 0) : 0) * c.Quantity +
                        (c.Product != null ? c.Product.Price : 0) * c.Quantity);
            return View("Checkout", model);
        }

        public async Task<IActionResult> Success(int orderId)
        {
            var userId = _userManager.GetUserId(User);
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Post)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

            if (order == null) return NotFound();

            return View(order);
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var orders = await _context.Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var userId = _userManager.GetUserId(User);
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Post)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

            if (order == null) return NotFound();

            return View(order);
        }
    }
}
