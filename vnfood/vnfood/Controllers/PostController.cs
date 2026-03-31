using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vnfood.Data;
using vnfood.Models;

namespace vnfood.Controllers
{
    [Authorize]
    public class PostController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public PostController(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
        }

        // GET: /Post/Detail/5
        [AllowAnonymous]
        public async Task<IActionResult> Detail(int id)
        {
            ViewData["ActiveNav"] = "home";
            var post = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            ViewBag.CurrentUser = currentUser;
            ViewBag.IsLiked = currentUser != null && post.Likes.Any(l => l.UserId == currentUser.Id);
            return View(post);
        }

        // GET: /Post/Create
        [HttpGet]
        public IActionResult Create()
        {
            ViewData["ActiveNav"] = "home";
            ViewData["Title"] = "Chia sẻ Công Thức - Bếp Việt";
            return View();
        }

        // POST: /Post/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Post model, IFormFile? imageFile, IFormFile? imageFile2, IFormFile? imageFile3, IFormFile? imageFile4)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var post = new Post
            {
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                Title = model.Title?.Trim(),
                Content = model.Content?.Trim(),
                PrepTime = model.PrepTime?.Trim(),
                Servings = model.Servings?.Trim(),
                Ingredients = model.Ingredients?.Trim(),
                Instructions = model.Instructions?.Trim()
            };

            async Task<string?> ProcessImage(IFormFile? file)
            {
                if (file != null && file.Length > 0)
                {
                    var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                    var allowed = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                    if (allowed.Contains(ext))
                    {
                        var uploads = Path.Combine(_env.WebRootPath, "uploads", "posts");
                        Directory.CreateDirectory(uploads);
                        var fileName = $"{Guid.NewGuid()}{ext}";
                        var path = Path.Combine(uploads, fileName);
                        using var stream = new FileStream(path, FileMode.Create);
                        await file.CopyToAsync(stream);
                        return $"/uploads/posts/{fileName}";
                    }
                }
                return null;
            }

            post.ImageUrl = await ProcessImage(imageFile);
            post.ImageUrl2 = await ProcessImage(imageFile2);
            post.ImageUrl3 = await ProcessImage(imageFile3);
            post.ImageUrl4 = await ProcessImage(imageFile4);

            if (string.IsNullOrWhiteSpace(post.Title) && string.IsNullOrWhiteSpace(post.Content) && post.ImageUrl == null)
            {
                TempData["Error"] = "Vui lòng nhập tên công thức, mô tả hoặc chọn ảnh chính.";
                return View(model);
            }

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();
            return RedirectToAction("Detail", new { id = post.Id });
        }

        // POST: /Post/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var post = await _context.Posts.FindAsync(id);
            if (post == null || post.UserId != user?.Id)
                return Forbid();

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Home");
        }

        // POST: /Post/ToggleLike/5  (AJAX)
        [HttpPost]
        public async Task<IActionResult> ToggleLike(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var post = await _context.Posts.FindAsync(id);
            if (post == null) return NotFound();

            var existing = await _context.Likes
                .FirstOrDefaultAsync(l => l.PostId == id && l.UserId == user.Id);

            bool isLiked;
            if (existing != null)
            {
                _context.Likes.Remove(existing);
                isLiked = false;
            }
            else
            {
                _context.Likes.Add(new Like { PostId = id, UserId = user.Id });
                isLiked = true;

                if (post.UserId != user.Id)
                {
                    _context.Notifications.Add(new Notification
                    {
                        UserId = post.UserId,
                        SenderId = user.Id,
                        Message = $"{user.FullName ?? user.UserName} đã lưu công thức: {post.Title ?? "bài viết của bạn"}.",
                        LinkUri = $"/Post/Detail/{post.Id}",
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
            await _context.SaveChangesAsync();

            int count = await _context.Likes.CountAsync(l => l.PostId == id);
            return Json(new { isLiked, count });
        }

        public class CommentRequest
        {
            public int PostId { get; set; }
            public string Content { get; set; } = string.Empty;
        }

        // POST: /Post/AddComment  (AJAX)
        [HttpPost]
        public async Task<IActionResult> AddComment([FromBody] CommentRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.Content) || req.PostId <= 0)
                return BadRequest();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var post = await _context.Posts.FindAsync(req.PostId);
            if (post == null) return NotFound();

            var comment = new Comment
            {
                PostId = req.PostId,
                UserId = user.Id,
                Content = req.Content.Trim(),
                CreatedAt = DateTime.UtcNow
            };
            _context.Comments.Add(comment);
            
            if (post.UserId != user.Id)
            {
                _context.Notifications.Add(new Notification
                {
                    UserId = post.UserId,
                    SenderId = user.Id,
                    Message = $"{user.FullName ?? user.UserName} đã bình luận: {post.Title ?? "bài viết của bạn"}.",
                    LinkUri = $"/Post/Detail/{post.Id}",
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();

            return Json(new
            {
                id = comment.Id,
                content = comment.Content,
                author = user.FullName ?? user.UserName,
                avatarUrl = user.AvatarUrl,
                time = "Vừa xong"
            });
        }
    }
}
