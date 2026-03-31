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
    public class SearchController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SearchController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string q)
        {
            ViewData["ActiveNav"] = ""; // No active nav icon for search
            ViewData["Title"] = "Tìm kiếm: " + q;
            
            var model = new SearchViewModel { Query = q ?? "" };

            if (!string.IsNullOrWhiteSpace(q))
            {
                string term = q.Trim().ToLower();

                model.Users = await _userManager.Users
                    .Where(u => (!string.IsNullOrEmpty(u.FullName) && u.FullName.ToLower().Contains(term)) ||
                                (!string.IsNullOrEmpty(u.UserName) && u.UserName.ToLower().Contains(term)))
                    .Take(10)
                    .ToListAsync();

                model.Posts = await _context.Posts
                    .Include(p => p.User)
                    .Include(p => p.Likes)
                    .Include(p => p.Comments)
                        .ThenInclude(c => c.User)
                    .Where(p => (!string.IsNullOrEmpty(p.Content) && p.Content.ToLower().Contains(term)))
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(30)
                    .ToListAsync();

                var currentUser = await _userManager.GetUserAsync(User);
                ViewBag.CurrentUser = currentUser;

                if (currentUser != null)
                {
                    model.LikedPostIds = model.Posts
                        .Where(p => p.Likes.Any(l => l.UserId == currentUser.Id))
                        .Select(p => p.Id).ToHashSet();
                }
            }

            return View(model);
        }
    }
}
