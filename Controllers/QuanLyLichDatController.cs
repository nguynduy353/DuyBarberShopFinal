
using DuyBarberShop.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DuyBarberShop.ViewModels;
using DuyBarberShop.Models;

namespace DuyBarberShop.Controllers
{
    [Authorize(Roles = "Admin")]
    public class QuanLyLichDatController : Controller
    {
        private readonly ApplicationDbContext _context;

        public QuanLyLichDatController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /QuanLyLichDat
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var lichDats = await _context.LichDats
                .Include(l => l.KhachHang)
                .Include(l => l.NhanVien)
                    .ThenInclude(nv => nv!.TaiKhoan)
                .Include(l => l.ChiTietLichDats)
                    .ThenInclude(ct => ct.DichVu)
                .OrderByDescending(l => l.ThoiGianBatDau)
                .ToListAsync();

            ViewBag.NhanViens = await _context.NhanViens
                .Include(nv => nv.TaiKhoan)
                .Where(nv => nv.TrangThaiLamViec
                    && nv.TaiKhoan.TrangThai)
                .OrderBy(nv => nv.TaiKhoan.HoTen)
                .ToListAsync();

            return View(lichDats);
        }

        // POST: /QuanLyLichDat/XacNhan
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XacNhan(
            PhanCongNhanVienViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Vui lòng chọn nhân viên hợp lệ.";
                return RedirectToAction(nameof(Index));
            }

            // Tìm lịch đặt cần xác nhận
            var lichDat = await _context.LichDats
                .FirstOrDefaultAsync(l =>
                    l.MaLichDat == model.MaLichDat);

            if (lichDat == null)
                return NotFound();

            // Chỉ xác nhận lịch đang chờ
            if (lichDat.TrangThai != "ChoXacNhan")
            {
                TempData["Error"] = "Lịch đặt không còn chờ xác nhận.";
                return RedirectToAction(nameof(Index));
            }

            // Kiểm tra nhân viên đang làm việc
            var nhanVien = await _context.NhanViens
                .Include(nv => nv.TaiKhoan)
                .FirstOrDefaultAsync(nv =>
                    nv.MaNhanVien == model.MaNhanVien &&
                    nv.TrangThaiLamViec);

            if (nhanVien == null ||
                !nhanVien.TaiKhoan.TrangThai)
            {
                TempData["Error"] = "Nhân viên không hợp lệ hoặc đã ngừng làm việc.";
                return RedirectToAction(nameof(Index));
            }

            // Kiểm tra lịch bị trùng của nhân viên
            bool trungLich = await _context.LichDats
                .AnyAsync(l =>
                    l.MaLichDat != lichDat.MaLichDat &&
                    l.MaNhanVien == model.MaNhanVien &&
                    (l.TrangThai == "DaXacNhan" ||
                     l.TrangThai == "DangThucHien") &&
                    l.ThoiGianBatDau < lichDat.ThoiGianKetThuc &&
                    l.ThoiGianKetThuc > lichDat.ThoiGianBatDau);

            if (trungLich)
            {
                TempData["Error"] =
                    "Nhân viên đã có lịch làm việc trong khoảng thời gian này.";

                return RedirectToAction(nameof(Index));
            }

            // Phân công nhân viên và xác nhận lịch
            lichDat.MaNhanVien = model.MaNhanVien;
            lichDat.TrangThai = "DaXacNhan";

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Xác nhận lịch và phân công nhân viên thành công!";

            return RedirectToAction(nameof(Index));
        }

        // POST: /QuanLyLichDat/BatDau
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BatDau(int id)
        {
            var lichDat = await _context.LichDats
                .FirstOrDefaultAsync(l => l.MaLichDat == id);

            if (lichDat == null)
                return NotFound();

            if (lichDat.TrangThai != "DaXacNhan" ||
                lichDat.MaNhanVien == null)
            {
                TempData["Error"] =
                    "Chỉ có thể bắt đầu lịch đã xác nhận và có nhân viên.";

                return RedirectToAction(nameof(Index));
            }

            lichDat.TrangThai = "DangThucHien";

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Bắt đầu thực hiện lịch đặt thành công!";

            return RedirectToAction(nameof(Index));
        }

        // POST: /QuanLyLichDat/HoanThanh
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HoanThanh(int id)
        {
            var lichDat = await _context.LichDats
                .FirstOrDefaultAsync(l => l.MaLichDat == id);

            if (lichDat == null)
                return NotFound();

            if (lichDat.TrangThai != "DangThucHien")
            {
                TempData["Error"] =
                    "Chỉ có thể hoàn thành lịch đang thực hiện.";

                return RedirectToAction(nameof(Index));
            }

            lichDat.TrangThai = "DaHoanThanh";

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Hoàn thành lịch đặt thành công!";

            return RedirectToAction(nameof(Index));
        }

    }
}
