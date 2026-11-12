
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DuyBarberShop.Models
{
    public class ChiTietLichDat
    {
        [Key]
        public int MaChiTietLichDat { get; set; }

        [Required]
        public int MaLichDat { get; set; }

        [Required]
        public int MaDichVu { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal DonGia { get; set; }

        [Required]
        public int ThoiLuongPhut { get; set; }

        [ForeignKey(nameof(MaLichDat))]
        public LichDat LichDat { get; set; } = null!;

        [ForeignKey(nameof(MaDichVu))]
        public DichVu DichVu { get; set; } = null!;
    }
}
