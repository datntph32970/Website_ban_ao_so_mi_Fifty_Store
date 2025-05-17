using System;
using System.Collections.Generic;

namespace API.DbConects.DTOs.Admin.HoaDon
{
    public class HoaDonAdminDTO
    {
        public Guid id_hoa_don { get; set; }
        public string ma_hoa_don { get; set; }
        public Guid? id_khach_hang { get; set; }
        public string ten_khach_hang { get; set; }
        public string? sdt_khach_hang { get; set; }
        public string? id_phuong_thuc_thanh_toan { get; set; }
        public string? dia_chi_nhan_hang { get; set; }
        public string? ghi_chu { get; set; }
        public decimal? so_tien_khach_tra { get; set; }
        public decimal? so_tien_thua_tra_khach { get; set; }
        public string loai_hoa_don { get; set; }
        public decimal tong_tien_don_hang { get; set; }
        public decimal? phi_van_chuyen { get; set; }
        public decimal? so_tien_khuyen_mai { get; set; }
        public decimal tong_tien_phai_thanh_toan { get; set; }
        public string trang_thai { get; set; }
        public string ten_phuong_thuc_thanh_toan { get; set; }
        public DateTime ngay_tao { get; set; }
        public string ten_nguoi_xu_ly { get; set; }
        public NhanVien_HoaDonAdminDTO nhanVienXuLy { get; set; }
        public KhuyenMai_HoaDonAdminDTO? khuyenMai { get; set; }
        public KhachHang_HoaDonAdminDTO? khachHang { get; set; }
        public List<HoaDonChiTietAdminDTO>? hoaDonChiTiets { get; set; }
        public CuaHang_HoaDonAdminDTO? cuaHang { get; set; }
    }
    public class CuaHang_HoaDonAdminDTO
    {
        public Guid id_cua_hang { get; set; }
        public string ten_cua_hang { get; set; }
        public string website { get; set; }
        public string email { get; set; }
        public string sdt { get; set; }
        public string dia_chi { get; set; }
        public string mo_ta { get; set; }
        public string? hinh_anh_logo_cua_hang_url { get; set; }
    }
    public class KhuyenMai_HoaDonAdminDTO
    {
        public Guid? id_khuyen_mai { get; set; }
        public string? ten_khuyen_mai { get; set; }
        public string? ma_khuyen_mai { get; set; }
        public string? loai_khuyen_mai { get; set; }
        public decimal? gia_tri_khuyen_mai { get; set; }
        public decimal? gia_tri_giam_toi_da { get; set; }

    }
    public class KhachHang_HoaDonAdminDTO
    {
        public Guid? id_khach_hang { get; set; }
        public string? ma_khach_hang { get; set; }
        public string? ten_khach_hang { get; set; }
        public string? sdt_khach_hang { get; set; }
    }
    public class NhanVien_HoaDonAdminDTO
    {
        public Guid? id_nhan_vien { get; set; }
        public string? ma_nhan_vien { get; set; }
        public string? ten_nhan_vien { get; set; }
    }

    public class ThemHoaDonAdminDTO
    {
        public Guid id_khach_hang { get; set; }
        public string dia_chi { get; set; }
        public string ghi_chu { get; set; }
        public string phuong_thuc_thanh_toan { get; set; }
        public List<ThemHoaDonChiTietAdminDTO> hoaDonChiTiets { get; set; }
    }

    public class SuaHoaDonBanTaiQuayDTO
    {
        public Guid? id_khach_hang { get; set; }
        public string? dia_chi_nhan_hang { get; set; }
        public string? ghi_chu { get; set; }
        public string? trang_thai { get; set; }
        public Guid? id_phuong_thuc_thanh_toan { get; set; }
        public Guid? id_khuyen_mai { get; set; }
        public List<HoaDonChiTietBanTaiQuayDTO> hoaDonChiTiets { get; set; }
    }
    //tham số phân trang
    public class ThamSoPhanTrangHoaDonAdminDTO
    {
        public int trang_hien_tai { get; set; }
        public int so_phan_tu_tren_trang { get; set; }
        public string? tim_kiem { get; set; }
        public string? trang_thai { get; set; }
        public string? loai_hoa_don { get; set; }
        public string? id_phuong_thuc_thanh_toan { get; set; }
        public string? ngay_tao_tu { get; set; }
        public string? ngay_tao_den { get; set; }

    }//phân trang
    public class PhanTrangHoaDonAdminDTO
    {
        public int trang_hien_tai { get; set; }
        public int so_phan_tu_tren_trang { get; set; }
        public int tong_so_trang { get; set; }
        public int tong_so_phan_tu { get; set; }
        public List<HoaDonAdminDTO> danh_sach { get; set; }
    }
}