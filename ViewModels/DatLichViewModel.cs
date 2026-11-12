
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DuyBarberShop.ViewModels
{
    public class DatLichViewModel
    {
        [Required(ErrorMessage = "Vui lòng chọn ít nhất một dịch vụ.")]
        public List<int> DanhSachMaDichVu { get; set; } = new();

        [Required(ErrorMessage = "Vui lòng chọn ngày đặt lịch.")]
        [DataType(DataType.Date)]
        public DateTime? NgayDat { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn giờ đặt lịch.")]
        [DataType(DataType.Time)]
        public TimeSpan? GioDat { get; set; }

        [StringLength(500, ErrorMessage = "Ghi chú tối đa 500 ký tự.")]
        public string? GhiChu { get; set; }
    }
}
