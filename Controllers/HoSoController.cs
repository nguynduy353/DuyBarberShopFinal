
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
    [Authorize]
    public class HoSoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<TaiKhoan> _passwordHasher;

        public HoSoController(
            ApplicationDbContext context,
            IPasswordHasher<TaiKhoan> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        private int? GetCurrentUserId()
        {
            var id = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            return int.TryParse(id, out int userId)
                ? userId
                : null;
        }

        // GET: /Profile/Index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();

            if (userId == null)
                return Challenge();

            var taiKhoan = await _context.TaiKhoans
                .FindAsync(userId.Value);

            if (taiKhoan == null)
                return NotFound();

            var model = new HoSoViewModel
            {
                HoTen = taiKhoan.HoTen,
                Email = taiKhoan.Email,
                SoDienThoai = taiKhoan.SoDienThoai,
                VaiTro = taiKhoan.VaiTro
            };

            return View(model);
        }

        // POST: /Profile/Index
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(
            HoSoViewModel model)
        {
            var userId = GetCurrentUserId();

            if (userId == null)
                return Challenge();

            var taiKhoan = await _context.TaiKhoans
                .FindAsync(userId.Value);

            if (taiKhoan == null)
                return NotFound();

            // Email và vai trò luôn lấy từ database.
            model.Email = taiKhoan.Email;
            model.VaiTro = taiKhoan.VaiTro;

            ModelState.Remove(nameof(model.Email));
            ModelState.Remove(nameof(model.VaiTro));

            if (!ModelState.IsValid)
                return View(model);

            taiKhoan.HoTen = model.HoTen.Trim();
            taiKhoan.SoDienThoai = model.SoDienThoai.Trim();

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Cập nhật thông tin thành công.";

            return RedirectToAction(nameof(Index));
        }

        // GET: /Profile/ChangePassword
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View(new DoiMatKhauViewModel());
        }

        // POST: /Profile/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(
            DoiMatKhauViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userId = GetCurrentUserId();

            if (userId == null)
                return Challenge();

            var taiKhoan = await _context.TaiKhoans
                .FindAsync(userId.Value);

            if (taiKhoan == null)
                return NotFound();

            var result = _passwordHasher.VerifyHashedPassword(
                taiKhoan,
                taiKhoan.MatKhau,
                model.MatKhauHienTai);

            if (result == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(
                    nameof(model.MatKhauHienTai),
                    "Mật khẩu hiện tại không đúng.");

                return View(model);
            }

            taiKhoan.MatKhau =
                _passwordHasher.HashPassword(
                    taiKhoan,
                    model.MatKhauMoi);

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Đổi mật khẩu thành công.";

            return RedirectToAction(nameof(ChangePassword));
        }
    }
}
