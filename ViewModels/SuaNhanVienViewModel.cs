
using System.ComponentModel.DataAnnotations;

namespace DuyBarberShop.ViewModels
{
    public class SuaNhanVienViewModel
    {
        public int MaNhanVien { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên.")]
        [StringLength(100)]
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập email.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [StringLength(15)]
        public string? SoDienThoai { get; set; }

        public bool TrangThaiLamViec { get; set; }
    }
}
