using System;
using System.Collections.Generic;
using API.DbConects.DTOs.Admin.SanPham;

namespace API.DbConects.DTOs.Admin.HoaDon
{
    public class HoaDonChiTietAdminDTO
    {
        public Guid id_hoa_don_chi_tiet { get; set; }
        public string ma_hoa_don_chi_tiet { get; set; }
        public Guid id_san_pham_chi_tiet { get; set; }
        public string ma_san_pham_chi_tiet { get; set; }
        public string ten_san_pham { get; set; }
        public string ten_mau_sac { get; set; }
        public string ten_kich_co { get; set; }
        public int so_luong { get; set; }
        public decimal don_gia { get; set; }
        public decimal gia_sau_giam_gia { get; set; }
        public decimal gia_tri_khuyen_mai_cua_hoa_don_cho_hdct { get; set; }
        public decimal thanh_tien { get; set; }
        public string? ghi_chu { get; set; }
        public string trang_thai { get; set; }
        public DateTime? ngay_sua { get; set; }
        public string? ten_nhan_vien_xu_ly { get; set; }
        public decimal gia_sp_dang_ban { get; set; }
        public decimal gia_sau_giam_sp_dang_ban { get; set; }
        public string? url_anh { get; set; }
        public SanPhamChiTiet_HoaDonChiTietAdminDTO? sanPhamChiTiet { get; set; }
        public HoaDonAdminDTO? hoaDon { get; set; }
        public NhanVien_HoaDonAdminDTO? nhanVien { get; set; }
    }

    public class ThemHoaDonChiTietAdminDTO
    {
        public Guid id_san_pham_chi_tiet { get; set; }
        public int so_luong { get; set; }
    }
    public class SanPhamChiTiet_HoaDonChiTietAdminDTO
    {
        public Guid id_san_pham_chi_tiet { get; set; }
        public string ma_san_pham_chi_tiet { get; set; }
        public string ten_san_pham { get; set; }
        public string ten_mau_sac { get; set; }
        public string url_anh_san_pham_chi_tiet { get; set; }
        public string ten_kich_co { get; set; }
    }

    public class SuaHoaDonChiTietAdminDTO
    {
        public int so_luong { get; set; }
        public string trang_thai { get; set; }
    }
    public class HoaDonChiTietBanTaiQuayDTO
    {
        public Guid? id_hoa_don_chi_tiet { get; set; }
        public Guid id_hoa_don { get; set; }
        public Guid id_san_pham_chi_tiet { get; set; }
        public int so_luong { get; set; }
        public string? ghi_chu { get; set; }
    }
}