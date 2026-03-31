using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vnfood.Data;
using vnfood.Models;

namespace vnfood.Controllers
{
    [Authorize]
    public class VendorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public VendorController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var products = await _context.Products.Where(p => p.UserId == user.Id).CountAsync();
            
            // Orders that contain products from this user
            var incomingOrders = await _context.OrderItems
                .Include(oi => oi.Order)
                    .ThenInclude(o => o.User)
                .Include(oi => oi.Product)
                .Where(oi => oi.Product != null && oi.Product.UserId == user.Id)
                .OrderByDescending(oi => oi.Order.OrderDate)
                .ToListAsync();

            ViewBag.ProductCount = products;
            return View(incomingOrders);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, string status)
        {
            var user = await _userManager.GetUserAsync(User);
            // Verify that at least one item in this order belongs to this vendor
            var hasVendorItems = await _context.OrderItems
                .AnyAsync(oi => oi.OrderId == orderId && oi.Product != null && oi.Product.UserId == user.Id);

            if (!hasVendorItems) return Forbid();

            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return NotFound();

            order.Status = status;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Dashboard));
        }

        public async Task<IActionResult> MyProducts()
        {
            var user = await _userManager.GetUserAsync(User);
            var products = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.UserId == user.Id)
                .ToListAsync();
            return View(products);
        }
    }
}
