using System.ComponentModel.DataAnnotations;

public class SanPhamDTO
{
    public Guid IdSanPham { get; set; } // Thêm ID sản phẩm
    public string MaSanPham { get; set; }
    public string TenSanPham { get; set; }
    public string MoTa { get; set; }
    public string TrangThai { get; set; }

    [Required(ErrorMessage = "IdChatLieu is required")]
    public Guid IdChatLieu { get; set; }

    [Required(ErrorMessage = "IdKieuDang is required")]
    public Guid IdKieuDang { get; set; }

    [Required(ErrorMessage = "IdThuongHieu is required")]
    public Guid IdThuongHieu { get; set; }

    [Required(ErrorMessage = "IdXuatXu is required")]
    public Guid IdXuatXu { get; set; }

    [Required(ErrorMessage = "IdNguoiTao is required")]
    public Guid IdNguoiTao { get; set; }

    public DateTime NgayTao { get; set; } // Thêm Ngày Tạo
}
