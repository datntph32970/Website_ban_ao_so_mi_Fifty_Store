using API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Controllers
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T Data { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }

    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class ThongKeController : ControllerBase
    {
        private readonly IThongKeService _thongKeService;
        private readonly ILogger<ThongKeController> _logger;

        public ThongKeController(IThongKeService thongKeService, ILogger<ThongKeController> logger)
        {
            _thongKeService = thongKeService;
            _logger = logger;
        }

        private async Task<IActionResult> HandleApiOperationAsync<T>(
            Func<Task<T>> operation,
            string successMessage)
        {
            try
            {
                var result = await operation();
                return Ok(new ApiResponse<T>
                {
                    Success = true,
                    Data = result,
                    Message = successMessage
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thực hiện thao tác: {Message}", ex.Message);
                return StatusCode(500, new ApiResponse<T>
                {
                    Success = false,
                    Message = "Đã xảy ra lỗi khi xử lý yêu cầu"
                });
            }
        }

        private bool ValidateThangNam(int thang, int nam, out IActionResult errorResult)
        {
            if (thang < 1 || thang > 12)
            {
                errorResult = BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Tháng phải từ 1 đến 12"
                });
                return false;
            }

            if (nam < 2000 || nam > DateTime.Now.Year)
            {
                errorResult = BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Năm phải từ 2000 đến {DateTime.Now.Year}"
                });
                return false;
            }

            errorResult = null;
            return true;
        }

        private bool ValidateTuanNam(int tuan, int nam, out IActionResult errorResult)
        {
            if (tuan < 1 || tuan > 53)
            {
                errorResult = BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Tuần phải từ 1 đến 53"
                });
                return false;
            }

            if (nam < 2000 || nam > DateTime.Now.Year)
            {
                errorResult = BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Năm phải từ 2000 đến {DateTime.Now.Year}"
                });
                return false;
            }

            errorResult = null;
            return true;
        }

        #region Thống kê doanh thu

        /// <summary>
        /// Tính tổng doanh thu theo tháng
        /// </summary>
        /// <param name="thang">Tháng cần thống kê (1-12)</param>
        /// <param name="nam">Năm cần thống kê (2000 đến hiện tại)</param>
        /// <returns>Thông tin doanh thu của tháng</returns>
        /// <response code="200">Trả về thông tin doanh thu</response>
        /// <response code="400">Nếu tháng hoặc năm không hợp lệ</response>
        /// <response code="500">Nếu có lỗi xảy ra</response>
        [HttpGet("doanh-thu/theo-thang")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Admin,NhanVien")]

        public async Task<IActionResult> TinhDoanhThuTheoThang([FromQuery] int thang, [FromQuery] int nam)
        {
            if (!ValidateThangNam(thang, nam, out var errorResult))
                return errorResult;

            return await HandleApiOperationAsync<object>(
                async () =>
                {
                    var doanhThu = await _thongKeService.TinhTongDoanhThuTheoThang(thang, nam);
                    return new { thang, nam, doanh_thu = doanhThu };
                },
                $"Lấy doanh thu tháng {thang}/{nam} thành công"
            );
        }

        /// <summary>
        /// Tính tổng doanh thu theo năm
        /// </summary>
        /// <param name="nam">Năm cần thống kê (2000 đến hiện tại)</param>
        /// <returns>Thông tin doanh thu của năm</returns>
        [HttpGet("doanh-thu/theo-nam")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Admin,NhanVien")]

        public async Task<IActionResult> TinhDoanhThuTheoNam([FromQuery] int nam)
        {
            if (nam < 2000 || nam > DateTime.Now.Year)
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Năm phải từ 2000 đến {DateTime.Now.Year}"
                });

            return await HandleApiOperationAsync<object>(
                async () =>
                {
                    var doanhThu = await _thongKeService.TinhTongDoanhThuTheoNam(nam);
                    return new { nam, doanh_thu = doanhThu };
                },
                $"Lấy doanh thu năm {nam} thành công"
            );
        }

        /// <summary>
        /// Tính tổng doanh thu theo ngày
        /// </summary>
        /// <param name="ngay">Ngày cần thống kê</param>
        /// <returns>Thông tin doanh thu của ngày</returns>
        [HttpGet("doanh-thu/theo-ngay")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Admin,NhanVien")]

        public async Task<IActionResult> TinhDoanhThuTheoNgay([FromQuery] DateOnly ngay)
        {
            return await HandleApiOperationAsync<object>(
                async () =>
                {
                    var doanhThu = await _thongKeService.TinhTongDoanhThuTheoNgay(ngay);
                    return new { ngay = ngay.ToString("yyyy-MM-dd"), doanh_thu = doanhThu };
                },
                $"Lấy doanh thu ngày {ngay:dd/MM/yyyy} thành công"
            );
        }

        /// <summary>
        /// Tính tổng doanh thu theo tuần
        /// </summary>
        /// <param name="tuan">Tuần cần thống kê (1-53)</param>
        /// <param name="nam">Năm cần thống kê (2000 đến hiện tại)</param>
        /// <returns>Thông tin doanh thu của tuần</returns>
        [HttpGet("doanh-thu/theo-tuan")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Admin,NhanVien")]

        public async Task<IActionResult> TinhDoanhThuTheoTuan([FromQuery] int tuan, [FromQuery] int nam)
        {
            if (tuan < 1 || tuan > 53)
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Tuần phải từ 1 đến 53"
                });

            if (nam < 2000 || nam > DateTime.Now.Year)
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Năm phải từ 2000 đến {DateTime.Now.Year}"
                });

            try
            {
                var startDate = _thongKeService.GetStartDateOfWeek(tuan, nam);
                if (startDate.Year != nam)
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Tuần {tuan} không tồn tại trong năm {nam}"
                    });

                return await HandleApiOperationAsync<object>(
                    async () =>
                    {
                        var doanhThu = await _thongKeService.TinhTongDoanhThuTheoTuan(tuan, nam);
                        var endDate = startDate.AddDays(7);
                        return new
                        {
                            tuan,
                            nam,
                            tu_ngay = startDate.ToString("dd/MM/yyyy"),
                            den_ngay = endDate.ToString("dd/MM/yyyy"),
                            doanh_thu = doanhThu
                        };
                    },
                    $"Lấy doanh thu tuần {tuan} năm {nam} thành công"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tính doanh thu tuần {Tuan} năm {Nam}", tuan, nam);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Đã xảy ra lỗi khi tính doanh thu"
                });
            }
        }

        #endregion

        /// <summary>
        /// Lấy danh sách sản phẩm bán chạy nhất theo tháng
        /// </summary>
        /// <param name="thang">Tháng cần thống kê (1-12)</param>
        /// <param name="nam">Năm cần thống kê</param>
        /// <returns>Danh sách top 10 sản phẩm bán chạy nhất</returns>
        /// <response code="200">Trả về danh sách sản phẩm</response>
        /// <response code="400">Nếu tháng không hợp lệ</response>
        /// <response code="500">Nếu có lỗi xảy ra</response>
        [HttpGet("san-pham-ban-chay/theo-thang")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> LaySanPhamBanChayTheoThang([FromQuery] int thang, [FromQuery] int nam)
        {
            if (thang < 1 || thang > 12)
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Tháng phải từ 1 đến 12"
                });

            return await HandleApiOperationAsync<object>(
                async () =>
                {
                    var sanPhams = await _thongKeService.LaySanPhamBanChayNhatTheoThang(thang, nam);
                    if (!sanPhams.Any())
                        return new { thang, nam, san_pham_ban_chay = new List<object>(), tong_san_pham = 0 };

                    var sanPhamIds = sanPhams.Select(sp => sp.id_san_pham).ToList();
                    var soLuongBanDict = new Dictionary<Guid, int>();

                    foreach (var id in sanPhamIds)
                    {
                        var soLuong = await _thongKeService.TinhSoLuongSanPhamDaBan(id);
                        soLuongBanDict[id] = soLuong;
                    }

                    var result = sanPhams.Select(sp => new
                    {
                        id_san_pham = sp.id_san_pham,
                        ma_san_pham = sp.ma_san_pham,
                        ten_san_pham = sp.ten_san_pham,
                        mo_ta = sp.mo_ta,
                        so_luong_ban = soLuongBanDict.GetValueOrDefault(sp.id_san_pham)
                    })
                    .OrderByDescending(x => x.so_luong_ban)
                    .Take(10)
                    .ToList();

                    return new
                    {
                        thang,
                        nam,
                        san_pham_ban_chay = result,
                        tong_san_pham = result.Count,
                        message = $"Lấy danh sách {result.Count} sản phẩm bán chạy nhất tháng {thang}/{nam}"
                    };
                },
                $"Lấy danh sách sản phẩm bán chạy tháng {thang}/{nam} thành công"
            );
        }

        [HttpGet("san-pham-ban-chay/theo-nam")]
        [Authorize(Roles = "Admin,NhanVien")]

        public async Task<IActionResult> LaySanPhamBanChayTheoNam([FromQuery] int nam)
        {
            return await HandleApiOperationAsync<object>(
                async () =>
                {
                    var sanPhams = await _thongKeService.LaySanPhamBanChayNhatTheoNam(nam);
                    if (!sanPhams.Any())
                        return new { nam, san_pham_ban_chay = new List<object>(), tong_san_pham = 0 };

                    var sanPhamIds = sanPhams.Select(sp => sp.id_san_pham).ToList();
                    var soLuongBanDict = new Dictionary<Guid, int>();

                    foreach (var id in sanPhamIds)
                    {
                        var soLuong = await _thongKeService.TinhSoLuongSanPhamDaBan(id);
                        soLuongBanDict[id] = soLuong;
                    }

                    var result = sanPhams.Select(sp => new
                    {
                        id_san_pham = sp.id_san_pham,
                        ma_san_pham = sp.ma_san_pham,
                        ten_san_pham = sp.ten_san_pham,
                        mo_ta = sp.mo_ta,
                        so_luong_ban = soLuongBanDict.GetValueOrDefault(sp.id_san_pham)
                    })
                    .OrderByDescending(x => x.so_luong_ban)
                    .Take(10)
                    .ToList();

                    return new
                    {
                        nam,
                        san_pham_ban_chay = result,
                        tong_san_pham = result.Count,
                        message = $"Lấy danh sách {result.Count} sản phẩm bán chạy nhất năm {nam}"
                    };
                },
                $"Lấy danh sách sản phẩm bán chạy năm {nam} thành công"
            );
        }

        [HttpGet("san-pham-ban-chay/theo-tuan")]
        [Authorize(Roles = "Admin,NhanVien")]

        public async Task<IActionResult> LaySanPhamBanChayTheoTuan([FromQuery] int tuan, [FromQuery] int nam)
        {
            if (tuan < 1 || tuan > 53)
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Tuần phải từ 1 đến 53"
                });

            return await HandleApiOperationAsync<object>(
                async () =>
                {
                    var sanPhams = await _thongKeService.LaySanPhamBanChayNhatTheoTuan(tuan, nam);
                    if (!sanPhams.Any())
                        return new { tuan, nam, san_pham_ban_chay = new List<object>(), tong_san_pham = 0 };

                    var sanPhamIds = sanPhams.Select(sp => sp.id_san_pham).ToList();
                    var soLuongBanDict = new Dictionary<Guid, int>();

                    foreach (var id in sanPhamIds)
                    {
                        var soLuong = await _thongKeService.TinhSoLuongSanPhamDaBan(id);
                        soLuongBanDict[id] = soLuong;
                    }

                    var result = sanPhams.Select(sp => new
                    {
                        id_san_pham = sp.id_san_pham,
                        ma_san_pham = sp.ma_san_pham,
                        ten_san_pham = sp.ten_san_pham,
                        mo_ta = sp.mo_ta,
                        so_luong_ban = soLuongBanDict.GetValueOrDefault(sp.id_san_pham)
                    })
                    .OrderByDescending(x => x.so_luong_ban)
                    .Take(10)
                    .ToList();

                    return new
                    {
                        tuan,
                        nam,
                        san_pham_ban_chay = result,
                        tong_san_pham = result.Count,
                        message = $"Lấy danh sách {result.Count} sản phẩm bán chạy nhất tuần {tuan} năm {nam}"
                    };
                },
                $"Lấy danh sách sản phẩm bán chạy tuần {tuan} năm {nam} thành công"
            );
        }

        [HttpGet("san-pham-ban-chay/theo-ngay")]
        [Authorize(Roles = "Admin,NhanVien")]

        public async Task<IActionResult> LaySanPhamBanChayTheoNgay([FromQuery] DateOnly ngay)
        {
            return await HandleApiOperationAsync<object>(
                async () =>
                {
                    var sanPhams = await _thongKeService.LaySanPhamBanChayNhatTheoNgay(ngay);
                    if (!sanPhams.Any())
                        return new { ngay = ngay.ToString("yyyy-MM-dd"), san_pham_ban_chay = new List<object>(), tong_san_pham = 0 };

                    var sanPhamIds = sanPhams.Select(sp => sp.id_san_pham).ToList();
                    var soLuongBanDict = new Dictionary<Guid, int>();

                    foreach (var id in sanPhamIds)
                    {
                        var soLuong = await _thongKeService.TinhSoLuongSanPhamDaBan(id);
                        soLuongBanDict[id] = soLuong;
                    }

                    var result = sanPhams.Select(sp => new
                    {
                        id_san_pham = sp.id_san_pham,
                        ma_san_pham = sp.ma_san_pham,
                        ten_san_pham = sp.ten_san_pham,
                        mo_ta = sp.mo_ta,
                        so_luong_ban = soLuongBanDict.GetValueOrDefault(sp.id_san_pham)
                    })
                    .OrderByDescending(x => x.so_luong_ban)
                    .Take(10)
                    .ToList();

                    return new
                    {
                        ngay = ngay.ToString("yyyy-MM-dd"),
                        san_pham_ban_chay = result,
                        tong_san_pham = result.Count,
                        message = $"Lấy danh sách {result.Count} sản phẩm bán chạy nhất ngày {ngay:dd/MM/yyyy}"
                    };
                },
                $"Lấy danh sách sản phẩm bán chạy ngày {ngay:dd/MM/yyyy} thành công"
            );
        }

        #region Thống kê đơn hàng

        /// <summary>
        /// Tính tổng số đơn hàng theo tháng
        /// </summary>
        /// <param name="thang">Tháng cần thống kê (1-12)</param>
        /// <param name="nam">Năm cần thống kê (2000 đến hiện tại)</param>
        /// <returns>Thông tin số lượng đơn hàng của tháng</returns>
        [HttpGet("don-hang/theo-thang")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Admin,NhanVien")]

        public async Task<IActionResult> TinhDonHangTheoThang([FromQuery] int thang, [FromQuery] int nam)
        {
            if (!ValidateThangNam(thang, nam, out var errorResult))
                return errorResult;

            return await HandleApiOperationAsync<object>(
                async () =>
                {
                    var soDonHang = await _thongKeService.TinhTongDonHangTheoThang(thang, nam);
                    return new { thang, nam, so_don_hang = soDonHang };
                },
                $"Lấy số đơn hàng tháng {thang}/{nam} thành công"
            );
        }

        /// <summary>
        /// Tính tổng số đơn hàng theo năm
        /// </summary>
        /// <param name="nam">Năm cần thống kê (2000 đến hiện tại)</param>
        /// <returns>Thông tin số lượng đơn hàng của năm</returns>
        [HttpGet("don-hang/theo-nam")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Admin,NhanVien")]

        public async Task<IActionResult> TinhDonHangTheoNam([FromQuery] int nam)
        {
            if (nam < 2000 || nam > DateTime.Now.Year)
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Năm phải từ 2000 đến {DateTime.Now.Year}"
                });

            return await HandleApiOperationAsync<object>(
                async () =>
                {
                    var soDonHang = await _thongKeService.TinhTongDonHangTheoNam(nam);
                    return new { nam, so_don_hang = soDonHang };
                },
                $"Lấy số đơn hàng năm {nam} thành công"
            );
        }

        /// <summary>
        /// Tính tổng số đơn hàng theo ngày
        /// </summary>
        /// <param name="ngay">Ngày cần thống kê</param>
        /// <returns>Thông tin số lượng đơn hàng của ngày</returns>
        [HttpGet("don-hang/theo-ngay")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Admin,NhanVien")]

        public async Task<IActionResult> TinhDonHangTheoNgay([FromQuery] DateOnly ngay)
        {
            return await HandleApiOperationAsync<object>(
                async () =>
                {
                    var soDonHang = await _thongKeService.TinhTongDonHangTheoNgay(ngay);
                    return new { ngay = ngay.ToString("yyyy-MM-dd"), so_don_hang = soDonHang };
                },
                $"Lấy số đơn hàng ngày {ngay:dd/MM/yyyy} thành công"
            );
        }

        /// <summary>
        /// Tính tổng số đơn hàng theo tuần
        /// </summary>
        /// <param name="tuan">Tuần cần thống kê (1-53)</param>
        /// <param name="nam">Năm cần thống kê (2000 đến hiện tại)</param>
        /// <returns>Thông tin số lượng đơn hàng của tuần</returns>
        [HttpGet("don-hang/theo-tuan")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Admin,NhanVien")]

        public async Task<IActionResult> TinhDonHangTheoTuan([FromQuery] int tuan, [FromQuery] int nam)
        {
            if (!ValidateTuanNam(tuan, nam, out var errorResult))
                return errorResult;

            return await HandleApiOperationAsync<object>(
                async () =>
                {
                    var soDonHang = await _thongKeService.TinhTongDonHangTheoTuan(tuan, nam);
                    return new { tuan, nam, so_don_hang = soDonHang };
                },
                $"Lấy số đơn hàng tuần {tuan} năm {nam} thành công"
            );
        }

        #endregion

        #region Thống kê sản phẩm mới

        [HttpGet("san-pham-moi/theo-thang")]
        [Authorize(Roles = "Admin,NhanVien")]

        public async Task<IActionResult> TinhSanPhamMoiTheoThang([FromQuery] int thang, [FromQuery] int nam)
        {
            if (thang < 1 || thang > 12)
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Tháng phải từ 1 đến 12"
                });

            return await HandleApiOperationAsync<object>(
                async () =>
                {
                    var soSanPham = await _thongKeService.TinhTongSanPhamMoiTheoThang(thang, nam);
                    return new { thang, nam, so_san_pham_moi = soSanPham };
                },
                $"Lấy số sản phẩm mới tháng {thang}/{nam} thành công"
            );
        }
        [Authorize(Roles = "Admin,NhanVien")]

        [HttpGet("san-pham-moi/theo-nam")]
        public async Task<IActionResult> TinhSanPhamMoiTheoNam([FromQuery] int nam)
        {
            return await HandleApiOperationAsync<object>(
                async () =>
                {
                    var soSanPham = await _thongKeService.TinhTongSanPhamMoiTheoNam(nam);
                    return new { nam, so_san_pham_moi = soSanPham };
                },
                $"Lấy số sản phẩm mới năm {nam} thành công"
            );
        }

        [HttpGet("san-pham-moi/theo-tuan")]
        [Authorize(Roles = "Admin,NhanVien")]

        public async Task<IActionResult> TinhSanPhamMoiTheoTuan([FromQuery] int tuan, [FromQuery] int nam)
        {
            if (tuan < 1 || tuan > 53)
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Tuần phải từ 1 đến 53"
                });

            return await HandleApiOperationAsync<object>(
                async () =>
                {
                    var soSanPham = await _thongKeService.TinhTongSanPhamMoiTheoTuan(tuan, nam);
                    return new { tuan, nam, so_san_pham_moi = soSanPham };
                },
                $"Lấy số sản phẩm mới tuần {tuan} năm {nam} thành công"
            );
        }

        #endregion

        #region Thống kê nhân viên

        [HttpGet("nhan-vien/theo-thang")]
        [Authorize(Roles = "Admin,NhanVien")]

        public async Task<IActionResult> TinhNhanVienTheoThang([FromQuery] int thang, [FromQuery] int nam)
        {
            if (thang < 1 || thang > 12)
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Tháng phải từ 1 đến 12"
                });

            return await HandleApiOperationAsync<object>(
                async () =>
                {
                    var soNhanVien = await _thongKeService.TinhTongNhanVienTheoThang(thang, nam);
                    return new { thang, nam, so_nhan_vien_moi = soNhanVien };
                },
                $"Lấy số nhân viên mới tháng {thang}/{nam} thành công"
            );
        }

        [HttpGet("nhan-vien/theo-nam")]
        [Authorize(Roles = "Admin,NhanVien")]

        public async Task<IActionResult> TinhNhanVienTheoNam([FromQuery] int nam)
        {
            return await HandleApiOperationAsync<object>(
                async () =>
                {
                    var soNhanVien = await _thongKeService.TinhTongNhanVienTheoNam(nam);
                    return new { nam, so_nhan_vien_moi = soNhanVien };
                },
                $"Lấy số nhân viên mới năm {nam} thành công"
            );
        }

        [HttpGet("nhan-vien/doanh-thu-cao-nhat/theo-thang")]
        [Authorize(Roles = "Admin,NhanVien")]

        public async Task<IActionResult> LayNhanVienDoanhThuCaoNhatTheoThang([FromQuery] int thang, [FromQuery] int nam)
        {
            if (thang < 1 || thang > 12)
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Tháng phải từ 1 đến 12"
                });

            return await HandleApiOperationAsync<object>(
                async () =>
                {
                    var danhSach = await _thongKeService.LayDanhSachNhanVienCoDoanhThuCaoNhatTheoThang(thang, nam);
                    var result = danhSach.Select(item => new
                    {
                        nhan_vien = new
                        {
                            id = item.Item1.id_nhan_vien,
                            ma_nhan_vien = item.Item1.ma_nhan_vien,
                            ten_nhan_vien = item.Item1.ten_nhan_vien,
                            email = item.Item1.email,
                            so_dien_thoai = item.Item1.so_dien_thoai
                        },
                        doanh_thu = item.Item2
                    }).ToList();

                    return new
                    {
                        thang,
                        nam,
                        danh_sach = result,
                        tong_nhan_vien = result.Count,
                        message = $"Lấy danh sách {result.Count} nhân viên có doanh thu cao nhất tháng {thang}/{nam}"
                    };
                },
                $"Lấy danh sách nhân viên doanh thu cao tháng {thang}/{nam} thành công"
            );
        }

        [HttpGet("nhan-vien/doanh-thu-cao-nhat/theo-nam")]
        [Authorize(Roles = "Admin,NhanVien")]

        public async Task<IActionResult> LayNhanVienDoanhThuCaoNhatTheoNam([FromQuery] int nam)
        {
            return await HandleApiOperationAsync<object>(
                async () =>
                {
                    var danhSach = await _thongKeService.LayDanhSachNhanVienCoDoanhThuCaoNhatTheoNam(nam);
                    var result = danhSach.Select(item => new
                    {
                        nhan_vien = new
                        {
                            id = item.Item1.id_nhan_vien,
                            ma_nhan_vien = item.Item1.ma_nhan_vien,
                            ten_nhan_vien = item.Item1.ten_nhan_vien,
                            email = item.Item1.email,
                            so_dien_thoai = item.Item1.so_dien_thoai
                        },
                        doanh_thu = item.Item2
                    }).ToList();

                    return new
                    {
                        nam,
                        danh_sach = result,
                        tong_nhan_vien = result.Count,
                        message = $"Lấy danh sách {result.Count} nhân viên có doanh thu cao nhất năm {nam}"
                    };
                },
                $"Lấy danh sách nhân viên doanh thu cao năm {nam} thành công"
            );
        }

        #endregion
    }
}