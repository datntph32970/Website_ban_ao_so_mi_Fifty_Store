using System;
using System.Collections.Generic;

namespace API.DbConects.DTOs.Admin.SanPham
{
    public class SanPhamAdminDTO
    {
        public Guid id_san_pham { get; set; }
        public string ma_san_pham { get; set; }
        public string ten_san_pham { get; set; }
        public string mo_ta { get; set; }
        public string trang_thai { get; set; }
        public string url_anh_mac_dinh { get; set; }
        public DateTime ngay_tao { get; set; }
        public DateTime? ngay_sua { get; set; }
        public string ma_nguoi_tao { get; set; }
        public string? ma_nguoi_sua { get; set; }
        public string ten_nguoi_tao { get; set; }
        public string? ten_nguoi_sua { get; set; }
        public ThuongHieuAdminDTO thuongHieu { get; set; }
        public DanhMucAdminDTO danhMuc { get; set; }
        public KieuDangAdminDTO kieuDang { get; set; }
        public ChatLieuAdminDTO chatLieu { get; set; }
        public XuatXuAdminDTO xuatXu { get; set; }
        public List<SanPhamChiTietAdminDTO> sanPhamChiTiets { get; set; }
    }

    public class ThongKeDTO
    {
        public int tong_so_luong { get; set; }
        public int so_luong_ban { get; set; }
        public decimal tong_doanh_thu { get; set; }
        public int so_don_hang { get; set; }
    }

    public class ThemSanPhamAdminDTO
    {
        public string ten_san_pham { get; set; }
        public string mo_ta { get; set; }
        public string url_anh_mac_dinh { get; set; }
        public string id_thuong_hieu { get; set; }
        public string id_danh_muc { get; set; }
        public string id_kieu_dang { get; set; }
        public string id_chat_lieu { get; set; }
        public string id_xuat_xu { get; set; }
        public List<ThemSanPhamChiTietAdminDTO> sanPhamChiTiets { get; set; }
    }

    public class SuaSanPhamAdminDTO
    {
        public string ten_san_pham { get; set; }
        public string mo_ta { get; set; }
        public string id_kieu_dang { get; set; }
        public string id_chat_lieu { get; set; }
        public string id_thuong_hieu { get; set; }
        public string id_xuat_xu { get; set; }
        public string id_danh_muc { get; set; }
        public string trang_thai { get; set; }
        public string url_anh_mac_dinh { get; set; }
        public List<SuaSanPhamChiTietAdminDTO> sanPhamChiTiets { get; set; }
    }

    public class CapNhatTrangThaiSanPhamDTO
    {
        public string trang_thai { get; set; }
    }

    public class PhanTrangSanPhamDTO
    {
        public int trang_hien_tai { get; set; }
        public int so_phan_tu_tren_trang { get; set; }
        public int tong_so_trang { get; set; }
        public int tong_so_phan_tu { get; set; }
        public decimal gia_lon_nhat { get; set; }
        public List<SanPhamAdminDTO> danh_sach { get; set; }
    }

    public class ThamSoPhanTrangSanPhamDTO
    {
        public int trang_hien_tai { get; set; } = 1;
        public int so_phan_tu_tren_trang { get; set; } = 10;
        public string? tim_kiem { get; set; }
        public string? sap_xep_theo { get; set; }
        public bool sap_xep_tang { get; set; } = true;
        public List<string>? id_thuong_hieu { get; set; }
        public List<string>? id_danh_muc { get; set; }
        public List<string>? id_kieu_dang { get; set; }
        public List<string>? id_chat_lieu { get; set; }
        public List<string>? id_xuat_xu { get; set; }
        public decimal? gia_tu { get; set; }
        public decimal? gia_den { get; set; }
    }
    public class SanPhamDTO
    {
        public Guid id_san_pham { get; set; }
        public string ma_san_pham { get; set; }
        public string ten_san_pham { get; set; }
        public string mo_ta { get; set; }
        public string trang_thai { get; set; }
        public string url_anh_mac_dinh { get; set; }
        public string ten_thuong_hieu { get; set; }
        public string ten_danh_muc { get; set; }
        public string ten_kieu_dang { get; set; }
        public string ten_chat_lieu { get; set; }
        public string ten_xuat_xu { get; set; }
        public DateTime ngay_tao { get; set; }
        public DateTime? ngay_sua { get; set; }
        public List<SanPhamChiTietDTO> sanPhamChiTiets { get; set; }
    }
}