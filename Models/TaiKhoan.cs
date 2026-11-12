
using System.ComponentModel.DataAnnotations;

namespace DuyBarberShop.Models
{
    public class TaiKhoan
    {
        [Key]
        public int MaTaiKhoan { get; set; }

        [Required]
        [StringLength(100)]
        public string HoTen { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [StringLength(15)]
        public string? SoDienThoai { get; set; }

        [Required]
        public string MatKhau { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string VaiTro { get; set; } = "KhachHang";

        public bool TrangThai { get; set; } = true;

        public NhanVien? NhanVien { get; set; }

        public ICollection<LichDat> LichDats { get; set; }
            = new List<LichDat>();
    }
}
