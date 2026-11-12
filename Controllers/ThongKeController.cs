using DuyBarberShop.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace DuyBarberShop.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ThongKeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ThongKeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /ThongKe?thang=2026-11
        public async Task<IActionResult> Index(string? thang)
        {
            DateTime? dauThang = null;
            if (!string.IsNullOrWhiteSpace(thang))
            {
                if (!DateTime.TryParseExact(thang, "yyyy-MM",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
                {
                    TempData["Error"] = "Tháng không hợp lệ. Vui lòng chọn lại.";
                    return RedirectToAction(nameof(Index));
                }
                dauThang = parsed;
            }

            var query = _context.LichDats.AsNoTracking().AsQueryable();
            if (dauThang.HasValue)
            {
                var tuNgay = dauThang.Value;
                var denNgay = tuNgay.AddMonths(1);
                query = query.Where(ld => ld.ThoiGianBatDau >= tuNgay && ld.ThoiGianBatDau < denNgay);
            }

            var lichDats = await query
                .Include(ld => ld.ChiTietLichDats)
                    .ThenInclude(ct => ct.DichVu)
                .ToListAsync();

            ViewBag.Thang = dauThang?.ToString("yyyy-MM");
            ViewBag.TenKhoangThoiGian = dauThang.HasValue
                ? $"Tháng {dauThang.Value:MM/yyyy}"
                : "Toàn bộ thời gian";

            ViewBag.TongLichDat = lichDats.Count;
            ViewBag.DaHoanThanh = lichDats.Count(ld => ld.TrangThai == "DaHoanThanh");
            ViewBag.TongDoanhThu = lichDats
                .Where(ld => ld.TrangThai == "DaHoanThanh")
                .SelectMany(ld => ld.ChiTietLichDats)
                .Sum(ct => ct.DonGia);

            // Mỗi dịch vụ trong một lịch được tính một lượt; không tính lịch đã hủy.
            var topDichVu = lichDats
                .Where(ld => ld.TrangThai != "DaHuy")
                .SelectMany(ld => ld.ChiTietLichDats)
                .GroupBy(ct => new { ct.MaDichVu, ct.DichVu.TenDichVu })
                .Select(g => new { Ten = g.Key.TenDichVu, SoLuot = g.Count() })
                .OrderByDescending(x => x.SoLuot)
                .ThenBy(x => x.Ten)
                .Take(5)
                .ToList();
            ViewBag.TopDichVu = topDichVu.ToDictionary(x => x.Ten, x => x.SoLuot);


            // Biểu đồ hiển thị 12 tháng của năm đang chọn; mặc định năm hiện tại.
            int namBieuDo = dauThang?.Year ?? DateTime.Today.Year;
            var dauNam = new DateTime(namBieuDo, 1, 1);
            var cuoiNam = dauNam.AddYears(1);
            var lichHoanThanhTrongNam = await _context.LichDats
                .AsNoTracking()
                .Where(ld => ld.TrangThai == "DaHoanThanh"
                    && ld.ThoiGianBatDau >= dauNam && ld.ThoiGianBatDau < cuoiNam)
                .Select(ld => new
                {
                    Thang = ld.ThoiGianBatDau.Month,
                    DoanhThu = ld.ChiTietLichDats.Sum(ct => (decimal?)ct.DonGia) ?? 0m
                })
                .ToListAsync();

            var doanhThuTheoThang = new decimal[12];
            foreach (var lich in lichHoanThanhTrongNam)
                doanhThuTheoThang[lich.Thang - 1] += lich.DoanhThu;

            ViewBag.NamBieuDo = namBieuDo;
            ViewBag.DoanhThuTheoThang = doanhThuTheoThang;
            return View();
        }
    }
}
