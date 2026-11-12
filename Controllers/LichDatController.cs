
using DuyBarberShop.Data;
using DuyBarberShop.Models;
using DuyBarberShop.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DuyBarberShop.Controllers
{
    [Authorize(Roles = "KhachHang")]
    public class LichDatController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LichDatController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /LichDat
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var maTaiKhoan = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(maTaiKhoan, out int maKhachHang))
                return Challenge();

            var lichDats = await _context.LichDats
                .Include(l => l.ChiTietLichDats)
                    .ThenInclude(ct => ct.DichVu)
                .Where(l => l.MaKhachHang == maKhachHang)
                .OrderByDescending(l => l.ThoiGianBatDau)
                .ToListAsync();

            return View(lichDats);
        }

        // POST: /LichDat/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var maTaiKhoan = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(maTaiKhoan, out int maKhachHang))
                return Challenge();

            var lichDat = await _context.LichDats
                .FirstOrDefaultAsync(l =>
                    l.MaLichDat == id &&
                    l.MaKhachHang == maKhachHang);

            if (lichDat == null)
                return NotFound();

            if (lichDat.TrangThai != "ChoXacNhan")
            {
                TempData["Error"] =
                    "Chỉ có thể hủy lịch đang chờ xác nhận.";

                return RedirectToAction(nameof(Index));
            }

            lichDat.TrangThai = "DaHuy";

            await _context.SaveChangesAsync();

            TempData["Success"] = "Hủy lịch đặt thành công!";

            return RedirectToAction(nameof(Index));
        }

        // GET: /LichDat/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.DichVus = await _context.DichVus
                .Where(d => d.TrangThai)
                .OrderBy(d => d.TenDichVu)
                .ToListAsync();

            return View(new DatLichViewModel());
        }

        // POST: /LichDat/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DatLichViewModel model)
        {

            var maDichVus = model.DanhSachMaDichVu
                .Distinct()
                .ToList();

            var dichVus = await _context.DichVus
                .Where(d => maDichVus.Contains(d.MaDichVu)
                            && d.TrangThai)
                .ToListAsync();

            if (maDichVus.Count == 0 ||
                dichVus.Count != maDichVus.Count)
            {
                ModelState.AddModelError(
                    nameof(model.DanhSachMaDichVu),
                    "Vui lòng chọn ít nhất một dịch vụ hợp lệ.");
            }

            if (model.NgayDat.HasValue && model.GioDat.HasValue)
            {
                var batDau = model.NgayDat.Value.Date
                    .Add(model.GioDat.Value);

                if (batDau <= DateTime.Now)
                {
                    ModelState.AddModelError(
                        nameof(model.NgayDat),
                        "Vui lòng chọn thời gian trong tương lai.");
                }
            }

            if (!ModelState.IsValid)
            {
                ViewBag.DichVus = await _context.DichVus
                    .Where(d => d.TrangThai)
                    .OrderBy(d => d.TenDichVu)
                    .ToListAsync();

                return View(model);
            }

            int tongThoiLuong = dichVus.Sum(d => d.ThoiLuongPhut);

            var thoiGianBatDau = model.NgayDat!.Value.Date
                .Add(model.GioDat!.Value);

            var thoiGianKetThuc = thoiGianBatDau
                .AddMinutes(tongThoiLuong);

            var maTaiKhoan = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(maTaiKhoan, out int maKhachHang))
                return Challenge();

            // Kiểm tra khách hàng đã có lịch trùng thời gian hay chưa
            bool lichBiTrung = await _context.LichDats.AnyAsync(l =>
                l.MaKhachHang == maKhachHang &&
                (l.TrangThai == "ChoXacNhan" ||
                 l.TrangThai == "DaXacNhan" ||
                 l.TrangThai == "DangThucHien") &&
                l.ThoiGianBatDau < thoiGianKetThuc &&
                l.ThoiGianKetThuc > thoiGianBatDau
            );

            if (lichBiTrung)
            {
                ModelState.AddModelError(
                    nameof(model.GioDat),
                    "Bạn đã có lịch đặt trùng khoảng thời gian này. Vui lòng chọn thời gian khác.");

                ViewBag.DichVus = await _context.DichVus
                    .Where(d => d.TrangThai)
                    .OrderBy(d => d.TenDichVu)
                    .ToListAsync();

                return View(model);
            }

            var lichDat = new LichDat
            {
                MaKhachHang = maKhachHang,
                MaNhanVien = null,
                ThoiGianBatDau = thoiGianBatDau,
                ThoiGianKetThuc = thoiGianKetThuc,
                TrangThai = "ChoXacNhan",
                NgayTao = DateTime.Now,
                GhiChu = model.GhiChu
            };

            foreach (var dichVu in dichVus)
            {
                lichDat.ChiTietLichDats.Add(
                    new ChiTietLichDat
                    {
                        MaDichVu = dichVu.MaDichVu,
                        DonGia = dichVu.GiaDichVu,
                        ThoiLuongPhut = dichVu.ThoiLuongPhut
                    });
            }

            _context.LichDats.Add(lichDat);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đặt lịch thành công!";

            return RedirectToAction(nameof(Create));
        }
    }
}
