
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DuyBarberShop.Models
{
    public class NhanVien
    {
        [Key]
        public int MaNhanVien { get; set; }

        [Required]
        public int MaTaiKhoan { get; set; }

        public bool TrangThaiLamViec { get; set; } = true;

        [ForeignKey(nameof(MaTaiKhoan))]
        public TaiKhoan TaiKhoan { get; set; } = null!;

        public ICollection<LichDat> LichDats { get; set; }
            = new List<LichDat>();
    }
}
