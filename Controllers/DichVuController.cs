
using DuyBarberShop.Data;
using DuyBarberShop.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DuyBarberShop.Controllers
{
    public class DichVuController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public DichVuController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: /Services
        [AllowAnonymous]
        public async Task<IActionResult> Index(string? search)
        {
            var query = _context.DichVus.AsQueryable();

            if (!User.IsInRole("Admin"))
            {
                query = query.Where(d => d.TrangThai);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(d =>
                    d.TenDichVu.Contains(search));
            }

            var dichVus = await query
                .OrderBy(d => d.TenDichVu)
                .ToListAsync();

            ViewBag.Search = search;
            return View(dichVus);
        }

        // GET: /Services/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var dichVu = await _context.DichVus
                .FirstOrDefaultAsync(d => d.MaDichVu == id);

            if (dichVu == null)
                return NotFound();

            return View(dichVu);
        }

        // GET: /Services/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Services/Create
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DichVu dichVu, IFormFile? anhDichVu)
        {
            if (!ModelState.IsValid)
                return View(dichVu);

            if (anhDichVu != null && anhDichVu.Length > 0)
            {
                var (path, error) = await LuuAnhAsync(anhDichVu);
                if (error != null)
                {
                    ModelState.AddModelError("anhDichVu", error);
                    return View(dichVu);
                }
                dichVu.HinhAnh = path;
            }

            _context.DichVus.Add(dichVu);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Thêm dịch vụ thành công!";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Services/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var dichVu = await _context.DichVus.FindAsync(id);

            if (dichVu == null)
                return NotFound();

            return View(dichVu);
        }

        // POST: /Services/Edit/5
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DichVu model, IFormFile? anhDichVu)
        {
            if (id != model.MaDichVu)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(model);

            var dichVu = await _context.DichVus.FindAsync(id);

            if (dichVu == null)
                return NotFound();

            if (anhDichVu != null && anhDichVu.Length > 0)
            {
                var (path, error) = await LuuAnhAsync(anhDichVu);
                if (error != null)
                {
                    ModelState.AddModelError("anhDichVu", error);
                    model.HinhAnh = dichVu.HinhAnh;
                    return View(model);
                }
                dichVu.HinhAnh = path;
            }

            dichVu.TenDichVu = model.TenDichVu;
            dichVu.MoTa = model.MoTa;
            dichVu.GiaDichVu = model.GiaDichVu;
            dichVu.ThoiLuongPhut = model.ThoiLuongPhut;
            dichVu.TrangThai = model.TrangThai;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Cập nhật dịch vụ thành công!";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Services/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var dichVu = await _context.DichVus.FindAsync(id);

            if (dichVu == null)
                return NotFound();

            return View(dichVu);
        }

        // POST: /Services/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var dichVu = await _context.DichVus.FindAsync(id);

            if (dichVu == null)
                return NotFound();

            bool daDuocDat = await _context.ChiTietLichDats
                .AnyAsync(c => c.MaDichVu == id);

            if (daDuocDat)
            {
                TempData["Error"] =
                    "Dịch vụ đã có lịch đặt, không thể xóa.";
                return RedirectToAction(nameof(Index));
            }

            // Giữ file ảnh trên đĩa khi xóa dịch vụ để tránh xóa nhầm ảnh đang dùng.
            _context.DichVus.Remove(dichVu);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Xóa dịch vụ thành công!";
            return RedirectToAction(nameof(Index));
        }

        // Kiểm tra cả chữ ký tệp, phần mở rộng và kích thước trước khi lưu.
        private async Task<(string? path, string? error)> LuuAnhAsync(IFormFile file)
        {
            const long maxSize = 5 * 1024 * 1024;
            if (file.Length == 0 || file.Length > maxSize)
                return (null, "Ảnh phải có dung lượng từ 1 byte đến 5 MB.");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (ext != ".jpg" && ext != ".jpeg" && ext != ".png" && ext != ".webp")
                return (null, "Chỉ chấp nhận ảnh JPG, PNG hoặc WebP.");

            byte[] header = new byte[12];
            using (var stream = file.OpenReadStream())
            {
                var count = await stream.ReadAsync(header.AsMemory(0, header.Length));
                if (count < 12)
                    return (null, "Tệp ảnh không hợp lệ.");
            }
            bool jpeg = header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF;
            bool png = header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47
                && header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A;
            bool webp = System.Text.Encoding.ASCII.GetString(header, 0, 4) == "RIFF"
                && System.Text.Encoding.ASCII.GetString(header, 8, 4) == "WEBP";
            if (!((ext == ".jpg" || ext == ".jpeg") && jpeg || ext == ".png" && png || ext == ".webp" && webp))
                return (null, "Nội dung tệp không đúng định dạng ảnh đã chọn.");

            var root = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
            var folder = Path.Combine(root, "images", "services");
            Directory.CreateDirectory(folder);
            var filename = $"{Guid.NewGuid():N}{ext}";
            var filepath = Path.Combine(folder, filename);
            await using (var output = new FileStream(filepath, FileMode.CreateNew))
            {
                await file.CopyToAsync(output);
            }
            return ($"/images/services/{filename}", null);
        }
    }
}
