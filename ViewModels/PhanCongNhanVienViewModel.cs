
using System.ComponentModel.DataAnnotations;

namespace DuyBarberShop.ViewModels
{
    public class PhanCongNhanVienViewModel
    {
        [Required]
        public int MaLichDat { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn nhân viên.")]
        [Range(1, int.MaxValue,
            ErrorMessage = "Vui lòng chọn nhân viên hợp lệ.")]
        public int MaNhanVien { get; set; }
    }
}
