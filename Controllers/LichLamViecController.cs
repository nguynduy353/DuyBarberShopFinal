
using DuyBarberShop.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DuyBarberShop.Controllers
{
    [Authorize(Roles = "NhanVien")]
    public class LichLamViecController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LichLamViecController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /LichLamViec
        public async Task<IActionResult> Index()
        {
            // Lấy mã tài khoản đang đăng nhập
            var maTaiKhoanString =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(maTaiKhoanString, out int maTaiKhoan))
            {
                return Unauthorized();
            }

            // Tìm nhân viên tương ứng với tài khoản
            var nhanVien = await _context.NhanViens
                .FirstOrDefaultAsync(nv =>
                    nv.MaTaiKhoan == maTaiKhoan);

            if (nhanVien == null)
            {
                return NotFound("Không tìm thấy thông tin nhân viên.");
            }

            // Chỉ lấy lịch được phân công cho nhân viên này
            var lichLamViecs = await _context.LichDats
                .Include(ld => ld.KhachHang)
                .Include(ld => ld.ChiTietLichDats)
                    .ThenInclude(ct => ct.DichVu)
                .Where(ld => ld.MaNhanVien == nhanVien.MaNhanVien)
                .OrderBy(ld => ld.ThoiGianBatDau)
                .ToListAsync();

            return View(lichLamViecs);
        }
    }
}
