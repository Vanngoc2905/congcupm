using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using vnfood.Data;
using vnfood.Models;

namespace vnfood.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["ActiveNav"] = "home";

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            var followingIds = await _context.Follows
                .Where(f => f.FollowerId == currentUser.Id)
                .Select(f => f.FolloweeId)
                .ToListAsync();

            var feedUserIds = followingIds.ToList();
            feedUserIds.Add(currentUser.Id);

            var posts = await _context.Posts
                .Where(p => feedUserIds.Contains(p.UserId))
                .Include(p => p.User)
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User)
                .OrderByDescending(p => p.CreatedAt)
                .Take(30)
                .ToListAsync();

            var suggestedUsers = await _userManager.Users
                .Where(u => u.Id != currentUser.Id && !followingIds.Contains(u.Id))
                .Take(5)
                .ToListAsync();

            ViewBag.CurrentUser = currentUser;
            ViewBag.SuggestedUsers = suggestedUsers;
            ViewBag.LikedPostIds = posts.Where(p => p.Likes.Any(l => l.UserId == currentUser.Id))
                                        .Select(p => p.Id).ToHashSet();

            return View(posts);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
