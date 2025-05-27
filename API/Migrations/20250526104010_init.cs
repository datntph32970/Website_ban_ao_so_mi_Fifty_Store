using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TaiKhoans",
                columns: table => new
                {
                    id_tai_khoan = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ma_tai_khoan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ten_dang_nhap = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    mat_khau = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    trang_thai = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    da_doi_mat_khau = table.Column<bool>(type: "bit", nullable: false),
                    chuc_vu = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaiKhoans", x => x.id_tai_khoan);
                });

            migrationBuilder.CreateTable(
                name: "KhachHangs",
                columns: table => new
                {
                    id_khach_hang = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    id_tai_khoan = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ma_khach_hang = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ten_khach_hang = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ngay_sinh = table.Column<DateOnly>(type: "date", nullable: true),
                    so_dien_thoai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    gioi_tinh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    trang_thai = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KhachHangs", x => x.id_khach_hang);
                    table.ForeignKey(
                        name: "FK_KhachHangs_TaiKhoans_id_tai_khoan",
                        column: x => x.id_tai_khoan,
                        principalTable: "TaiKhoans",
                        principalColumn: "id_tai_khoan",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NhanViens",
                columns: table => new
                {
                    id_nhan_vien = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    id_tai_khoan = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ma_nhan_vien = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ten_nhan_vien = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    so_dien_thoai = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ngay_sinh = table.Column<DateTime>(type: "datetime2", nullable: false),
                    dia_chi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    cccd = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    gioi_tinh = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    trang_thai = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    id_nguoi_tao = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ngay_sua = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NhanViens", x => x.id_nhan_vien);
                    table.ForeignKey(
                        name: "FK_NhanViens_TaiKhoans_id_nguoi_tao",
                        column: x => x.id_nguoi_tao,
                        principalTable: "TaiKhoans",
                        principalColumn: "id_tai_khoan",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NhanViens_TaiKhoans_id_tai_khoan",
                        column: x => x.id_tai_khoan,
                        principalTable: "TaiKhoans",
                        principalColumn: "id_tai_khoan",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DiaChis",
                columns: table => new
                {
                    id_dia_chi = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    id_khach_hang = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    tinh = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    huyen = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    xa = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    dia_chi_cu_the = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    so_dien_thoai = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ten_nguoi_nhan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    dia_chi_mac_dinh = table.Column<bool>(type: "bit", nullable: false),
                    ngay_tao = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ngay_sua = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiaChis", x => x.id_dia_chi);
                    table.ForeignKey(
                        name: "FK_DiaChis_KhachHangs_id_khach_hang",
                        column: x => x.id_khach_hang,
                        principalTable: "KhachHangs",
                        principalColumn: "id_khach_hang",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChatLieus",
                columns: table => new
                {
                    id_chat_lieu = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ma_chat_lieu = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ten_chat_lieu = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    mo_ta = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    trang_thai = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    id_nguoi_tao = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    id_nguoi_sua = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ngay_sua = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatLieus", x => x.id_chat_lieu);
                    table.ForeignKey(
                        name: "FK_ChatLieus_NhanViens_id_nguoi_sua",
                        column: x => x.id_nguoi_sua,
                        principalTable: "NhanViens",
                        principalColumn: "id_nhan_vien",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChatLieus_NhanViens_id_nguoi_tao",
                        column: x => x.id_nguoi_tao,
                        principalTable: "NhanViens",
                        principalColumn: "id_nhan_vien",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DanhMucs",
                columns: table => new
                {
                    id_danh_muc = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ma_danh_muc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ten_danh_muc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    mo_ta = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    trang_thai = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    id_nguoi_tao = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    id_nguoi_sua = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ngay_sua = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DanhMucs", x => x.id_danh_muc);
                    table.ForeignKey(
                        name: "FK_DanhMucs_NhanViens_id_nguoi_sua",
                        column: x => x.id_nguoi_sua,
                        principalTable: "NhanViens",
                        principalColumn: "id_nhan_vien",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DanhMucs_NhanViens_id_nguoi_tao",
                        column: x => x.id_nguoi_tao,
                        principalTable: "NhanViens",
                        principalColumn: "id_nhan_vien",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GiamGias",
                columns: table => new
                {
                    id_giam_gia = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ma_giam_gia = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ten_giam_gia = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    mo_ta = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    kieu_giam_gia = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    gia_tri_giam = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    so_luong_da_su_dung = table.Column<int>(type: "int", nullable: false),
                    thoi_gian_bat_dau = table.Column<DateTime>(type: "datetime2", nullable: false),
                    thoi_gian_ket_thuc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    trang_thai = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime2", nullable: false),
                    id_nguoi_tao = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    id_nguoi_cap_nhat = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GiamGias", x => x.id_giam_gia);
                    table.ForeignKey(
                        name: "FK_GiamGias_NhanViens_id_nguoi_cap_nhat",
                        column: x => x.id_nguoi_cap_nhat,
                        principalTable: "NhanViens",
                        principalColumn: "id_nhan_vien",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GiamGias_NhanViens_id_nguoi_tao",
                        column: x => x.id_nguoi_tao,
                        principalTable: "NhanViens",
                        principalColumn: "id_nhan_vien",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HinhAnhs",
                columns: table => new
                {
                    id_hinh_anh = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ma_hinh_anh = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    id_nguoi_tao = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    id_nguoi_sua = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ngay_sua = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HinhAnhs", x => x.id_hinh_anh);
                    table.ForeignKey(
                        name: "FK_HinhAnhs_NhanViens_id_nguoi_sua",
                        column: x => x.id_nguoi_sua,
                        principalTable: "NhanViens",
                        principalColumn: "id_nhan_vien",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HinhAnhs_NhanViens_id_nguoi_tao",
                        column: x => x.id_nguoi_tao,
                        principalTable: "NhanViens",
                        principalColumn: "id_nhan_vien",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KhuyenMais",
                columns: table => new
                {
                    id_khuyen_mai = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ma_khuyen_mai = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ten_khuyen_mai = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    mo_ta = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    kieu_khuyen_mai = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    gia_tri_giam = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    gia_tri_giam_toi_da = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    gia_tri_don_hang_toi_thieu = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    so_luong_toi_da = table.Column<int>(type: "int", nullable: false),
                    so_luong_da_su_dung = table.Column<int>(type: "int", nullable: false),
                    thoi_gian_bat_dau = table.Column<DateTime>(type: "datetime2", nullable: false),
                    thoi_gian_ket_thuc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    trang_thai = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    id_nguoi_tao = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    id_nguoi_sua = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ngay_sua = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KhuyenMais", x => x.id_khuyen_mai);
                    table.ForeignKey(
                        name: "FK_KhuyenMais_NhanViens_id_nguoi_sua",
                        column: x => x.id_nguoi_sua,
                        principalTable: "NhanViens",
                        principalColumn: "id_nhan_vien",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_KhuyenMais_NhanViens_id_nguoi_tao",
                        column: x => x.id_nguoi_tao,
                        principalTable: "NhanViens",
                        principalColumn: "id_nhan_vien",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KichCos",
                columns: table => new
                {
                    id_kich_co = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ma_kich_co = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ten_kich_co = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    mo_ta = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    trang_thai = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    id_nguoi_tao = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    id_nguoi_sua = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ngay_sua = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KichCos", x => x.id_kich_co);
                    table.ForeignKey(
                        name: "FK_KichCos_NhanViens_id_nguoi_sua",
                        column: x => x.id_nguoi_sua,
                        principalTable: "NhanViens",
                        principalColumn: "id_nhan_vien",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_KichCos_NhanViens_id_nguoi_tao",
                        column: x => x.id_nguoi_tao,
                        principalTable: "NhanViens",
                        principalColumn: "id_nhan_vien",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KieuDangs",
                columns: table => new
                {
                    id_kieu_dang = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ma_kieu_dang = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ten_kieu_dang = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    mo_ta = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    trang_thai = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    id_nguoi_tao = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    id_nguoi_sua = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ngay_sua = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KieuDangs", x => x.id_kieu_dang);
                    table.ForeignKey(
                        name: "FK_KieuDangs_NhanViens_id_nguoi_sua",
                        column: x => x.id_nguoi_sua,
                        principalTable: "NhanViens",
                        principalColumn: "id_nhan_vien",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_KieuDangs_NhanViens_id_nguoi_tao",
                        column: x => x.id_nguoi_tao,
                        principalTable: "NhanViens",
                        principalColumn: "id_nhan_vien",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MauSacs",
                columns: table => new
                {
                    id_mau_sac = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ma_mau_sac = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ten_mau_sac = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    mo_ta = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    trang_thai = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    id_nguoi_tao = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    id_nguoi_sua = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ngay_sua = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MauSacs", x => x.id_mau_sac);
                    table.ForeignKey(
                        name: "FK_MauSacs_NhanViens_id_nguoi_sua",
                        column: x => x.id_nguoi_sua,
                        principalTable: "NhanViens",
                        principalColumn: "id_nhan_vien",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MauSacs_NhanViens_id_nguoi_tao",
                        column: x => x.id_nguoi_tao,
                        principalTable: "NhanViens",
                        principalColumn: "id_nhan_vien",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PhuongThucThanhToans",
                columns: table => new
                {
                    id_phuong_thuc_thanh_toan = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ten_phuong_thuc_thanh_toan = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ma_phuong_thuc_thanh_toan = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    mo_ta = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    trang_thai = table.Column<bool>(type: "bit", nullable: false),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime2", nullable: true),
                    id_nguoi_tao = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    id_nguoi_sua = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhuongThucThanhToans", x => x.id_phuong_thuc_thanh_toan);
                    table.ForeignKey(
                        name: "FK_PhuongThucThanhToans_NhanViens_id_nguoi_sua",
                        column: x => x.id_nguoi_sua,
                        principalTable: "NhanViens",
                        principalColumn: "id_nhan_vien",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhuongThucThanhToans_NhanViens_id_nguoi_tao",
                        column: x => x.id_nguoi_tao,
                        principalTable: "NhanViens",
                        principalColumn: "id_nhan_vien",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ThuongHieus",
                columns: table => new
                {
                    id_thuong_hieu = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ma_thuong_hieu = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ten_thuong_hieu = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    mo_ta = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    trang_thai = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    id_nguoi_tao = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    id_nguoi_sua = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ngay_sua = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThuongHieus", x => x.id_thuong_hieu);
                    table.ForeignKey(
                        name: "FK_ThuongHieus_NhanViens_id_nguoi_sua",
                        column: x => x.id_nguoi_sua,
                        principalTable: "NhanViens",
                        principalColumn: "id_nhan_vien",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ThuongHieus_NhanViens_id_nguoi_tao",
                        column: x => x.id_nguoi_tao,
                        principalTable: "NhanViens",
                        principalColumn: "id_nhan_vien",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "XuatXus",
                columns: table => new
                {
                    id_xuat_xu = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ma_xuat_xu = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ten_xuat_xu = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    trang_thai = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    mo_ta = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ngay_sua = table.Column<DateTime>(type: "datetime2", nullable: true),
                    id_nguoi_tao = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    id_nguoi_sua = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_XuatXus", x => x.id_xuat_xu);
                    table.ForeignKey(
                        name: "FK_XuatXus_NhanViens_id_nguoi_sua",
                        column: x => x.id_nguoi_sua,
                        principalTable: "NhanViens",
                        principalColumn: "id_nhan_vien",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_XuatXus_NhanViens_id_nguoi_tao",
                        column: x => x.id_nguoi_tao,
                        principalTable: "NhanViens",
                        principalColumn: "id_nhan_vien",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CuaHangs",
                columns: table => new
                {
                    id_cua_hang = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ten_cua_hang = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    website = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    sdt = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    dia_chi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    mo_ta = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    id_hinh_anh = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    id_nguoi_sua = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CuaHangs", x => x.id_cua_hang);
                    table.ForeignKey(
                        name: "FK_CuaHangs_HinhAnhs_id_hinh_anh",
                        column: x => x.id_hinh_anh,
                        principalTable: "HinhAnhs",
                        principalColumn: "id_hinh_anh",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CuaHangs_NhanViens_id_nguoi_sua",
                        column: x => x.id_nguoi_sua,
                        principalTable: "NhanViens",
                        principalColumn: "id_nhan_vien",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SanPhams",
                columns: table => new
                {
                    id_san_pham = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ma_san_pham = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ten_san_pham = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    mo_ta = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    trang_thai = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    id_nguoi_tao = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    id_nguoi_sua = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    id_anh_mac_dinh = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ngay_sua = table.Column<DateTime>(type: "datetime2", nullable: true),
                    id_chat_lieu = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    id_kieu_dang = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    id_thuong_hieu = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    id_xuat_xu = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    id_danh_muc = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SanPhams", x => x.id_san_pham);
                    table.ForeignKey(
                        name: "FK_SanPhams_ChatLieus_id_chat_lieu",
                        column: x => x.id_chat_lieu,
                        principalTable: "ChatLieus",
                        principalColumn: "id_chat_lieu",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SanPhams_DanhMucs_id_danh_muc",
                        column: x => x.id_danh_muc,
                        principalTable: "DanhMucs",
                        principalColumn: "id_danh_muc",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SanPhams_HinhAnhs_id_anh_mac_dinh",
                        column: x => x.id_anh_mac_dinh,
                        principalTable: "HinhAnhs",
                        principalColumn: "id_hinh_anh");
                    table.ForeignKey(
                        name: "FK_SanPhams_KieuDangs_id_kieu_dang",
                        column: x => x.id_kieu_dang,
                        principalTable: "KieuDangs",
                        principalColumn: "id_kieu_dang",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SanPhams_NhanViens_id_nguoi_sua",
                        column: x => x.id_nguoi_sua,
                        principalTable: "NhanViens",
                        principalColumn: "id_nhan_vien",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SanPhams_NhanViens_id_nguoi_tao",
                        column: x => x.id_nguoi_tao,
                        principalTable: "NhanViens",
                        principalColumn: "id_nhan_vien",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SanPhams_ThuongHieus_id_thuong_hieu",
                        column: x => x.id_thuong_hieu,
                        principalTable: "ThuongHieus",
                        principalColumn: "id_thuong_hieu",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SanPhams_XuatXus_id_xuat_xu",
                        column: x => x.id_xuat_xu,
                        principalTable: "XuatXus",
                        principalColumn: "id_xuat_xu",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HoaDons",
                columns: table => new
                {
                    id_hoa_don = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ma_hoa_don = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ngay_sua = table.Column<DateTime>(type: "datetime2", nullable: true),
                    tong_tien_don_hang = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    so_tien_khuyen_mai = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ghi_chu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ly_do_huy_don_hang = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tong_tien_phai_thanh_toan = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    so_tien_khach_tra = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    so_tien_thua_tra_khach = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ten_khach_hang = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ten_nhan_vien = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    sdt_khach_hang = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    phi_van_chuyen = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    dia_chi_nhan_hang = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    loai_hoa_don = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    id_khach_hang = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    id_nhan_vien_xu_ly = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    id_cua_hang = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    id_khuyen_mai = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    id_phuong_thuc_thanh_toan = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    trang_thai_hoa_don = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ly_do_tra_hang = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ngay_yeu_cau_tra_hang = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ngay_xac_nhan_tra_hang = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ngay_hoan_thanh_tra_hang = table.Column<DateTime>(type: "datetime2", nullable: true),
                    hinh_anh_tra_hang = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoaDons", x => x.id_hoa_don);
                    table.ForeignKey(
                        name: "FK_HoaDons_CuaHangs_id_cua_hang",
                        column: x => x.id_cua_hang,
                        principalTable: "CuaHangs",
                        principalColumn: "id_cua_hang");
                    table.ForeignKey(
                        name: "FK_HoaDons_KhachHangs_id_khach_hang",
                        column: x => x.id_khach_hang,
                        principalTable: "KhachHangs",
                        principalColumn: "id_khach_hang");
                    table.ForeignKey(
                        name: "FK_HoaDons_KhuyenMais_id_khuyen_mai",
                        column: x => x.id_khuyen_mai,
                        principalTable: "KhuyenMais",
                        principalColumn: "id_khuyen_mai");
                    table.ForeignKey(
                        name: "FK_HoaDons_NhanViens_id_nhan_vien_xu_ly",
                        column: x => x.id_nhan_vien_xu_ly,
                        principalTable: "NhanViens",
                        principalColumn: "id_nhan_vien",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HoaDons_PhuongThucThanhToans_id_phuong_thuc_thanh_toan",
                        column: x => x.id_phuong_thuc_thanh_toan,
                        principalTable: "PhuongThucThanhToans",
                        principalColumn: "id_phuong_thuc_thanh_toan");
                });

            migrationBuilder.CreateTable(
                name: "SanPhamChiTiets",
                columns: table => new
                {
                    id_san_pham_chi_tiet = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ma_san_pham_chi_tiet = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    so_luong = table.Column<int>(type: "int", nullable: false),
                    gia_ban = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    gia_nhap = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    trang_thai = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    id_nguoi_tao = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    id_nguoi_sua = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ngay_sua = table.Column<DateTime>(type: "datetime2", nullable: true),
                    id_san_pham = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    id_kich_co = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    id_mau_sac = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SanPhamChiTiets", x => x.id_san_pham_chi_tiet);
                    table.ForeignKey(
                        name: "FK_SanPhamChiTiets_KichCos_id_kich_co",
                        column: x => x.id_kich_co,
                        principalTable: "KichCos",
                        principalColumn: "id_kich_co",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SanPhamChiTiets_MauSacs_id_mau_sac",
                        column: x => x.id_mau_sac,
                        principalTable: "MauSacs",
                        principalColumn: "id_mau_sac",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SanPhamChiTiets_NhanViens_id_nguoi_sua",
                        column: x => x.id_nguoi_sua,
                        principalTable: "NhanViens",
                        principalColumn: "id_nhan_vien",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SanPhamChiTiets_NhanViens_id_nguoi_tao",
                        column: x => x.id_nguoi_tao,
                        principalTable: "NhanViens",
                        principalColumn: "id_nhan_vien",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SanPhamChiTiets_SanPhams_id_san_pham",
                        column: x => x.id_san_pham,
                        principalTable: "SanPhams",
                        principalColumn: "id_san_pham",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GioHangChiTiets",
                columns: table => new
                {
                    id_gio_hang_chi_tiet = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    so_luong = table.Column<int>(type: "int", nullable: false),
                    trang_thai = table.Column<bool>(type: "bit", nullable: false),
                    id_khach_hang = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    id_san_pham_chi_tiet = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GioHangChiTiets", x => x.id_gio_hang_chi_tiet);
                    table.ForeignKey(
                        name: "FK_GioHangChiTiets_KhachHangs_id_khach_hang",
                        column: x => x.id_khach_hang,
                        principalTable: "KhachHangs",
                        principalColumn: "id_khach_hang",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GioHangChiTiets_SanPhamChiTiets_id_san_pham_chi_tiet",
                        column: x => x.id_san_pham_chi_tiet,
                        principalTable: "SanPhamChiTiets",
                        principalColumn: "id_san_pham_chi_tiet",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HinhAnhSanPhamChiTiet",
                columns: table => new
                {
                    id_hinh_anh_san_pham_chi_tiet = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    id_hinh_anh = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    id_san_pham_chi_tiet = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HinhAnhSanPhamChiTiet", x => x.id_hinh_anh_san_pham_chi_tiet);
                    table.ForeignKey(
                        name: "FK_HinhAnhSanPhamChiTiet_HinhAnhs_id_hinh_anh",
                        column: x => x.id_hinh_anh,
                        principalTable: "HinhAnhs",
                        principalColumn: "id_hinh_anh",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HinhAnhSanPhamChiTiet_SanPhamChiTiets_id_san_pham_chi_tiet",
                        column: x => x.id_san_pham_chi_tiet,
                        principalTable: "SanPhamChiTiets",
                        principalColumn: "id_san_pham_chi_tiet",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HoaDonChiTiets",
                columns: table => new
                {
                    id_hoa_don_chi_tiet = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ma_hoa_don_chi_tiet = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    id_hoa_don = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    id_san_pham_chi_tiet = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ten_san_pham = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ten_mau_sac = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ten_kich_co = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    so_luong = table.Column<int>(type: "int", nullable: false),
                    don_gia = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    gia_sau_giam_gia = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    gia_tri_khuyen_mai_cua_hoa_don_cho_hdct = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    thanh_tien = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ghi_chu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    trang_thai = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    id_nhan_vien_xu_ly = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ngay_sua = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoaDonChiTiets", x => x.id_hoa_don_chi_tiet);
                    table.ForeignKey(
                        name: "FK_HoaDonChiTiets_HoaDons_id_hoa_don",
                        column: x => x.id_hoa_don,
                        principalTable: "HoaDons",
                        principalColumn: "id_hoa_don",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HoaDonChiTiets_NhanViens_id_nhan_vien_xu_ly",
                        column: x => x.id_nhan_vien_xu_ly,
                        principalTable: "NhanViens",
                        principalColumn: "id_nhan_vien");
                    table.ForeignKey(
                        name: "FK_HoaDonChiTiets_SanPhamChiTiets_id_san_pham_chi_tiet",
                        column: x => x.id_san_pham_chi_tiet,
                        principalTable: "SanPhamChiTiets",
                        principalColumn: "id_san_pham_chi_tiet",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SanPhamChiTietGiamGias",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    id_san_pham_chi_tiet = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    id_giam_gia = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SanPhamChiTietGiamGias", x => x.id);
                    table.ForeignKey(
                        name: "FK_SanPhamChiTietGiamGias_GiamGias_id_giam_gia",
                        column: x => x.id_giam_gia,
                        principalTable: "GiamGias",
                        principalColumn: "id_giam_gia",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SanPhamChiTietGiamGias_SanPhamChiTiets_id_san_pham_chi_tiet",
                        column: x => x.id_san_pham_chi_tiet,
                        principalTable: "SanPhamChiTiets",
                        principalColumn: "id_san_pham_chi_tiet",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "TaiKhoans",
                columns: new[] { "id_tai_khoan", "chuc_vu", "da_doi_mat_khau", "ma_tai_khoan", "mat_khau", "ten_dang_nhap", "trang_thai" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), "Admin", false, "TK00000001", "8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918", "admin", "HoatDong" },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "NhanVien", false, "TK00000002", "fa5a1d3e67d2193b86bc68c7db41bd84f242fe4e41146ef4a4a5441254d2a3f7", "nhanvien", "HoatDong" }
                });

            migrationBuilder.InsertData(
                table: "NhanViens",
                columns: new[] { "id_nhan_vien", "cccd", "dia_chi", "email", "gioi_tinh", "id_nguoi_tao", "id_tai_khoan", "ma_nhan_vien", "ngay_sinh", "ngay_sua", "ngay_tao", "so_dien_thoai", "ten_nhan_vien", "trang_thai" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000001"), "000000000001", "Hà Nội", "datntph32970@gmail.com", "Nam", new Guid("11111111-1111-1111-1111-111111111111"), new Guid("11111111-1111-1111-1111-111111111111"), "TK00000001", new DateTime(2004, 7, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new DateTime(2025, 4, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "0369104997", "họ và tên admin 1", "HoatDong" },
                    { new Guid("33333333-3333-3333-3333-333333333333"), "000000000000", "Hà Nội", "nthanhdat7112004@gmail.com", "Nam", new Guid("11111111-1111-1111-1111-111111111111"), new Guid("22222222-2222-2222-2222-222222222222"), "TK00000002", new DateTime(2004, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "0111111111", "họ và tên nhân viên 1", "HoatDong" }
                });

            migrationBuilder.InsertData(
                table: "CuaHangs",
                columns: new[] { "id_cua_hang", "dia_chi", "email", "id_hinh_anh", "id_nguoi_sua", "mo_ta", "sdt", "ten_cua_hang", "website" },
                values: new object[] { new Guid("00000000-0000-1234-5678-901234567890"), "Hà Nội", "fiftystore@gmail.com", null, new Guid("00000000-0000-0000-0000-000000000001"), "Cửa hàng áo sơ mi thời trang nam", "0123456789", "FIFTY STORE", "https://www.shirtstore.com" });

            migrationBuilder.InsertData(
                table: "PhuongThucThanhToans",
                columns: new[] { "id_phuong_thuc_thanh_toan", "id_nguoi_sua", "id_nguoi_tao", "ma_phuong_thuc_thanh_toan", "mo_ta", "ngay_cap_nhat", "ngay_tao", "ten_phuong_thuc_thanh_toan", "trang_thai" },
                values: new object[,]
                {
                    { new Guid("12345678-9012-3456-4213-123456781321"), null, new Guid("00000000-0000-0000-0000-000000000001"), "PTVNPAY", "Phương thức thanh toán VNPay", null, new DateTime(2025, 4, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "VNPAY", true },
                    { new Guid("12345678-9012-3456-7890-123456789012"), null, new Guid("00000000-0000-0000-0000-000000000001"), "PTTIENMAT", "Phương thức thanh toán tiền mặt", null, new DateTime(2025, 4, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "Tiền mặt", true }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChatLieus_id_nguoi_sua",
                table: "ChatLieus",
                column: "id_nguoi_sua");

            migrationBuilder.CreateIndex(
                name: "IX_ChatLieus_id_nguoi_tao",
                table: "ChatLieus",
                column: "id_nguoi_tao");

            migrationBuilder.CreateIndex(
                name: "IX_CuaHangs_id_hinh_anh",
                table: "CuaHangs",
                column: "id_hinh_anh",
                unique: true,
                filter: "[id_hinh_anh] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CuaHangs_id_nguoi_sua",
                table: "CuaHangs",
                column: "id_nguoi_sua",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DanhMucs_id_nguoi_sua",
                table: "DanhMucs",
                column: "id_nguoi_sua");

            migrationBuilder.CreateIndex(
                name: "IX_DanhMucs_id_nguoi_tao",
                table: "DanhMucs",
                column: "id_nguoi_tao");

            migrationBuilder.CreateIndex(
                name: "IX_DiaChis_id_khach_hang",
                table: "DiaChis",
                column: "id_khach_hang");

            migrationBuilder.CreateIndex(
                name: "IX_GiamGias_id_nguoi_cap_nhat",
                table: "GiamGias",
                column: "id_nguoi_cap_nhat");

            migrationBuilder.CreateIndex(
                name: "IX_GiamGias_id_nguoi_tao",
                table: "GiamGias",
                column: "id_nguoi_tao");

            migrationBuilder.CreateIndex(
                name: "IX_GioHangChiTiets_id_khach_hang",
                table: "GioHangChiTiets",
                column: "id_khach_hang");

            migrationBuilder.CreateIndex(
                name: "IX_GioHangChiTiets_id_san_pham_chi_tiet",
                table: "GioHangChiTiets",
                column: "id_san_pham_chi_tiet");

            migrationBuilder.CreateIndex(
                name: "IX_HinhAnhs_id_nguoi_sua",
                table: "HinhAnhs",
                column: "id_nguoi_sua");

            migrationBuilder.CreateIndex(
                name: "IX_HinhAnhs_id_nguoi_tao",
                table: "HinhAnhs",
                column: "id_nguoi_tao");

            migrationBuilder.CreateIndex(
                name: "IX_HinhAnhSanPhamChiTiet_id_hinh_anh",
                table: "HinhAnhSanPhamChiTiet",
                column: "id_hinh_anh");

            migrationBuilder.CreateIndex(
                name: "IX_HinhAnhSanPhamChiTiet_id_san_pham_chi_tiet",
                table: "HinhAnhSanPhamChiTiet",
                column: "id_san_pham_chi_tiet");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDonChiTiets_id_hoa_don",
                table: "HoaDonChiTiets",
                column: "id_hoa_don");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDonChiTiets_id_nhan_vien_xu_ly",
                table: "HoaDonChiTiets",
                column: "id_nhan_vien_xu_ly");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDonChiTiets_id_san_pham_chi_tiet",
                table: "HoaDonChiTiets",
                column: "id_san_pham_chi_tiet");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDons_id_cua_hang",
                table: "HoaDons",
                column: "id_cua_hang");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDons_id_khach_hang",
                table: "HoaDons",
                column: "id_khach_hang");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDons_id_khuyen_mai",
                table: "HoaDons",
                column: "id_khuyen_mai");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDons_id_nhan_vien_xu_ly",
                table: "HoaDons",
                column: "id_nhan_vien_xu_ly");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDons_id_phuong_thuc_thanh_toan",
                table: "HoaDons",
                column: "id_phuong_thuc_thanh_toan");

            migrationBuilder.CreateIndex(
                name: "IX_KhachHangs_id_tai_khoan",
                table: "KhachHangs",
                column: "id_tai_khoan",
                unique: true,
                filter: "[id_tai_khoan] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_KhuyenMais_id_nguoi_sua",
                table: "KhuyenMais",
                column: "id_nguoi_sua");

            migrationBuilder.CreateIndex(
                name: "IX_KhuyenMais_id_nguoi_tao",
                table: "KhuyenMais",
                column: "id_nguoi_tao");

            migrationBuilder.CreateIndex(
                name: "IX_KichCos_id_nguoi_sua",
                table: "KichCos",
                column: "id_nguoi_sua");

            migrationBuilder.CreateIndex(
                name: "IX_KichCos_id_nguoi_tao",
                table: "KichCos",
                column: "id_nguoi_tao");

            migrationBuilder.CreateIndex(
                name: "IX_KieuDangs_id_nguoi_sua",
                table: "KieuDangs",
                column: "id_nguoi_sua");

            migrationBuilder.CreateIndex(
                name: "IX_KieuDangs_id_nguoi_tao",
                table: "KieuDangs",
                column: "id_nguoi_tao");

            migrationBuilder.CreateIndex(
                name: "IX_MauSacs_id_nguoi_sua",
                table: "MauSacs",
                column: "id_nguoi_sua");

            migrationBuilder.CreateIndex(
                name: "IX_MauSacs_id_nguoi_tao",
                table: "MauSacs",
                column: "id_nguoi_tao");

            migrationBuilder.CreateIndex(
                name: "IX_NhanViens_id_nguoi_tao",
                table: "NhanViens",
                column: "id_nguoi_tao");

            migrationBuilder.CreateIndex(
                name: "IX_NhanViens_id_tai_khoan",
                table: "NhanViens",
                column: "id_tai_khoan",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PhuongThucThanhToans_id_nguoi_sua",
                table: "PhuongThucThanhToans",
                column: "id_nguoi_sua");

            migrationBuilder.CreateIndex(
                name: "IX_PhuongThucThanhToans_id_nguoi_tao",
                table: "PhuongThucThanhToans",
                column: "id_nguoi_tao");

            migrationBuilder.CreateIndex(
                name: "IX_SanPhamChiTietGiamGias_id_giam_gia",
                table: "SanPhamChiTietGiamGias",
                column: "id_giam_gia");

            migrationBuilder.CreateIndex(
                name: "IX_SanPhamChiTietGiamGias_id_san_pham_chi_tiet",
                table: "SanPhamChiTietGiamGias",
                column: "id_san_pham_chi_tiet");

            migrationBuilder.CreateIndex(
                name: "IX_SanPhamChiTiets_id_kich_co",
                table: "SanPhamChiTiets",
                column: "id_kich_co");

            migrationBuilder.CreateIndex(
                name: "IX_SanPhamChiTiets_id_mau_sac",
                table: "SanPhamChiTiets",
                column: "id_mau_sac");

            migrationBuilder.CreateIndex(
                name: "IX_SanPhamChiTiets_id_nguoi_sua",
                table: "SanPhamChiTiets",
                column: "id_nguoi_sua");

            migrationBuilder.CreateIndex(
                name: "IX_SanPhamChiTiets_id_nguoi_tao",
                table: "SanPhamChiTiets",
                column: "id_nguoi_tao");

            migrationBuilder.CreateIndex(
                name: "IX_SanPhamChiTiets_id_san_pham",
                table: "SanPhamChiTiets",
                column: "id_san_pham");

            migrationBuilder.CreateIndex(
                name: "IX_SanPhams_id_anh_mac_dinh",
                table: "SanPhams",
                column: "id_anh_mac_dinh",
                unique: true,
                filter: "[id_anh_mac_dinh] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SanPhams_id_chat_lieu",
                table: "SanPhams",
                column: "id_chat_lieu");

            migrationBuilder.CreateIndex(
                name: "IX_SanPhams_id_danh_muc",
                table: "SanPhams",
                column: "id_danh_muc");

            migrationBuilder.CreateIndex(
                name: "IX_SanPhams_id_kieu_dang",
                table: "SanPhams",
                column: "id_kieu_dang");

            migrationBuilder.CreateIndex(
                name: "IX_SanPhams_id_nguoi_sua",
                table: "SanPhams",
                column: "id_nguoi_sua");

            migrationBuilder.CreateIndex(
                name: "IX_SanPhams_id_nguoi_tao",
                table: "SanPhams",
                column: "id_nguoi_tao");

            migrationBuilder.CreateIndex(
                name: "IX_SanPhams_id_thuong_hieu",
                table: "SanPhams",
                column: "id_thuong_hieu");

            migrationBuilder.CreateIndex(
                name: "IX_SanPhams_id_xuat_xu",
                table: "SanPhams",
                column: "id_xuat_xu");

            migrationBuilder.CreateIndex(
                name: "IX_ThuongHieus_id_nguoi_sua",
                table: "ThuongHieus",
                column: "id_nguoi_sua");

            migrationBuilder.CreateIndex(
                name: "IX_ThuongHieus_id_nguoi_tao",
                table: "ThuongHieus",
                column: "id_nguoi_tao");

            migrationBuilder.CreateIndex(
                name: "IX_XuatXus_id_nguoi_sua",
                table: "XuatXus",
                column: "id_nguoi_sua");

            migrationBuilder.CreateIndex(
                name: "IX_XuatXus_id_nguoi_tao",
                table: "XuatXus",
                column: "id_nguoi_tao");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DiaChis");

            migrationBuilder.DropTable(
                name: "GioHangChiTiets");

            migrationBuilder.DropTable(
                name: "HinhAnhSanPhamChiTiet");

            migrationBuilder.DropTable(
                name: "HoaDonChiTiets");

            migrationBuilder.DropTable(
                name: "SanPhamChiTietGiamGias");

            migrationBuilder.DropTable(
                name: "HoaDons");

            migrationBuilder.DropTable(
                name: "GiamGias");

            migrationBuilder.DropTable(
                name: "SanPhamChiTiets");

            migrationBuilder.DropTable(
                name: "CuaHangs");

            migrationBuilder.DropTable(
                name: "KhachHangs");

            migrationBuilder.DropTable(
                name: "KhuyenMais");

            migrationBuilder.DropTable(
                name: "PhuongThucThanhToans");

            migrationBuilder.DropTable(
                name: "KichCos");

            migrationBuilder.DropTable(
                name: "MauSacs");

            migrationBuilder.DropTable(
                name: "SanPhams");

            migrationBuilder.DropTable(
                name: "ChatLieus");

            migrationBuilder.DropTable(
                name: "DanhMucs");

            migrationBuilder.DropTable(
                name: "HinhAnhs");

            migrationBuilder.DropTable(
                name: "KieuDangs");

            migrationBuilder.DropTable(
                name: "ThuongHieus");

            migrationBuilder.DropTable(
                name: "XuatXus");

            migrationBuilder.DropTable(
                name: "NhanViens");

            migrationBuilder.DropTable(
                name: "TaiKhoans");
        }
    }
}
