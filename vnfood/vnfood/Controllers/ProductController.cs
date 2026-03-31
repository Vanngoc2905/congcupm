using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vnfood.Data;
using vnfood.Models;
using vnfood.ViewModels;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace vnfood.Controllers
{
    [Authorize]
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public ProductController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var categories = await _context.Categories.ToListAsync();
            var model = new ProductCreateViewModel
            {
                Categories = categories
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductCreateViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            if (ModelState.IsValid)
            {
                var product = new Product
                {
                    Name = model.Name,
                    Description = model.Description,
                    Price = model.Price,
                    StockQuantity = model.StockQuantity,
                    CategoryId = model.CategoryId,
                    UserId = user.Id,
                    CreatedAt = DateTime.UtcNow
                };

                async Task<string?> ProcessImage(IFormFile? file)
                {
                    if (file != null && file.Length > 0)
                    {
                        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                        var uploads = Path.Combine(_env.WebRootPath, "uploads", "products");
                        Directory.CreateDirectory(uploads);
                        var fileName = $"{Guid.NewGuid()}{ext}";
                        var path = Path.Combine(uploads, fileName);
                        using var stream = new FileStream(path, FileMode.Create);
                        await file.CopyToAsync(stream);
                        return $"/uploads/products/{fileName}";
                    }
                    return null;
                }

                product.ImageUrl = await ProcessImage(model.ImageFile);
                product.ImageUrl2 = await ProcessImage(model.ImageFile2);
                product.ImageUrl3 = await ProcessImage(model.ImageFile3);

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index", "Shop");
            }

            model.Categories = await _context.Categories.ToListAsync();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            return View(product);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            if (product.UserId != user?.Id) return Forbid();

            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product model)
        {
            var user = await _userManager.GetUserAsync(User);
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            if (product.UserId != user?.Id) return Forbid();

            if (ModelState.IsValid)
            {
                product.Name = model.Name;
                product.Description = model.Description;
                product.Price = model.Price;
                product.StockQuantity = model.StockQuantity;
                product.CategoryId = model.CategoryId;

                await _context.SaveChangesAsync();
                return RedirectToAction("MyProducts", "Vendor");
            }

            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            if (product.UserId != user?.Id) return Forbid();

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return RedirectToAction("MyProducts", "Vendor");
        }
    }
}
