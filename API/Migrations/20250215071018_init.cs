using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PhuongThucThanhToans",
                columns: table => new
                {
                    id_phuong_thuc_thanh_toan = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ten_phuong_thuc_thanh_toan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ma_phuong_thuc_thanh_toan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    mo_ta = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    trang_thai = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhuongThucThanhToans", x => x.id_phuong_thuc_thanh_toan);
                });

            migrationBuilder.CreateTable(
                name: "TaiKhoans",
                columns: table => new
                {
                    id_tai_khoan = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ma_tai_khoan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ten_dang_nhap = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    mat_khau = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    trang_thai = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    chuc_vu = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaiKhoans", x => x.id_tai_khoan);
                });

            migrationBuilder.CreateTable(
                name: "TrangThaiHoaDons",
                columns: table => new
                {
                    id_trang_thai_hoa_don = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ten_trang_thai_hoa_don = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ma_trang_thai_hoa_don = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    mo_ta = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    trang_thai = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrangThaiHoaDons", x => x.id_trang_thai_hoa_don);
                });

            migrationBuilder.CreateTable(
                name: "KhachHangs",
                columns: table => new
                {
                    id_khach_hang = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    id_tai_khoan = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ma_khach_hang = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ten_khach_hang = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ngay_sinh = table.Column<DateTime>(type: "datetime2", nullable: false),
                    so_dien_thoai = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    gioi_tinh = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    trang_thai = table.Column<string>(type: "nvarchar(max)", nullable: false)
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
                    ten_nhan_vien = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    so_dien_thoai = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ngay_sinh = table.Column<DateTime>(type: "datetime2", nullable: false),
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
                    dia_chi_mac_dinh = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
                    trang_thai = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    id_nguoi_tao = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    id_nguoi_sua = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ngay_sua = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NhanVienid_nhan_vien = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NhanVienid_nhan_vien1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatLieus", x => x.id_chat_lieu);
                    table.ForeignKey(
                        name: "FK_ChatLieus_NhanViens_NhanVienid_nhan_vien",
                        column: x => x.NhanVienid_nhan_vien,
                        principalTable: "NhanViens",
                        principalColumn: "id_nhan_vien");
                    table.ForeignKey(
                        name: "FK_ChatLieus_NhanViens_NhanVienid_nhan_vien1",
                        column: x => x.NhanVienid_nhan_vien1,
                        principalTable: "NhanViens",
                        principalColumn: "id_nhan_vien");
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
                name: "GiamGias",
                columns: table => new
                {
                    id_giam_gia = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ten_giam_gia = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    mo_ta = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    loai_giam_gia = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    thoi_gian_bat_dau = table.Column<DateTime>(type: "datetime2", nullable: false),
                    thoi_gian_ket_thuc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    trang_thai = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime2", nullable: false),
                    id_nguoi_tao = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    id_nguoi_cap_nhat = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NhanVienid_nhan_vien = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NhanVienid_nhan_vien1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GiamGias", x => x.id_giam_gia);
                    table.ForeignKey(
                        name: "FK_GiamGias_NhanViens_NhanVienid_nhan_vien",
                        column: x => x.NhanVienid_nhan_vien,
                        principalTable: "NhanViens",
                        principalColumn: "id_nhan_vien");
                    table.ForeignKey(
                        name: "FK_GiamGias_NhanViens_NhanVienid_nhan_vien1",
                        column: x => x.NhanVienid_nhan_vien1,
                        principalTable: "NhanViens",
                        principalColumn: "id_nhan_vien");
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
                    kieu_giam_gia = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    gia_tri_giam = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    gia_tri_giam_toi_thieu = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    gia_tri_giam_toi_da = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    so_luong_toi_da = table.Column<int>(type: "int", nullable: false),
                    so_luong_da_su_dung = table.Column<int>(type: "int", nullable: false),
                    thoi_gian_bat_dau = table.Column<DateTime>(type: "datetime2", nullable: false),
                    thoi_gian_ket_thuc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    trang_thai = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    id_nguoi_tao = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ngay_sua = table.Column<DateTime>(type: "datetime2", nullable: false),
                    id_nguoi_sua = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NhanVienid_nhan_vien = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NhanVienid_nhan_vien1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KhuyenMais", x => x.id_khuyen_mai);
                    table.ForeignKey(
                        name: "FK_KhuyenMais_NhanViens_NhanVienid_nhan_vien",
                        column: x => x.NhanVienid_nhan_vien,
                        principalTable: "NhanViens",
                        principalColumn: "id_nhan_vien");
                    table.ForeignKey(
                        name: "FK_KhuyenMais_NhanViens_NhanVienid_nhan_vien1",
                        column: x => x.NhanVienid_nhan_vien1,
                        principalTable: "NhanViens",
                        principalColumn: "id_nhan_vien");
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
                    trang_thai = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    id_nguoi_tao = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    id_nguoi_sua = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ngay_sua = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NhanVienid_nhan_vien = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NhanVienid_nhan_vien1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KichCos", x => x.id_kich_co);
                    table.ForeignKey(
                        name: "FK_KichCos_NhanViens_NhanVienid_nhan_vien",
                        column: x => x.NhanVienid_nhan_vien,
                        principalTable: "NhanViens",
                        principalColumn: "id_nhan_vien");
                    table.ForeignKey(
                        name: "FK_KichCos_NhanViens_NhanVienid_nhan_vien1",
                        column: x => x.NhanVienid_nhan_vien1,
                        principalTable: "NhanViens",
                        principalColumn: "id_nhan_vien");
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
                    trang_thai = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    id_nguoi_tao = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    id_nguoi_sua = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ngay_sua = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NhanVienid_nhan_vien = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NhanVienid_nhan_vien1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KieuDangs", x => x.id_kieu_dang);
                    table.ForeignKey(
                        name: "FK_KieuDangs_NhanViens_NhanVienid_nhan_vien",
                        column: x => x.NhanVienid_nhan_vien,
                        principalTable: "NhanViens",
                        principalColumn: "id_nhan_vien");
                    table.ForeignKey(
                        name: "FK_KieuDangs_NhanViens_NhanVienid_nhan_vien1",
                        column: x => x.NhanVienid_nhan_vien1,
                        principalTable: "NhanViens",
                        principalColumn: "id_nhan_vien");
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
                    trang_thai = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    id_nguoi_tao = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    id_nguoi_sua = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ngay_sua = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NhanVienid_nhan_vien = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NhanVienid_nhan_vien1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MauSacs", x => x.id_mau_sac);
                    table.ForeignKey(
                        name: "FK_MauSacs_NhanViens_NhanVienid_nhan_vien",
                        column: x => x.NhanVienid_nhan_vien,
                        principalTable: "NhanViens",
                        principalColumn: "id_nhan_vien");
                    table.ForeignKey(
                        name: "FK_MauSacs_NhanViens_NhanVienid_nhan_vien1",
                        column: x => x.NhanVienid_nhan_vien1,
                        principalTable: "NhanViens",
                        principalColumn: "id_nhan_vien");
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
                name: "ThuongHieus",
                columns: table => new
                {
                    id_thuong_hieu = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ma_thuong_hieu = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ten_thuong_hieu = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    trang_thai = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    id_nguoi_tao = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    id_nguoi_sua = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ngay_sua = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NhanVienid_nhan_vien = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NhanVienid_nhan_vien1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThuongHieus", x => x.id_thuong_hieu);
                    table.ForeignKey(
                        name: "FK_ThuongHieus_NhanViens_NhanVienid_nhan_vien",
                        column: x => x.NhanVienid_nhan_vien,
                        principalTable: "NhanViens",
                        principalColumn: "id_nhan_vien");
                    table.ForeignKey(
                        name: "FK_ThuongHieus_NhanViens_NhanVienid_nhan_vien1",
                        column: x => x.NhanVienid_nhan_vien1,
                        principalTable: "NhanViens",
                        principalColumn: "id_nhan_vien");
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
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ngay_sua = table.Column<DateTime>(type: "datetime2", nullable: true),
                    id_nguoi_tao = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    id_nguoi_sua = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NhanVienid_nhan_vien = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NhanVienid_nhan_vien1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_XuatXus", x => x.id_xuat_xu);
                    table.ForeignKey(
                        name: "FK_XuatXus_NhanViens_NhanVienid_nhan_vien",
                        column: x => x.NhanVienid_nhan_vien,
                        principalTable: "NhanViens",
                        principalColumn: "id_nhan_vien");
                    table.ForeignKey(
                        name: "FK_XuatXus_NhanViens_NhanVienid_nhan_vien1",
                        column: x => x.NhanVienid_nhan_vien1,
                        principalTable: "NhanViens",
                        principalColumn: "id_nhan_vien");
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
                name: "HoaDons",
                columns: table => new
                {
                    id_hoa_don = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ma_hoa_don = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    tong_tien_don_hang = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    so_tien_khuyen_mai = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ghi_chu = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    tong_tien_phai_thanh_toan = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ten_khach_hang = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ten_nhan_vien = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    sdt_khach_hang = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    dia_chi_nhan_hang = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    id_khach_hang = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    id_nhan_vien = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    id_khuyen_mai = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    id_phuong_thuc_thanh_toan = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    id_trang_thai_hoa_don = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoaDons", x => x.id_hoa_don);
                    table.ForeignKey(
                        name: "FK_HoaDons_KhachHangs_id_khach_hang",
                        column: x => x.id_khach_hang,
                        principalTable: "KhachHangs",
                        principalColumn: "id_khach_hang",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HoaDons_KhuyenMais_id_khuyen_mai",
                        column: x => x.id_khuyen_mai,
                        principalTable: "KhuyenMais",
                        principalColumn: "id_khuyen_mai",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HoaDons_NhanViens_id_nhan_vien",
                        column: x => x.id_nhan_vien,
                        principalTable: "NhanViens",
                        principalColumn: "id_nhan_vien");
                    table.ForeignKey(
                        name: "FK_HoaDons_PhuongThucThanhToans_id_phuong_thuc_thanh_toan",
                        column: x => x.id_phuong_thuc_thanh_toan,
                        principalTable: "PhuongThucThanhToans",
                        principalColumn: "id_phuong_thuc_thanh_toan",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HoaDons_TrangThaiHoaDons_id_trang_thai_hoa_don",
                        column: x => x.id_trang_thai_hoa_don,
                        principalTable: "TrangThaiHoaDons",
                        principalColumn: "id_trang_thai_hoa_don",
                        onDelete: ReferentialAction.Cascade);
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
                    ngay_sua = table.Column<DateTime>(type: "datetime2", nullable: true),
                    id_chat_lieu = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    id_kieu_dang = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    id_thuong_hieu = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    id_xuat_xu = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NhanVienid_nhan_vien = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NhanVienid_nhan_vien1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
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
                        name: "FK_SanPhams_KieuDangs_id_kieu_dang",
                        column: x => x.id_kieu_dang,
                        principalTable: "KieuDangs",
                        principalColumn: "id_kieu_dang",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SanPhams_NhanViens_NhanVienid_nhan_vien",
                        column: x => x.NhanVienid_nhan_vien,
                        principalTable: "NhanViens",
                        principalColumn: "id_nhan_vien");
                    table.ForeignKey(
                        name: "FK_SanPhams_NhanViens_NhanVienid_nhan_vien1",
                        column: x => x.NhanVienid_nhan_vien1,
                        principalTable: "NhanViens",
                        principalColumn: "id_nhan_vien");
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
                name: "SanPhamChiTiets",
                columns: table => new
                {
                    id_san_pham_chi_tiet = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ma_san_pham_chi_tiet = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    so_luong = table.Column<int>(type: "int", nullable: false),
                    gia_ban = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    gia_nhap = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    so_tien_giam_gia_theo_chuong_trinh = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    trang_thai = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    id_nguoi_tao = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    id_nguoi_sua = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ngay_sua = table.Column<DateTime>(type: "datetime2", nullable: true),
                    id_san_pham = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    id_kich_co = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    id_mau_sac = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NhanVienid_nhan_vien = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NhanVienid_nhan_vien1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
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
                        name: "FK_SanPhamChiTiets_NhanViens_NhanVienid_nhan_vien",
                        column: x => x.NhanVienid_nhan_vien,
                        principalTable: "NhanViens",
                        principalColumn: "id_nhan_vien");
                    table.ForeignKey(
                        name: "FK_SanPhamChiTiets_NhanViens_NhanVienid_nhan_vien1",
                        column: x => x.NhanVienid_nhan_vien1,
                        principalTable: "NhanViens",
                        principalColumn: "id_nhan_vien");
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
                name: "GiamGiaSanPhamChiTiets",
                columns: table => new
                {
                    id_giam_gia_san_pham_chi_tiet = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    id_giam_gia = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    id_san_pham_chi_tiet = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime2", nullable: false),
                    id_nguoi_tao = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    id_nguoi_cap_nhat = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NhanVienid_nhan_vien = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NhanVienid_nhan_vien1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GiamGiaSanPhamChiTiets", x => x.id_giam_gia_san_pham_chi_tiet);
                    table.ForeignKey(
                        name: "FK_GiamGiaSanPhamChiTiets_GiamGias_id_giam_gia",
                        column: x => x.id_giam_gia,
                        principalTable: "GiamGias",
                        principalColumn: "id_giam_gia",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GiamGiaSanPhamChiTiets_NhanViens_NhanVienid_nhan_vien",
                        column: x => x.NhanVienid_nhan_vien,
                        principalTable: "NhanViens",
                        principalColumn: "id_nhan_vien");
                    table.ForeignKey(
                        name: "FK_GiamGiaSanPhamChiTiets_NhanViens_NhanVienid_nhan_vien1",
                        column: x => x.NhanVienid_nhan_vien1,
                        principalTable: "NhanViens",
                        principalColumn: "id_nhan_vien");
                    table.ForeignKey(
                        name: "FK_GiamGiaSanPhamChiTiets_NhanViens_id_nguoi_cap_nhat",
                        column: x => x.id_nguoi_cap_nhat,
                        principalTable: "NhanViens",
                        principalColumn: "id_nhan_vien",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GiamGiaSanPhamChiTiets_NhanViens_id_nguoi_tao",
                        column: x => x.id_nguoi_tao,
                        principalTable: "NhanViens",
                        principalColumn: "id_nhan_vien",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GiamGiaSanPhamChiTiets_SanPhamChiTiets_id_san_pham_chi_tiet",
                        column: x => x.id_san_pham_chi_tiet,
                        principalTable: "SanPhamChiTiets",
                        principalColumn: "id_san_pham_chi_tiet",
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
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GioHangChiTiets_SanPhamChiTiets_id_san_pham_chi_tiet",
                        column: x => x.id_san_pham_chi_tiet,
                        principalTable: "SanPhamChiTiets",
                        principalColumn: "id_san_pham_chi_tiet",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HinhAnhs",
                columns: table => new
                {
                    id_hinh_anh = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ma_hinh_anh = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ten_hinh_anh = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    trang_thai = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    id_san_pham_chi_tiet = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    id_nguoi_tao = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    id_nguoi_sua = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ngay_sua = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NhanVienid_nhan_vien = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NhanVienid_nhan_vien1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HinhAnhs", x => x.id_hinh_anh);
                    table.ForeignKey(
                        name: "FK_HinhAnhs_NhanViens_NhanVienid_nhan_vien",
                        column: x => x.NhanVienid_nhan_vien,
                        principalTable: "NhanViens",
                        principalColumn: "id_nhan_vien");
                    table.ForeignKey(
                        name: "FK_HinhAnhs_NhanViens_NhanVienid_nhan_vien1",
                        column: x => x.NhanVienid_nhan_vien1,
                        principalTable: "NhanViens",
                        principalColumn: "id_nhan_vien");
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
                    table.ForeignKey(
                        name: "FK_HinhAnhs_SanPhamChiTiets_id_san_pham_chi_tiet",
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
                    so_luong = table.Column<int>(type: "int", nullable: false),
                    don_gia = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    thanh_tien = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ghi_chu = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    trang_thai = table.Column<bool>(type: "bit", nullable: false)
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
                        name: "FK_HoaDonChiTiets_SanPhamChiTiets_id_san_pham_chi_tiet",
                        column: x => x.id_san_pham_chi_tiet,
                        principalTable: "SanPhamChiTiets",
                        principalColumn: "id_san_pham_chi_tiet",
                        onDelete: ReferentialAction.Cascade);
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
                name: "IX_ChatLieus_NhanVienid_nhan_vien",
                table: "ChatLieus",
                column: "NhanVienid_nhan_vien");

            migrationBuilder.CreateIndex(
                name: "IX_ChatLieus_NhanVienid_nhan_vien1",
                table: "ChatLieus",
                column: "NhanVienid_nhan_vien1");

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
                name: "IX_GiamGias_NhanVienid_nhan_vien",
                table: "GiamGias",
                column: "NhanVienid_nhan_vien");

            migrationBuilder.CreateIndex(
                name: "IX_GiamGias_NhanVienid_nhan_vien1",
                table: "GiamGias",
                column: "NhanVienid_nhan_vien1");

            migrationBuilder.CreateIndex(
                name: "IX_GiamGiaSanPhamChiTiets_id_giam_gia",
                table: "GiamGiaSanPhamChiTiets",
                column: "id_giam_gia");

            migrationBuilder.CreateIndex(
                name: "IX_GiamGiaSanPhamChiTiets_id_nguoi_cap_nhat",
                table: "GiamGiaSanPhamChiTiets",
                column: "id_nguoi_cap_nhat");

            migrationBuilder.CreateIndex(
                name: "IX_GiamGiaSanPhamChiTiets_id_nguoi_tao",
                table: "GiamGiaSanPhamChiTiets",
                column: "id_nguoi_tao");

            migrationBuilder.CreateIndex(
                name: "IX_GiamGiaSanPhamChiTiets_id_san_pham_chi_tiet",
                table: "GiamGiaSanPhamChiTiets",
                column: "id_san_pham_chi_tiet",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GiamGiaSanPhamChiTiets_NhanVienid_nhan_vien",
                table: "GiamGiaSanPhamChiTiets",
                column: "NhanVienid_nhan_vien");

            migrationBuilder.CreateIndex(
                name: "IX_GiamGiaSanPhamChiTiets_NhanVienid_nhan_vien1",
                table: "GiamGiaSanPhamChiTiets",
                column: "NhanVienid_nhan_vien1");

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
                name: "IX_HinhAnhs_id_san_pham_chi_tiet",
                table: "HinhAnhs",
                column: "id_san_pham_chi_tiet");

            migrationBuilder.CreateIndex(
                name: "IX_HinhAnhs_NhanVienid_nhan_vien",
                table: "HinhAnhs",
                column: "NhanVienid_nhan_vien");

            migrationBuilder.CreateIndex(
                name: "IX_HinhAnhs_NhanVienid_nhan_vien1",
                table: "HinhAnhs",
                column: "NhanVienid_nhan_vien1");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDonChiTiets_id_hoa_don",
                table: "HoaDonChiTiets",
                column: "id_hoa_don");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDonChiTiets_id_san_pham_chi_tiet",
                table: "HoaDonChiTiets",
                column: "id_san_pham_chi_tiet");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDons_id_khach_hang",
                table: "HoaDons",
                column: "id_khach_hang");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDons_id_khuyen_mai",
                table: "HoaDons",
                column: "id_khuyen_mai");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDons_id_nhan_vien",
                table: "HoaDons",
                column: "id_nhan_vien");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDons_id_phuong_thuc_thanh_toan",
                table: "HoaDons",
                column: "id_phuong_thuc_thanh_toan");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDons_id_trang_thai_hoa_don",
                table: "HoaDons",
                column: "id_trang_thai_hoa_don");

            migrationBuilder.CreateIndex(
                name: "IX_KhachHangs_id_tai_khoan",
                table: "KhachHangs",
                column: "id_tai_khoan",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KhuyenMais_id_nguoi_sua",
                table: "KhuyenMais",
                column: "id_nguoi_sua");

            migrationBuilder.CreateIndex(
                name: "IX_KhuyenMais_id_nguoi_tao",
                table: "KhuyenMais",
                column: "id_nguoi_tao");

            migrationBuilder.CreateIndex(
                name: "IX_KhuyenMais_NhanVienid_nhan_vien",
                table: "KhuyenMais",
                column: "NhanVienid_nhan_vien");

            migrationBuilder.CreateIndex(
                name: "IX_KhuyenMais_NhanVienid_nhan_vien1",
                table: "KhuyenMais",
                column: "NhanVienid_nhan_vien1");

            migrationBuilder.CreateIndex(
                name: "IX_KichCos_id_nguoi_sua",
                table: "KichCos",
                column: "id_nguoi_sua");

            migrationBuilder.CreateIndex(
                name: "IX_KichCos_id_nguoi_tao",
                table: "KichCos",
                column: "id_nguoi_tao");

            migrationBuilder.CreateIndex(
                name: "IX_KichCos_NhanVienid_nhan_vien",
                table: "KichCos",
                column: "NhanVienid_nhan_vien");

            migrationBuilder.CreateIndex(
                name: "IX_KichCos_NhanVienid_nhan_vien1",
                table: "KichCos",
                column: "NhanVienid_nhan_vien1");

            migrationBuilder.CreateIndex(
                name: "IX_KieuDangs_id_nguoi_sua",
                table: "KieuDangs",
                column: "id_nguoi_sua");

            migrationBuilder.CreateIndex(
                name: "IX_KieuDangs_id_nguoi_tao",
                table: "KieuDangs",
                column: "id_nguoi_tao");

            migrationBuilder.CreateIndex(
                name: "IX_KieuDangs_NhanVienid_nhan_vien",
                table: "KieuDangs",
                column: "NhanVienid_nhan_vien");

            migrationBuilder.CreateIndex(
                name: "IX_KieuDangs_NhanVienid_nhan_vien1",
                table: "KieuDangs",
                column: "NhanVienid_nhan_vien1");

            migrationBuilder.CreateIndex(
                name: "IX_MauSacs_id_nguoi_sua",
                table: "MauSacs",
                column: "id_nguoi_sua");

            migrationBuilder.CreateIndex(
                name: "IX_MauSacs_id_nguoi_tao",
                table: "MauSacs",
                column: "id_nguoi_tao");

            migrationBuilder.CreateIndex(
                name: "IX_MauSacs_NhanVienid_nhan_vien",
                table: "MauSacs",
                column: "NhanVienid_nhan_vien");

            migrationBuilder.CreateIndex(
                name: "IX_MauSacs_NhanVienid_nhan_vien1",
                table: "MauSacs",
                column: "NhanVienid_nhan_vien1");

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
                name: "IX_SanPhamChiTiets_NhanVienid_nhan_vien",
                table: "SanPhamChiTiets",
                column: "NhanVienid_nhan_vien");

            migrationBuilder.CreateIndex(
                name: "IX_SanPhamChiTiets_NhanVienid_nhan_vien1",
                table: "SanPhamChiTiets",
                column: "NhanVienid_nhan_vien1");

            migrationBuilder.CreateIndex(
                name: "IX_SanPhams_id_chat_lieu",
                table: "SanPhams",
                column: "id_chat_lieu");

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
                name: "IX_SanPhams_NhanVienid_nhan_vien",
                table: "SanPhams",
                column: "NhanVienid_nhan_vien");

            migrationBuilder.CreateIndex(
                name: "IX_SanPhams_NhanVienid_nhan_vien1",
                table: "SanPhams",
                column: "NhanVienid_nhan_vien1");

            migrationBuilder.CreateIndex(
                name: "IX_ThuongHieus_id_nguoi_sua",
                table: "ThuongHieus",
                column: "id_nguoi_sua");

            migrationBuilder.CreateIndex(
                name: "IX_ThuongHieus_id_nguoi_tao",
                table: "ThuongHieus",
                column: "id_nguoi_tao");

            migrationBuilder.CreateIndex(
                name: "IX_ThuongHieus_NhanVienid_nhan_vien",
                table: "ThuongHieus",
                column: "NhanVienid_nhan_vien");

            migrationBuilder.CreateIndex(
                name: "IX_ThuongHieus_NhanVienid_nhan_vien1",
                table: "ThuongHieus",
                column: "NhanVienid_nhan_vien1");

            migrationBuilder.CreateIndex(
                name: "IX_XuatXus_id_nguoi_sua",
                table: "XuatXus",
                column: "id_nguoi_sua");

            migrationBuilder.CreateIndex(
                name: "IX_XuatXus_id_nguoi_tao",
                table: "XuatXus",
                column: "id_nguoi_tao");

            migrationBuilder.CreateIndex(
                name: "IX_XuatXus_NhanVienid_nhan_vien",
                table: "XuatXus",
                column: "NhanVienid_nhan_vien");

            migrationBuilder.CreateIndex(
                name: "IX_XuatXus_NhanVienid_nhan_vien1",
                table: "XuatXus",
                column: "NhanVienid_nhan_vien1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DiaChis");

            migrationBuilder.DropTable(
                name: "GiamGiaSanPhamChiTiets");

            migrationBuilder.DropTable(
                name: "GioHangChiTiets");

            migrationBuilder.DropTable(
                name: "HinhAnhs");

            migrationBuilder.DropTable(
                name: "HoaDonChiTiets");

            migrationBuilder.DropTable(
                name: "GiamGias");

            migrationBuilder.DropTable(
                name: "HoaDons");

            migrationBuilder.DropTable(
                name: "SanPhamChiTiets");

            migrationBuilder.DropTable(
                name: "KhachHangs");

            migrationBuilder.DropTable(
                name: "KhuyenMais");

            migrationBuilder.DropTable(
                name: "PhuongThucThanhToans");

            migrationBuilder.DropTable(
                name: "TrangThaiHoaDons");

            migrationBuilder.DropTable(
                name: "KichCos");

            migrationBuilder.DropTable(
                name: "MauSacs");

            migrationBuilder.DropTable(
                name: "SanPhams");

            migrationBuilder.DropTable(
                name: "ChatLieus");

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
