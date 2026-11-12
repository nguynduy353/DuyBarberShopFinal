
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DuyBarberShop.Models
{
    public class DichVu
    {
        [Key]
        public int MaDichVu { get; set; }

        [Required]
        [StringLength(100)]
        public string TenDichVu { get; set; } = string.Empty;

        [StringLength(500)]
        public string? MoTa { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal GiaDichVu { get; set; }

        [Required]
        public int ThoiLuongPhut { get; set; }

        public bool TrangThai { get; set; } = true;

        [StringLength(300)]
        public string? HinhAnh { get; set; }

        public ICollection<ChiTietLichDat> ChiTietLichDats
        { get; set; } = new List<ChiTietLichDat>();
    }
}
