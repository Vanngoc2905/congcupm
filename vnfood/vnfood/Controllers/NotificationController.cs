using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vnfood.Data;
using vnfood.Models;

namespace vnfood.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Notification/GetLatest
        [HttpGet]
        public async Task<IActionResult> GetLatest()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var unreadCount = await _context.Notifications
                .CountAsync(n => n.UserId == user.Id && !n.IsRead);

            var latest = await _context.Notifications
                .Include(n => n.Sender)
                .Where(n => n.UserId == user.Id)
                .OrderByDescending(n => n.CreatedAt)
                .Take(7)
                .Select(n => new
                {
                    n.Id,
                    n.Message,
                    n.LinkUri,
                    n.IsRead,
                    n.CreatedAt,
                    SenderAvatar = n.Sender != null ? n.Sender.AvatarUrl : null,
                    SenderName = n.Sender != null ? (n.Sender.FullName ?? n.Sender.UserName) : "Hệ thống"
                })
                .ToListAsync();

            return Json(new { unreadCount, latest });
        }

        // POST: /Notification/MarkRead/5
        [HttpPost]
        public async Task<IActionResult> MarkRead(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var notif = await _context.Notifications.FindAsync(id);
            if (notif != null && notif.UserId == user.Id)
            {
                notif.IsRead = true;
                await _context.SaveChangesAsync();
            }

            return Ok();
        }

        // POST: /Notification/MarkAllRead
        [HttpPost]
        public async Task<IActionResult> MarkAllRead()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var unread = await _context.Notifications
                .Where(n => n.UserId == user.Id && !n.IsRead)
                .ToListAsync();

            foreach (var n in unread)
            {
                n.IsRead = true;
            }

            if (unread.Any())
            {
                await _context.SaveChangesAsync();
            }

            return Ok();
        }
    }
}
