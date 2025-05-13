using System.Runtime.CompilerServices;
using API.DbConects.DTOs.Admin.SanPham;
using API.DbConects.Entities.Entities_San_Pham;
using API.Services.Interfaces;
using API.Services.JwtServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;

namespace API.Controllers.SanPham_Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class SanPhamChiTietController : ControllerBase
    {
        private readonly IBaseService<SanPhamChiTiet> _sanPhamChiTietServices;
        private readonly IBaseService<SanPham> _sanPhamServices;
        private readonly IBaseService<MauSac> _mauSacServices;
        private readonly IBaseService<KichCo> _kichCoServices;
        private readonly IBaseService<HinhAnh> _hinhAnhServices;
        private readonly IJwtServices _jwtServices;

        public SanPhamChiTietController(IBaseService<SanPhamChiTiet> sanPhamChiTietServices, IBaseService<SanPham> sanPhamServices, IBaseService<MauSac> mauSacServices, IBaseService<KichCo> kichCoServices, IBaseService<HinhAnh> hinhAnhServices, IJwtServices jwtServices)
        {
            _sanPhamChiTietServices = sanPhamChiTietServices;
            _sanPhamServices = sanPhamServices;
            _mauSacServices = mauSacServices;
            _kichCoServices = kichCoServices;
            _hinhAnhServices = hinhAnhServices;
            _jwtServices = jwtServices;
        }
        [HttpGet("lay-danh-sach-san-pham-chi-tiet")]
        [Authorize(Roles = "Admin,NhanVien")]

        public async Task<IActionResult> GetSanPhamChiTietAsync()
        {
            var result = await _sanPhamChiTietServices.GetAllWithIncludeAsync(
                q => q.Include(sp => sp.SanPham)
                    .Include(sp => sp.KichCo)
                    .Include(sp => sp.MauSac)
                    .Include(sp => sp.HinhAnhSanPhamChiTiets)
                    .ThenInclude(ha => ha.HinhAnhs)
            );
            return Ok(result);
        }

        [HttpGet("lay-san-pham-chi-tiet-theo-id/{id}")]
        [Authorize(Roles = "Admin,NhanVien")]

        public async Task<IActionResult> GetSanPhamChiTietByIdAsync(Guid id)
        {
            var result = await _sanPhamChiTietServices.GetByIdWithIncludeAsync(id,
                q => q.Include(sp => sp.SanPham)
                    .Include(sp => sp.KichCo)
                    .Include(sp => sp.MauSac)
                    .Include(sp => sp.HinhAnhSanPhamChiTiets)
                    .ThenInclude(ha => ha.HinhAnhs)
            );
            return Ok(result);
        }

        [HttpPost("them-san-pham-chi-tiet")]
        [Authorize(Roles = "Admin,NhanVien")]

        public async Task<IActionResult> AddSanPhamChiTietAsync(Guid id, [FromBody] ThemSanPhamChiTietAdminDTO sanPhamChiTietDTO)
        {
            if (sanPhamChiTietDTO == null)
                return BadRequest("Dữ liệu không hợp lệ");

            if (sanPhamChiTietDTO.id_san_pham == Guid.Empty)
                return BadRequest("ID sản phẩm không được để trống");

            if (sanPhamChiTietDTO.id_mau_sac == Guid.Empty)
                return BadRequest("ID màu sắc không được để trống");

            if (sanPhamChiTietDTO.id_kich_co == Guid.Empty)
                return BadRequest("ID kích cỡ không được để trống");

            if (sanPhamChiTietDTO.so_luong <= 0)
                return BadRequest("Số lượng phải lớn hơn 0");

            if (sanPhamChiTietDTO.gia_nhap <= 0)
                return BadRequest("Giá nhập phải lớn hơn 0");

            if (sanPhamChiTietDTO.gia_ban <= 0)
                return BadRequest("Giá bán phải lớn hơn 0");

            if (sanPhamChiTietDTO.gia_ban <= sanPhamChiTietDTO.gia_nhap)
                return BadRequest("Giá bán phải lớn hơn giá nhập");

            if (sanPhamChiTietDTO.them_hinh_anh_spcts.Count == 0)
                return BadRequest("Vui lòng thêm hình ảnh sản phẩm chi tiết");

            var result = await _sanPhamChiTietServices.ExecuteInTransactionAsync(async () =>
            {
                var sanPhamChiTiet = new SanPhamChiTiet
                {
                    id_san_pham_chi_tiet = Guid.NewGuid(),
                    ma_san_pham_chi_tiet = (await _sanPhamServices.GetByIdAsync(id)).ma_san_pham + "-" +
                                        (await _mauSacServices.GetByIdAsync(sanPhamChiTietDTO.id_mau_sac)).ten_mau_sac + "-" +
                                        (await _kichCoServices.GetByIdAsync(sanPhamChiTietDTO.id_kich_co)).ten_kich_co,
                    id_san_pham = id,
                    id_mau_sac = sanPhamChiTietDTO.id_mau_sac,
                    id_kich_co = sanPhamChiTietDTO.id_kich_co,
                    so_luong = sanPhamChiTietDTO.so_luong,
                    gia_nhap = sanPhamChiTietDTO.gia_nhap,
                    gia_ban = sanPhamChiTietDTO.gia_ban,
                    trang_thai = "Còn hàng",
                    ngay_tao = DateTime.Now,
                    id_nguoi_tao = Guid.Parse(GetMaNhanVien())
                };

                var createResult = await _sanPhamChiTietServices.CreateAsync(sanPhamChiTiet);
                if (!createResult) return false;

                foreach (var hinhAnh in sanPhamChiTietDTO.them_hinh_anh_spcts)
                {
                    var hinhAnhSanPhamChiTiet = new HinhAnh
                    {
                        id_hinh_anh = Guid.NewGuid(),
                        url = hinhAnh.hinh_anh_urls,
                        ngay_tao = DateTime.Now,
                        id_nguoi_tao = Guid.Parse(GetMaNhanVien())
                    };
                    var imageResult = await _hinhAnhServices.CreateAsync(hinhAnhSanPhamChiTiet);
                    if (!imageResult) return false;
                }

                return true;
            });

            if (result) return Ok("Thêm sản phẩm chi tiết thành công");
            return BadRequest("Đã có lỗi khi thêm sản phẩm chi tiết!");
        }
        [HttpPut("sua-san-pham-chi-tiet/{id}")]
        [Authorize(Roles = "NhanVien")]
        public async Task<IActionResult> UpdateSanPhamChiTietAsync(Guid id, [FromBody] SuaSanPhamChiTietAdminDTO sanPhamChiTietDTO)
        {
            if (sanPhamChiTietDTO == null)
                return BadRequest("Dữ liệu không hợp lệ");

            if (sanPhamChiTietDTO.so_luong <= 0)
                return BadRequest("Số lượng phải lớn hơn 0");

            if (sanPhamChiTietDTO.gia_nhap <= 0)
                return BadRequest("Giá nhập phải lớn hơn 0");

            if (sanPhamChiTietDTO.gia_ban <= 0)
                return BadRequest("Giá bán phải lớn hơn 0");

            if (sanPhamChiTietDTO.gia_ban <= sanPhamChiTietDTO.gia_nhap)
                return BadRequest("Giá bán phải lớn hơn giá nhập");

            var result = await _sanPhamChiTietServices.ExecuteInTransactionAsync(async () =>
            {
                var existingSanPhamChiTiet = await _sanPhamChiTietServices.GetByIdAsync(id);
                if (existingSanPhamChiTiet == null) return false;

                existingSanPhamChiTiet.so_luong = sanPhamChiTietDTO.so_luong;
                existingSanPhamChiTiet.gia_nhap = sanPhamChiTietDTO.gia_nhap;
                existingSanPhamChiTiet.gia_ban = sanPhamChiTietDTO.gia_ban;
                existingSanPhamChiTiet.trang_thai = sanPhamChiTietDTO.trang_thai;

                var updateResult = await _sanPhamChiTietServices.UpdateAsync(existingSanPhamChiTiet);
                if (!updateResult) return false;



                // Thêm hình ảnh mới nếu có
                if (sanPhamChiTietDTO.them_hinh_anh_spcts != null && sanPhamChiTietDTO.them_hinh_anh_spcts.Any())
                {
                    foreach (var hinhAnh in sanPhamChiTietDTO.them_hinh_anh_spcts)
                    {
                        var hinhAnhSanPhamChiTiet = new HinhAnh
                        {
                            id_hinh_anh = Guid.NewGuid(),
                            url = hinhAnh.hinh_anh_urls,
                            ngay_tao = DateTime.Now,
                            id_nguoi_tao = Guid.Parse(GetMaNhanVien())
                        };
                        var imageResult = await _hinhAnhServices.CreateAsync(hinhAnhSanPhamChiTiet);
                        if (!imageResult) return false;
                    }
                }

                return true;
            });

            if (result) return Ok("Cập nhật sản phẩm chi tiết thành công");
            return BadRequest("Đã có lỗi khi cập nhật sản phẩm chi tiết!");
        }
        [HttpDelete("xoa-san-pham-chi-tiet/{id}")]
        [Authorize(Roles = "Admin,NhanVien")]

        public async Task<IActionResult> DeleteSanPhamChiTietAsync(Guid id)
        {
            var result = await _sanPhamChiTietServices.DeleteAsync(id);
            if (result) return Ok("Xóa sản phẩm chi tiết thành công");
            return BadRequest("Đã có lỗi khi xóa sản phẩm chi tiết!");
        }
        private string GetMaNhanVien()
        {
            var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            var maNhanVien = _jwtServices.GetMaTaiKhoanFromToken(token);
            return maNhanVien;
        }
    }
}
