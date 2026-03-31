using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using vnfood.Models;
using vnfood.ViewModels;
using Microsoft.EntityFrameworkCore;
using vnfood.Data;

namespace vnfood.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context,
            IWebHostEnvironment env)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _env = env;
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (_signInManager.IsSignedIn(User))
                return RedirectToAction("Index", "Home");
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (!ModelState.IsValid) return View(model);

            var result = await _signInManager.PasswordSignInAsync(
                model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
                return LocalRedirect(returnUrl ?? "/");

            ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng.");
            return View(model);
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            if (_signInManager.IsSignedIn(User))
                return RedirectToAction("Index", "Home");
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }

        // GET: /Account/Profile
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile(string? userId = null)
        {
            ViewData["ActiveNav"] = "profile";
            var currentUser = await _userManager.GetUserAsync(User);
            
            ApplicationUser? user;
            if (userId == null)
                user = currentUser;
            else
                user = await _userManager.FindByIdAsync(userId);

            if (user == null) return NotFound();

            var posts = await _context.Posts
                .Where(p => p.UserId == user.Id)
                .OrderByDescending(p => p.CreatedAt)
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                .ToListAsync();

            var savedPosts = await _context.Likes
                .Where(l => l.UserId == user.Id)
                .Include(l => l.Post)
                    .ThenInclude(p => p.Likes)
                .Include(l => l.Post)
                    .ThenInclude(p => p.Comments)
                .OrderByDescending(l => l.Id)
                .Select(l => l.Post)
                .ToListAsync();

            ViewBag.FollowersCount = await _context.Follows.CountAsync(f => f.FolloweeId == user.Id);
            ViewBag.FollowingCount = await _context.Follows.CountAsync(f => f.FollowerId == user.Id);
            ViewBag.IsFollowing = currentUser != null && await _context.Follows.AnyAsync(f => f.FollowerId == currentUser.Id && f.FolloweeId == user.Id);

            ViewBag.ProfileUser = user;
            ViewBag.Posts = posts;
            ViewBag.SavedPosts = savedPosts;
            ViewBag.IsOwner = currentUser?.Id == user.Id;
            return View();
        }

        // GET: /Account/EditProfile
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            ViewData["ActiveNav"] = "profile";
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            ViewBag.CurrentAvatarUrl = user.AvatarUrl;

            var model = new EditProfileViewModel
            {
                FullName = user.FullName ?? "",
                Bio = user.Bio ?? "",
                Email = user.Email ?? ""
            };
            return View(model);
        }

        // POST: /Account/EditProfile
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(EditProfileViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            user.FullName = model.FullName;
            user.Bio = model.Bio;

            // Handle avatar upload
            if (model.AvatarFile != null && model.AvatarFile.Length > 0)
            {
                var ext = Path.GetExtension(model.AvatarFile.FileName).ToLowerInvariant();
                var allowed = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                if (!allowed.Contains(ext))
                {
                    ModelState.AddModelError("AvatarFile", "Chỉ hỗ trợ ảnh JPG, PNG, GIF, WEBP.");
                    return View(model);
                }

                var uploads = Path.Combine(_env.WebRootPath, "uploads", "avatars");
                Directory.CreateDirectory(uploads);
                var fileName = $"{user.Id}_{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(uploads, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                    await model.AvatarFile.CopyToAsync(stream);

                user.AvatarUrl = $"/uploads/avatars/{fileName}";
            }

            await _userManager.UpdateAsync(user);
            TempData["Success"] = "Cập nhật hồ sơ thành công!";
            return RedirectToAction("Profile");
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        // GET: /Account/ChangePassword
        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            ViewData["ActiveNav"] = "profile";
            return View();
        }

        // POST: /Account/ChangePassword
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                TempData["Success"] = "Đổi mật khẩu thành công!";
                return RedirectToAction("Profile");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }

        // POST: /Account/ToggleFollow
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleFollow(string followeeId)
        {
            var follower = await _userManager.GetUserAsync(User);
            if (follower == null || follower.Id == followeeId) return BadRequest();

            var existingFollow = await _context.Follows
                .FirstOrDefaultAsync(f => f.FollowerId == follower.Id && f.FolloweeId == followeeId);

            bool isFollowing;
            if (existingFollow != null)
            {
                _context.Follows.Remove(existingFollow);
                isFollowing = false;
            }
            else
            {
                _context.Follows.Add(new Follow { FollowerId = follower.Id, FolloweeId = followeeId });
                isFollowing = true;

                // Push Notification
                _context.Notifications.Add(new vnfood.Models.Notification
                {
                    UserId = followeeId,
                    SenderId = follower.Id,
                    Message = $"{follower.FullName ?? follower.UserName} đã bắt đầu theo dõi bạn.",
                    LinkUri = $"/Account/Profile?userId={follower.Id}",
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
            int followersCount = await _context.Follows.CountAsync(f => f.FolloweeId == followeeId);

            return Json(new { isFollowing, followersCount });
        }
    }
}
