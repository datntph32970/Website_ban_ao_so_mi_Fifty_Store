using System;
using System.Collections.Generic;
using API.DbConects.DTOs.Client.HoaDon;

namespace API.DbConects.DTOs.Client.TaiKhoan
{
    public class TaiKhoanClientDTO
    {
        public Guid id_tai_khoan { get; set; }
        public string ma_tai_khoan { get; set; }
        public string ho_ten { get; set; }
        public string so_dien_thoai { get; set; }
        public string email { get; set; }
        public string dia_chi { get; set; }
        public List<HoaDonClientDTO> HoaDons { get; set; }
    }
    public class ThongTinNguoiDung
    {
        public string id_tai_khoan { get; set; }
        public string ma_tai_khoan { get; set; }
        public string ten_dang_nhap { get; set; }
        public bool da_doi_mat_khau { get; set; }
        public string chuc_vu { get; set; }
    }
    public class DangKyClientDTO
    {
        public string ten_dang_nhap { get; set; }
        public string mat_khau { get; set; }
        public string xac_nhan_mat_khau { get; set; }
    }

    public class DangNhapClientDTO
    {
        public string ten_dang_nhap { get; set; }
        public string mat_khau { get; set; }
    }

    public class SuaThongTinClientDTO
    {
        public string ho_ten { get; set; }
        public string so_dien_thoai { get; set; }
        public string email { get; set; }
        public string dia_chi { get; set; }
    }

    public class DoiMatKhauClientDTO
    {
        public string ten_dang_nhap { get; set; }
        public string mat_khau_cu { get; set; }
        public string mat_khau_moi { get; set; }
        public string xac_nhan_mat_khau_moi { get; set; }
    }
}