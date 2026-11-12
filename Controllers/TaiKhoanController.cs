
using DuyBarberShop.Data;
using DuyBarberShop.Models;
using DuyBarberShop.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DuyBarberShop.Controllers
{
    public class TaiKhoanController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<TaiKhoan> _passwordHasher;

        public TaiKhoanController(
            ApplicationDbContext context,
            IPasswordHasher<TaiKhoan> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        // GET: /Account/Register
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(
            DangKyViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string email = model.Email.Trim().ToLowerInvariant();

            bool emailDaTonTai = await _context.TaiKhoans
                .AnyAsync(t => t.Email == email);

            if (emailDaTonTai)
            {
                ModelState.AddModelError(
                    nameof(model.Email),
                    "Email này đã được sử dụng.");

                return View(model);
            }

            var taiKhoan = new TaiKhoan
            {
                HoTen = model.HoTen.Trim(),
                Email = email,
                SoDienThoai = model.SoDienThoai?.Trim(),
                VaiTro = "KhachHang",
                TrangThai = true
            };

            // Không lưu mật khẩu dạng văn bản thuần
            taiKhoan.MatKhau = _passwordHasher.HashPassword(
                taiKhoan, model.MatKhau);

            _context.TaiKhoans.Add(taiKhoan);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(
                    nameof(model.Email),
                    "Không thể tạo tài khoản. Email có thể đã tồn tại.");

                return View(model);
            }

            TempData["SuccessMessage"] =
                "Đăng ký thành công! Vui lòng đăng nhập.";

            return RedirectToAction(nameof(Login));
        }

        // GET: /Account/Login
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            DangNhapViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string email = model.Email.Trim().ToLowerInvariant();

            var taiKhoan = await _context.TaiKhoans
                .FirstOrDefaultAsync(t => t.Email == email);

            if (taiKhoan == null || !taiKhoan.TrangThai)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Email hoặc mật khẩu không đúng, hoặc tài khoản bị khóa.");

                return View(model);
            }

            var ketQua = _passwordHasher.VerifyHashedPassword(
                taiKhoan, taiKhoan.MatKhau, model.MatKhau);

            if (ketQua == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Email hoặc mật khẩu không đúng, hoặc tài khoản bị khóa.");

                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    taiKhoan.MaTaiKhoan.ToString()),

                new Claim(ClaimTypes.Name, taiKhoan.HoTen),

                new Claim(ClaimTypes.Email, taiKhoan.Email),

                new Claim(ClaimTypes.Role, taiKhoan.VaiTro)
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.GhiNhoDangNhap
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                authProperties);

            if (!string.IsNullOrEmpty(returnUrl)
                && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            return RedirectToAction("Index", "TrangChu");
        }

        // POST: /Account/Logout
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Index", "TrangChu");
        }

        // GET: /Account/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // GET: /TaiKhoan
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index(string? search)
        {
            var query = _context.TaiKhoans.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(t =>
                    t.HoTen.Contains(search) ||
                    t.Email.Contains(search));
            }

            var taiKhoans = await query
                .OrderBy(t => t.MaTaiKhoan)
                .ToListAsync();

            ViewBag.Search = search;

            return View(taiKhoans);
        }

        // POST: /TaiKhoan/ToggleStatus/5
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var taiKhoan = await _context.TaiKhoans
                .FindAsync(id);

            if (taiKhoan == null)
                return NotFound();

            // Không cho Admin tự khóa tài khoản của mình
            var maDangNhap = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (maDangNhap == taiKhoan.MaTaiKhoan.ToString())
            {
                TempData["Error"] =
                    "Bạn không thể tự khóa tài khoản của mình.";

                return RedirectToAction(nameof(Index));
            }

            taiKhoan.TrangThai = !taiKhoan.TrangThai;

            await _context.SaveChangesAsync();

            TempData["Success"] = taiKhoan.TrangThai
                ? "Mở khóa tài khoản thành công!"
                : "Khóa tài khoản thành công!";

            return RedirectToAction(nameof(Index));
        }

    }
}
