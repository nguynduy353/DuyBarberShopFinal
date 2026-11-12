
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DuyBarberShop.Models
{
    public class LichDat
    {
        [Key]
        public int MaLichDat { get; set; }

        [Required]
        public int MaKhachHang { get; set; }

        public int? MaNhanVien { get; set; }

        [Required]
        public DateTime ThoiGianBatDau { get; set; }

        [Required]
        public DateTime ThoiGianKetThuc { get; set; }

        [Required]
        [StringLength(30)]
        public string TrangThai { get; set; } = "ChoXacNhan";

        public DateTime NgayTao { get; set; } = DateTime.Now;

        [StringLength(500)]
        public string? GhiChu { get; set; }

        [ForeignKey(nameof(MaKhachHang))]
        public TaiKhoan KhachHang { get; set; } = null!;

        [ForeignKey(nameof(MaNhanVien))]
        public NhanVien? NhanVien { get; set; }

        public ICollection<ChiTietLichDat> ChiTietLichDats
        { get; set; } = new List<ChiTietLichDat>();
    }
}
