using System;
using System.Collections.Generic;
using API.DbConects.DTOs.Admin.KhuyenMai;

namespace API.DbConects.DTOs.Admin.SanPham
{
    public class SanPhamChiTietAdminDTO
    {
        public Guid id_san_pham_chi_tiet { get; set; }
        public string ma_san_pham_chi_tiet { get; set; }
        public int so_luong { get; set; }
        public decimal gia_ban { get; set; }
        public decimal gia_nhap { get; set; }
        public string trang_thai { get; set; }
        public DateTime ngay_tao { get; set; }
        public string ten_nguoi_tao { get; set; }
        public string ma_nguoi_tao { get; set; }
        public string ma_nguoi_sua { get; set; }
        public DateTime? ngay_sua { get; set; }
        public string ten_nguoi_sua { get; set; }
        public MauSacAdminDTO mauSac { get; set; }
        public KichCoAdminDTO kichCo { get; set; }
        public List<HinhAnhSanPhamChiTietAdminDTO> hinhAnhSanPhamChiTiets { get; set; }
        public GiamGiaAdminDTO giamGia { get; set; }
    }

    public class ThemSanPhamChiTietAdminDTO
    {
        public Guid? id_san_pham { get; set; }
        public Guid id_mau_sac { get; set; }
        public Guid id_kich_co { get; set; }
        public string? id_giam_gia { get; set; }
        public int so_luong { get; set; }
        public decimal gia_nhap { get; set; }
        public decimal gia_ban { get; set; }
        public List<ThemHinhAnhSanPhamChiTietAdminDTO> them_hinh_anh_spcts { get; set; }
    }

    public class SuaSanPhamChiTietAdminDTO
    {
        public Guid id_san_pham_chi_tiet { get; set; }
        public Guid id_mau_sac { get; set; }
        public Guid id_kich_co { get; set; }
        public string? id_giam_gia { get; set; }
        public int so_luong { get; set; }
        public decimal gia_ban { get; set; }
        public decimal gia_nhap { get; set; }
        public string trang_thai { get; set; }
        public List<ThemHinhAnhSanPhamChiTietAdminDTO> them_hinh_anh_spcts { get; set; }
    }
    public class ThemHinhAnhSanPhamChiTietAdminDTO
    {
        public string? id_san_pham_chi_tiet { get; set; }
        public string hinh_anh_urls { get; set; }
    }
    public class HinhAnhSanPhamChiTietAdminDTO
    {

        public Guid id_hinh_anh { get; set; }
        public string hinh_anh_urls { get; set; }
    }

    public class SuaHinhAnhSanPhamChiTietAdminDTO
    {
        public string hinh_anh_urls { get; set; }
        public bool mac_dinh { get; set; }
    }
    public class SanPhamChiTietDTO
    {
        public Guid id_san_pham_chi_tiet { get; set; }
        public string ma_san_pham_chi_tiet { get; set; }
        public int so_luong { get; set; }
        public decimal gia_ban { get; set; }
        public decimal gia_nhap { get; set; }
        public string trang_thai { get; set; }
        public DateTime ngay_tao { get; set; }
        public DateTime? ngay_sua { get; set; }
        public string ten_mau_sac { get; set; }
        public string ten_kich_co { get; set; }
        public List<HinhAnhSanPhamChiTietAdminDTO> hinhAnhSanPhamChiTiets { get; set; }
        public GiamGiaAdminDTO giamGia { get; set; }
    }

}