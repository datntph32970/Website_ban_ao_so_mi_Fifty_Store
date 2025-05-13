using System;
using System.Collections.Generic;
using API.DbConects.DTOs.Admin.HoaDon;

namespace API.DbConects.DTOs.Admin.TaiKhoan
{
    public class TaiKhoanAdminDTO
    {
        public Guid id_tai_khoan { get; set; }
        public string ma_tai_khoan { get; set; }
        public string ten_dang_nhap { get; set; }
        public string mat_khau { get; set; }
        public string ho_ten { get; set; }
        public string so_dien_thoai { get; set; }
        public string email { get; set; }
        public string dia_chi { get; set; }
        public string vai_tro { get; set; }
        public string trang_thai { get; set; }
        public DateTime ngay_tao { get; set; }
        public string ten_nguoi_tao { get; set; }
        public DateTime? ngay_sua { get; set; }
        public string ten_nguoi_sua { get; set; }
        public List<HoaDonAdminDTO> HoaDons { get; set; }
    }

    public class ThemTaiKhoanAdminDTO
    {
        public string ten_dang_nhap { get; set; }
        public string mat_khau { get; set; }
        public string ho_ten { get; set; }
        public string so_dien_thoai { get; set; }
        public string email { get; set; }
        public string dia_chi { get; set; }
        public string vai_tro { get; set; }
    }

    public class SuaTaiKhoanAdminDTO
    {
        public string ho_ten { get; set; }
        public string so_dien_thoai { get; set; }
        public string email { get; set; }
        public string dia_chi { get; set; }
        public string vai_tro { get; set; }
        public string trang_thai { get; set; }
    }
    public class SuaTaiKhoanNhanVienDTO
    {

        public string id_nhan_vien { get; set; }
        public string? chuc_vu { get; set; }
        public string? trang_thai { get; set; }
    }

    public class DoiMatKhauAdminDTO
    {
        public string mat_khau_cu { get; set; }
        public string mat_khau_moi { get; set; }
        public string xac_nhan_mat_khau_moi { get; set; }
    }
    public enum ChucVuTaiKhoan
    {
        Admin,
        NhanVien,
        KhachHang
    }
    public enum GioiTinhTaiKhoan
    {
        Nam,
        Nu,
        Khac
    }
    public enum TrangThaiTaiKhoan
    {
        HoatDong,
        KhongHoatDong
    }
}