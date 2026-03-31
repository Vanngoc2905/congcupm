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
    public class MessagesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MessagesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string? userId)
        {
            ViewData["ActiveNav"] = "messages";
            ViewData["Title"] = "Tin nhắn";
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            ViewBag.CurrentUser = currentUser;

            var model = new MessagesViewModel();

            var messageUsersIds = await _context.Messages
                .Where(m => m.SenderId == currentUser.Id || m.ReceiverId == currentUser.Id)
                .Select(m => m.SenderId == currentUser.Id ? m.ReceiverId : m.SenderId)
                .Distinct()
                .ToListAsync();

            model.Contacts = await _userManager.Users
                .Where(u => messageUsersIds.Contains(u.Id))
                .ToListAsync();

            if (!string.IsNullOrEmpty(userId))
            {
                model.ActiveContact = await _userManager.FindByIdAsync(userId);
                
                if (model.ActiveContact != null && !model.Contacts.Any(c => c.Id == userId))
                {
                    model.Contacts.Add(model.ActiveContact);
                }

                if (model.ActiveContact != null)
                {
                    model.Conversation = await _context.Messages
                        .Where(m => (m.SenderId == currentUser.Id && m.ReceiverId == userId) ||
                                    (m.SenderId == userId && m.ReceiverId == currentUser.Id))
                        .OrderBy(m => m.SentAt)
                        .ToListAsync();
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage(string receiverId, string content)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null || string.IsNullOrWhiteSpace(content)) return BadRequest();

            var msg = new Message
            {
                SenderId = currentUser.Id,
                ReceiverId = receiverId,
                Content = content,
                SentAt = DateTime.UtcNow
            };

            _context.Messages.Add(msg);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", new { userId = receiverId });
        }
    }
}
