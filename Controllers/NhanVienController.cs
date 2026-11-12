
using DuyBarberShop.Data;
using DuyBarberShop.Models;
using DuyBarberShop.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DuyBarberShop.Controllers
{
    [Authorize(Roles = "Admin")]
    public class NhanVienController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NhanVienController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /NhanVien
        public async Task<IActionResult> Index(string? search)
        {
            var query = _context.NhanViens
                .Include(nv => nv.TaiKhoan)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(nv =>
                    nv.TaiKhoan.HoTen.Contains(search) ||
                    nv.TaiKhoan.Email.Contains(search));
            }

            var nhanViens = await query
                .OrderBy(nv => nv.TaiKhoan.HoTen)
                .ToListAsync();

            ViewBag.Search = search;

            return View(nhanViens);
        }

        // GET: /NhanVien/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var nhanVien = await _context.NhanViens
                .Include(nv => nv.TaiKhoan)
                .FirstOrDefaultAsync(nv => nv.MaNhanVien == id);

            if (nhanVien == null)
                return NotFound();

            var model = new SuaNhanVienViewModel
            {
                MaNhanVien = nhanVien.MaNhanVien,
                HoTen = nhanVien.TaiKhoan.HoTen,
                Email = nhanVien.TaiKhoan.Email,
                SoDienThoai = nhanVien.TaiKhoan.SoDienThoai,
                TrangThaiLamViec = nhanVien.TrangThaiLamViec
            };

            return View(model);
        }

        // POST: /NhanVien/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id, SuaNhanVienViewModel model)
        {
            if (id != model.MaNhanVien)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(model);

            var nhanVien = await _context.NhanViens
                .Include(nv => nv.TaiKhoan)
                .FirstOrDefaultAsync(nv => nv.MaNhanVien == id);

            if (nhanVien == null)
                return NotFound();

            bool emailDaTonTai = await _context.TaiKhoans
                .AnyAsync(t =>
                    t.Email == model.Email &&
                    t.MaTaiKhoan != nhanVien.MaTaiKhoan);

            if (emailDaTonTai)
            {
                ModelState.AddModelError(
                    nameof(model.Email),
                    "Email này đã được sử dụng.");

                return View(model);
            }

            nhanVien.TaiKhoan.HoTen = model.HoTen;
            nhanVien.TaiKhoan.Email = model.Email;
            nhanVien.TaiKhoan.SoDienThoai = model.SoDienThoai;
            nhanVien.TrangThaiLamViec = model.TrangThaiLamViec;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Cập nhật nhân viên thành công!";

            return RedirectToAction(nameof(Index));
        }

        // GET: /NhanVien/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View(new ThemNhanVienViewModel());
        }

        // POST: /NhanVien/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ThemNhanVienViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            bool emailDaTonTai = await _context.TaiKhoans
                .AnyAsync(t => t.Email == model.Email);

            if (emailDaTonTai)
            {
                ModelState.AddModelError(
                    nameof(model.Email),
                    "Email này đã được sử dụng.");

                return View(model);
            }

            var taiKhoan = new TaiKhoan
            {
                HoTen = model.HoTen,
                Email = model.Email,
                SoDienThoai = model.SoDienThoai,
                VaiTro = "NhanVien",
                TrangThai = true
            };

            var hasher = new PasswordHasher<TaiKhoan>();

            taiKhoan.MatKhau = hasher.HashPassword(
                taiKhoan, model.MatKhau);

            var nhanVien = new NhanVien
            {
                TaiKhoan = taiKhoan,
                TrangThaiLamViec = model.TrangThaiLamViec
            };

            _context.NhanViens.Add(nhanVien);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Thêm nhân viên thành công!";

            return RedirectToAction(nameof(Index));
        }
        // POST: /NhanVien/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var nhanVien = await _context.NhanViens
                .Include(nv => nv.TaiKhoan)
                .FirstOrDefaultAsync(nv => nv.MaNhanVien == id);

            if (nhanVien == null)
                return NotFound();

            // Không xóa nhân viên có lịch sử được phân công.
            bool coLichDat = await _context.LichDats
                .AnyAsync(ld => ld.MaNhanVien == id);

            // Phòng trường hợp tài khoản nhân viên từng được dùng để đặt lịch.
            bool coLichKhachHang = await _context.LichDats
                .AnyAsync(ld => ld.MaKhachHang == nhanVien.MaTaiKhoan);

            if (coLichDat || coLichKhachHang)
            {
                TempData["Error"] = "Không thể xóa nhân viên đã có lịch đặt liên quan. Hãy chuyển sang trạng thái Ngừng làm việc.";
                return RedirectToAction(nameof(Index));
            }

            var taiKhoan = nhanVien.TaiKhoan;
            _context.NhanViens.Remove(nhanVien);
            _context.TaiKhoans.Remove(taiKhoan);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Xóa nhân viên thành công!";
            return RedirectToAction(nameof(Index));
        }

    }
}
