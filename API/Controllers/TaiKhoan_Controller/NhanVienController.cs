using API.DbConects.DTOs.Admin.TaiKhoan;
using API.DbConects.Entities.Entities_Hoa_Don;
using API.DbConects.Entities.Entities_Khuyen_Mai;
using API.DbConects.Entities.Entities_San_Pham;
using API.DbConects.Entities.Entities_Tai_Khoan;
using API.Services.Interfaces;
using API.Services.JwtServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;

namespace API.Controllers.TaiKhoan_Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,NhanVien")]
    public class NhanVienController : ControllerBase
    {
        private readonly IBaseService<NhanVien> _nhanVienService;
        private readonly IBaseService<TaiKhoan> _taiKhoanService;
        private readonly IJwtServices _jwtServices;
        private readonly IEmailService _emailService;
        private readonly IBaseService<XuatXu> _xuatXuService;
        private readonly IBaseService<HoaDon> _hoaDonService;
        private readonly IBaseService<HoaDonChiTiet> _hoaDonChiTietService;
        private readonly IBaseService<SanPham> _sanPhamService;
        private readonly IBaseService<SanPhamChiTiet> _sanPhamChiTietService;
        private readonly IBaseService<ThuongHieu> _thuongHieuService;
        private readonly IBaseService<MauSac> _mauSacService;
        private readonly IBaseService<KieuDang> _kieuDangService;
        private readonly IBaseService<KichCo> _kichCoService;
        private readonly IBaseService<DanhMuc> _danhMucService;
        private readonly IBaseService<KhachHang> _khachHangService;
        private readonly IBaseService<ChatLieu> _chatLieuService;
        private readonly IBaseService<GiamGia> _giamGiaService;
        private readonly IBaseService<KhuyenMai> _khuyenMaiService;
        private readonly IBaseService<HinhAnh> _hinhAnhService;

        public NhanVienController(
            IBaseService<NhanVien> nhanVienService,
            IBaseService<TaiKhoan> taiKhoanService,
            IJwtServices jwtServices,
            IEmailService emailService,
            IBaseService<XuatXu> xuatXuService,
            IBaseService<KhachHang> khachHangService,
            IBaseService<SanPham> sanPhamService,
            IBaseService<SanPhamChiTiet> sanPhamChiTietService,
            IBaseService<ThuongHieu> thuongHieuService,
            IBaseService<MauSac> mauSacService,
            IBaseService<KieuDang> kieuDangService,
            IBaseService<KichCo> kichCoService,
            IBaseService<ChatLieu> chatLieuService,
            IBaseService<GiamGia> giamGiaService,
            IBaseService<KhuyenMai> khuyenMaiService,
            IBaseService<DanhMuc> danhMucService,
             IBaseService<HoaDon> hoaDonService,
             IBaseService<HoaDonChiTiet> hoaDonChiTietService,
            IBaseService<HinhAnh> hinhAnhService)
        {
            _nhanVienService = nhanVienService;
            _taiKhoanService = taiKhoanService;
            _jwtServices = jwtServices;
            _emailService = emailService;
            _xuatXuService = xuatXuService;
            _sanPhamService = sanPhamService;
            _sanPhamChiTietService = sanPhamChiTietService;
            _thuongHieuService = thuongHieuService;
            _mauSacService = mauSacService;
            _kieuDangService = kieuDangService;
            _kichCoService = kichCoService;
            _chatLieuService = chatLieuService;
            _giamGiaService = giamGiaService;
            _khuyenMaiService = khuyenMaiService;
            _hinhAnhService = hinhAnhService;
            _danhMucService = danhMucService;
            _khachHangService = khachHangService;
            _hoaDonService = hoaDonService;
            _hoaDonChiTietService = hoaDonChiTietService;
        }

        [HttpGet("get-all-nhan-vien")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllNhanVien()
        {
            var nhanVien = await _nhanVienService.GetAllWithIncludeAsync(
                q => q.Include(n => n.TaiKhoanNhanVien)
            );
            return Ok(nhanVien);
        }

        [HttpGet("get-nhan-vien-by-id")]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> GetNhanVienById(Guid id)
        {
            var nhanVien = await _nhanVienService.GetByIdWithIncludeAsync(
                id,
                q => q.Include(n => n.TaiKhoanNhanVien)
            );
            return Ok(nhanVien);
        }
        [HttpGet("get-nhan-vien-dang-dang-nhap")]
        [Authorize(Roles = "NhanVien,Admin")]
        public async Task<IActionResult> GetNhanVienDangDangNhap()
        {
            var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            var idTaiKhoan = _jwtServices.GetUserIdFromToken(token);
            var nhanVien = await _nhanVienService.GetByConditionWithIncludeAsync(x => x.id_tai_khoan == idTaiKhoan,
                q => q.Include(n => n.TaiKhoanNhanVien)
            );
            return Ok(nhanVien);
        }
        [HttpPost("create-nhan-vien")]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> CreateNhanVien(ThemNhanVienAdminDTO themNhanVienAdminDTO)
        {
            // Validate required fields
            if (string.IsNullOrEmpty(themNhanVienAdminDTO.ten_dang_nhap))
                return BadRequest("Tên đăng nhập không được để trống");
            if (themNhanVienAdminDTO.ten_dang_nhap.Contains(" "))
                return BadRequest("Tên đăng nhập không được chứa khoảng trắng");

            if (string.IsNullOrEmpty(themNhanVienAdminDTO.ho_ten))
                return BadRequest("Họ tên không được để trống");

            if (string.IsNullOrEmpty(themNhanVienAdminDTO.so_dien_thoai))
                return BadRequest("Số điện thoại không được để trống");

            if (string.IsNullOrEmpty(themNhanVienAdminDTO.email))
                return BadRequest("Email không được để trống");

            if (string.IsNullOrEmpty(themNhanVienAdminDTO.gioi_tinh))
                return BadRequest("Giới tính không được để trống");

            if (string.IsNullOrEmpty(themNhanVienAdminDTO.cccd))
                return BadRequest("CCCD không được để trống");
            if (themNhanVienAdminDTO.cccd.Contains(" "))
                return BadRequest("Căn cước công dân không được chứa khoảng trắng");
            if (string.IsNullOrEmpty(themNhanVienAdminDTO.dia_chi))
                return BadRequest("Địa chỉ không được để trống");

            if (string.IsNullOrEmpty(themNhanVienAdminDTO.chuc_vu))
                return BadRequest("Vai trò không được để trống");

            if (string.IsNullOrEmpty(themNhanVienAdminDTO.trang_thai))
                return BadRequest("Trạng thái không được để trống");

            // Validate email format
            if (!themNhanVienAdminDTO.email.Contains("@") || !themNhanVienAdminDTO.email.Contains("."))
                return BadRequest("Email không hợp lệ");

            // Validate phone number format (basic check for numeric and length)
            if (!themNhanVienAdminDTO.so_dien_thoai.All(char.IsDigit) || themNhanVienAdminDTO.so_dien_thoai.Length != 10)
                return BadRequest("Số điện thoại không hợp lệ");

            // Validate date of birth (must be in the past)
            if (themNhanVienAdminDTO.ngay_sinh >= DateTime.Now)
                return BadRequest("Ngày sinh không hợp lệ");

            // Check if username already exists
            var existingUser = await _taiKhoanService.ExistsAsync(x =>
                x.ten_dang_nhap.Trim().ToLower() == themNhanVienAdminDTO.ten_dang_nhap.Trim().ToLower());
            if (existingUser)
                return BadRequest("Tên đăng nhập đã tồn tại");
            var existingEmail = await _nhanVienService.ExistsAsync(x =>
                x.email.Trim().ToLower() == themNhanVienAdminDTO.email.Trim().ToLower());
            if (existingEmail)
                return BadRequest("Email đã tồn tại");
            var existingPhone = await _nhanVienService.ExistsAsync(x =>
                x.so_dien_thoai.Trim().ToLower() == themNhanVienAdminDTO.so_dien_thoai.Trim().ToLower());
            if (existingPhone)
                return BadRequest("Số điện thoại đã tồn tại");
            var existingCCCD = await _nhanVienService.ExistsAsync(x => x.cccd.Trim() == themNhanVienAdminDTO.cccd.Trim());
            if (existingCCCD)
                return BadRequest("CCCD đã tồn tại");

            // Generate random password
            string randomPassword = GenerateRandomPassword();
            string hashedPassword = await BamMatKhau(randomPassword);

            // Create TaiKhoan
            var taiKhoan = new TaiKhoan
            {
                id_tai_khoan = Guid.NewGuid(),
                ten_dang_nhap = themNhanVienAdminDTO.ten_dang_nhap,
                mat_khau = hashedPassword,
                chuc_vu = themNhanVienAdminDTO.chuc_vu,
                trang_thai = themNhanVienAdminDTO.trang_thai,
                da_doi_mat_khau = false,
                ma_tai_khoan = TaoMaTaiKhoan()
            };
            var idnguoitao = GetIDNguoiTao();
            if (idnguoitao == null)
                return BadRequest("Không tìm thấy người tạo");
            // Create NhanVien
            var nhanVien = new NhanVien
            {
                id_nhan_vien = Guid.NewGuid(),
                id_tai_khoan = taiKhoan.id_tai_khoan,
                ten_nhan_vien = themNhanVienAdminDTO.ho_ten,
                so_dien_thoai = themNhanVienAdminDTO.so_dien_thoai,
                ma_nhan_vien = taiKhoan.ma_tai_khoan,
                email = themNhanVienAdminDTO.email,
                gioi_tinh = themNhanVienAdminDTO.gioi_tinh,
                cccd = themNhanVienAdminDTO.cccd,
                dia_chi = themNhanVienAdminDTO.dia_chi,
                id_nguoi_tao = (Guid)idnguoitao,
                ngay_sinh = themNhanVienAdminDTO.ngay_sinh,
                ngay_tao = DateTime.Now,
                trang_thai = themNhanVienAdminDTO.trang_thai
            };

            // Save to database
            var result = await _taiKhoanService.ExecuteInTransactionAsync(async () =>
            {
                var createTaiKhoanResult = await _taiKhoanService.CreateAsync(taiKhoan);
                if (!createTaiKhoanResult) return false;

                var createNhanVienResult = await _nhanVienService.CreateAsync(nhanVien);

                // Send email with password
                string emailBody = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <h2 style='color: #2c3e50; text-align: center; border-bottom: 2px solid #3498db; padding-bottom: 10px;'>Thông tin tài khoản nhân viên</h2>
                    <div style='background-color: #f9f9f9; padding: 20px; border-radius: 5px; margin: 20px 0;'>
                        <p style='color: #34495e;'>Xin chào <strong>{nhanVien.ten_nhan_vien}</strong>,</p>
                        <p style='color: #34495e;'>Tài khoản của bạn đã được tạo thành công với thông tin sau:</p>
                        <div style='background-color: #ffffff; padding: 15px; border-left: 4px solid #3498db; margin: 10px 0;'>
                            <p style='margin: 5px 0;'><strong>Tên đăng nhập:</strong> {taiKhoan.ten_dang_nhap}</p>
                            <p style='margin: 5px 0;'><strong>Mật khẩu:</strong> {randomPassword}</p>
                        </div>
                        <p style='color: #e74c3c; font-weight: bold;'>Vui lòng đổi mật khẩu sau khi đăng nhập lần đầu.</p>
                    </div>
                    <div style='text-align: right; margin-top: 20px; color: #7f8c8d;'>
                        <p style='margin: 5px 0;'>Trân trọng,</p>
                        <p style='margin: 5px 0; font-weight: bold;'>Ban quản trị</p>
                    </div>
                </div>";

                var emailSent = await _emailService.SendEmailAsync(
                    nhanVien.email,
                    "Thông tin tài khoản nhân viên",
                    emailBody
                );

                return createNhanVienResult;

            });

            if (!result)
                return BadRequest("Lỗi khi tạo nhân viên");

            return Ok("Tạo nhân viên thành công");
        }
        [HttpPut("update-quyen-hoac-trang-thai-nhan-vien")]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> UpdateQuyenHoacTrangThaiNhanVien(SuaTaiKhoanNhanVienDTO suaTaiKhoanNhanVienDTO)
        {
            var id_nhan_vien = GetIDNguoiTao();
            var nhanVien = await _nhanVienService.GetByIdAsync(Guid.Parse(suaTaiKhoanNhanVienDTO.id_nhan_vien));
            if (nhanVien == null)
                return NotFound("Không tìm thấy nhân viên");
            var taikhoan = await _taiKhoanService.GetByIdAsync(nhanVien.id_tai_khoan);
            if (taikhoan == null)
                return NotFound("Không tìm thấy tài khoản");

            // Không cho phép cập nhật tài khoản có mã TK00000001
            if (taikhoan.ma_tai_khoan == "TK00000001")
                return BadRequest("Tài khoản này là cố định không được cập nhật");

            // Kiểm tra xem người dùng có đang cố cập nhật chính mình không
            var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            var currentUserId = _jwtServices.GetUserIdFromToken(token);
            if (currentUserId == nhanVien.id_tai_khoan)
                return BadRequest("Không thể cập nhật trạng thái cho chính mình");

            // Update Chức vụ if provided
            if (!string.IsNullOrEmpty(suaTaiKhoanNhanVienDTO.chuc_vu))
            {
                if (suaTaiKhoanNhanVienDTO.chuc_vu != ChucVuTaiKhoan.Admin.ToString() && suaTaiKhoanNhanVienDTO.chuc_vu != ChucVuTaiKhoan.NhanVien.ToString())
                    return BadRequest("Vai trò không hợp lệ");

                taikhoan.chuc_vu = suaTaiKhoanNhanVienDTO.chuc_vu;
            }

            // Update Trạng thái if provided
            if (!string.IsNullOrEmpty(suaTaiKhoanNhanVienDTO.trang_thai))
            {
                if (suaTaiKhoanNhanVienDTO.trang_thai != "HoatDong" && suaTaiKhoanNhanVienDTO.trang_thai != "KhongHoatDong")
                    return BadRequest("Trạng thái không hợp lệ");

                taikhoan.trang_thai = suaTaiKhoanNhanVienDTO.trang_thai;
                nhanVien.trang_thai = suaTaiKhoanNhanVienDTO.trang_thai;
                nhanVien.ngay_sua = DateTime.Now;

            }

            var result = await _taiKhoanService.ExecuteInTransactionAsync(async () =>
            {
                var updateTaiKhoanResult = await _taiKhoanService.UpdateAsync(taikhoan);
                if (!updateTaiKhoanResult) return false;
                var updateNhanVienResult = await _nhanVienService.UpdateAsync(nhanVien);
                return updateNhanVienResult;
            });

            if (!result)
                return BadRequest("Lỗi khi cập nhật quyền hoặc trạng thái nhân viên");

            return Ok("Cập nhật quyền hoặc trạng thái nhân viên thành công");
        }

        private string GenerateRandomPassword()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
            var random = new Random();
            var password = new StringBuilder();

            // Ensure at least one of each required character type
            password.Append(chars.Where(c => char.IsUpper(c)).ElementAt(random.Next(26))); // Uppercase
            password.Append(chars.Where(c => char.IsLower(c)).ElementAt(random.Next(26))); // Lowercase
            password.Append(chars.Where(c => char.IsDigit(c)).ElementAt(random.Next(10))); // Digit
            password.Append(chars.Where(c => !char.IsLetterOrDigit(c)).ElementAt(random.Next(8))); // Special char

            // Add 4 more random characters
            for (int i = 0; i < 4; i++)
            {
                password.Append(chars[random.Next(chars.Length)]);
            }

            // Shuffle the password
            return new string(password.ToString().ToCharArray().OrderBy(x => random.Next()).ToArray());
        }

        private async Task<string> BamMatKhau(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                var sb = new StringBuilder();
                foreach (var b in hashedBytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }

        private string TaoMaTaiKhoan()
        {
            var lastTaiKhoan = _taiKhoanService.GetAllAsync().Result.OrderByDescending(t => t.ma_tai_khoan).FirstOrDefault();
            if (lastTaiKhoan == null)
                return "TK00000001";
            int startNumber = int.Parse(lastTaiKhoan.ma_tai_khoan.Substring(2)) + 1;
            return "TK" + startNumber.ToString("D8");
        }
        private Guid? GetIDNguoiTao()
        {
            var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            var idKhachHang = _jwtServices.GetUserIdFromToken(token);
            return idKhachHang;
        }
        [HttpDelete("delete-nhan-vien")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteNhanVien(XoaNhanVienAdminDTO xoaNhanVienAdminDTO)
        {
            try
            {
                // Kiểm tra ID nhân viên
                if (string.IsNullOrEmpty(xoaNhanVienAdminDTO.id_nhan_vien))
                    return BadRequest("ID nhân viên không được để trống");

                // Lấy thông tin nhân viên và tài khoản
                var nhanVien = await _nhanVienService.GetByIdWithIncludeAsync(
                    Guid.Parse(xoaNhanVienAdminDTO.id_nhan_vien),
                    q => q.Include(n => n.TaiKhoanNhanVien)
                );

                if (nhanVien == null)
                    return NotFound("Không tìm thấy nhân viên");

                // Kiểm tra tài khoản cố định
                if (nhanVien.TaiKhoanNhanVien?.ma_tai_khoan == "TK00000001")
                    return BadRequest("Không thể xóa tài khoản cố định của hệ thống");

                // Kiểm tra không cho phép xóa chính mình
                var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                var currentUserId = _jwtServices.GetUserIdFromToken(token);
                if (currentUserId == nhanVien.id_tai_khoan)
                    return BadRequest("Không thể xóa tài khoản của chính mình");

                // Kiểm tra dữ liệu liên quan
                var hasRelatedData = await CheckRelatedData(nhanVien.id_nhan_vien);
                if (hasRelatedData)
                    return BadRequest("Không thể xóa nhân viên vì đã có dữ liệu liên quan");

                // Thực hiện xóa trong transaction
                var result = await _nhanVienService.ExecuteInTransactionAsync(async () =>
                {
                    // Xóa nhân viên
                    var deleteNhanVienResult = await _nhanVienService.DeleteAsync(nhanVien.id_nhan_vien);
                    if (!deleteNhanVienResult) return false;

                    // Xóa tài khoản
                    if (nhanVien.TaiKhoanNhanVien != null)
                    {
                        var deleteTaiKhoanResult = await _taiKhoanService.DeleteAsync(nhanVien.TaiKhoanNhanVien.id_tai_khoan);
                        if (!deleteTaiKhoanResult) return false;
                    }

                    return true;
                });

                if (!result)
                    return BadRequest("Lỗi khi xóa nhân viên");

                return Ok("Xóa nhân viên thành công");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        private async Task<bool> CheckRelatedData(Guid nhanVienId)
        {
            // Kiểm tra hóa đơn
            var hasHoaDon = await _hoaDonService.ExistsAsync(n =>
                n.id_nhan_vien_xu_ly == nhanVienId);
            var hasHoaDonChiTiet = await _hoaDonChiTietService.ExistsAsync(n =>
                            n.id_nhan_vien_xu_ly == nhanVienId);
            // Kiểm tra xuất xứ
            var hasXuatXu = await _xuatXuService.ExistsAsync(x =>
                x.id_nguoi_tao == nhanVienId || x.id_nguoi_sua == nhanVienId);

            // Kiểm tra sản phẩm
            var hasSanPham = await _sanPhamService.ExistsAsync(x =>
                x.id_nguoi_tao == nhanVienId || x.id_nguoi_sua == nhanVienId);

            // Kiểm tra sản phẩm chi tiết
            var hasSanPhamChiTiet = await _sanPhamChiTietService.ExistsAsync(x =>
                x.id_nguoi_tao == nhanVienId || x.id_nguoi_sua == nhanVienId);

            // Kiểm tra thuộc tính sản phẩm
            var hasThuongHieu = await _thuongHieuService.ExistsAsync(x =>
                x.id_nguoi_tao == nhanVienId || x.id_nguoi_sua == nhanVienId);
            var hasMauSac = await _mauSacService.ExistsAsync(x =>
                x.id_nguoi_tao == nhanVienId || x.id_nguoi_sua == nhanVienId);
            var hasKieuDang = await _kieuDangService.ExistsAsync(x =>
                x.id_nguoi_tao == nhanVienId || x.id_nguoi_sua == nhanVienId);
            var hasKichCo = await _kichCoService.ExistsAsync(x =>
                x.id_nguoi_tao == nhanVienId || x.id_nguoi_sua == nhanVienId);
            var hasChatLieu = await _chatLieuService.ExistsAsync(x =>
                x.id_nguoi_tao == nhanVienId || x.id_nguoi_sua == nhanVienId);
            var hasDanhMuc = await _danhMucService.ExistsAsync(x =>
                            x.id_nguoi_tao == nhanVienId || x.id_nguoi_sua == nhanVienId);

            // Kiểm tra khuyến mãi
            var hasGiamGia = await _giamGiaService.ExistsAsync(x =>
                x.id_nguoi_tao == nhanVienId || x.id_nguoi_cap_nhat == nhanVienId);
            var hasKhuyenMai = await _khuyenMaiService.ExistsAsync(x =>
                x.id_nguoi_tao == nhanVienId || x.id_nguoi_sua == nhanVienId);

            // Kiểm tra hình ảnh
            var hasHinhAnh = await _hinhAnhService.ExistsAsync(x =>
                x.id_nguoi_tao == nhanVienId || x.id_nguoi_sua == nhanVienId);

            return hasHoaDon || hasXuatXu || hasSanPham || hasSanPhamChiTiet || hasHoaDonChiTiet || hasDanhMuc ||
                   hasThuongHieu || hasMauSac || hasKieuDang || hasKichCo ||
                   hasChatLieu || hasGiamGia || hasKhuyenMai || hasHinhAnh;
        }
        [HttpGet("search-nhan-vien")]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> SearchNhanVien(string keyword)
        {
            if (string.IsNullOrEmpty(keyword))
            {
                return BadRequest("Từ khóa tìm kiếm không được để trống.");
            }

            // Lấy danh sách nhân viên phù hợp, bao gồm thông tin tài khoản
            var nhanViens = await _nhanVienService.GetAllWithIncludeAsync(
                q => q.Include(nv => nv.TaiKhoanNhanVien)
            );

            var filteredNhanViens = nhanViens.Where(nv =>
                nv.ma_nhan_vien.Contains(keyword) ||
                nv.ten_nhan_vien.Contains(keyword) ||
                nv.email.Contains(keyword) ||
                nv.so_dien_thoai.Contains(keyword)
            ).ToList();

            // Trả về kết quả
            return Ok(filteredNhanViens);
        }
        [HttpPut("cap-nhat-thong-tin-nhan-vien")]
        [Authorize(Roles = "NhanVien,Admin")]
        public async Task<IActionResult> CapNhatThongTinNhanVien(CapNhatThongTinNhanVienDTO capNhatThongTinDTO)
        {
            try
            {
                // Lấy ID người dùng từ token
                var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                var idTaiKhoan = _jwtServices.GetUserIdFromToken(token);
                if (idTaiKhoan == null)
                    return BadRequest("Không tìm thấy thông tin người dùng");

                // Tìm nhân viên theo id_tai_khoan
                var nhanViens = await _nhanVienService.GetByConditionWithIncludeAsync(x => x.id_tai_khoan == idTaiKhoan,
                    q => q.Include(n => n.TaiKhoanNhanVien)
                );
                var nhanVien = nhanViens.FirstOrDefault();
                if (nhanVien == null)
                    return NotFound("Không tìm thấy thông tin nhân viên");

                // Kiểm tra email đã tồn tại chưa (nếu email thay đổi)
                if (nhanVien.email != capNhatThongTinDTO.email)
                {
                    var existingEmail = await _nhanVienService.ExistsAsync(x =>
                        x.email.Trim().ToLower() == capNhatThongTinDTO.email.Trim().ToLower() &&
                        x.id_nhan_vien != nhanVien.id_nhan_vien);
                    if (existingEmail)
                        return BadRequest("Email đã tồn tại");
                }

                // Kiểm tra số điện thoại đã tồn tại chưa (nếu số điện thoại thay đổi)
                if (nhanVien.so_dien_thoai != capNhatThongTinDTO.so_dien_thoai)
                {
                    var existingPhone = await _nhanVienService.ExistsAsync(x =>
                        x.so_dien_thoai.Trim().ToLower() == capNhatThongTinDTO.so_dien_thoai.Trim().ToLower() &&
                        x.id_nhan_vien != nhanVien.id_nhan_vien);
                    if (existingPhone)
                        return BadRequest("Số điện thoại đã tồn tại");
                }

                // Kiểm tra CCCD đã tồn tại chưa (nếu CCCD thay đổi)
                if (nhanVien.cccd != capNhatThongTinDTO.cccd)
                {
                    var existingCCCD = await _nhanVienService.ExistsAsync(x =>
                        x.cccd.Trim().ToLower() == capNhatThongTinDTO.cccd.Trim().ToLower() &&
                        x.id_nhan_vien != nhanVien.id_nhan_vien);
                    if (existingCCCD)
                        return BadRequest("CCCD đã tồn tại");
                }

                // Cập nhật thông tin nhân viên
                nhanVien.ten_nhan_vien = capNhatThongTinDTO.ho_ten;
                nhanVien.so_dien_thoai = capNhatThongTinDTO.so_dien_thoai;
                nhanVien.email = capNhatThongTinDTO.email;
                nhanVien.gioi_tinh = capNhatThongTinDTO.gioi_tinh;
                nhanVien.cccd = capNhatThongTinDTO.cccd;
                nhanVien.dia_chi = capNhatThongTinDTO.dia_chi;
                nhanVien.ngay_sinh = capNhatThongTinDTO.ngay_sinh;
                nhanVien.ngay_sua = DateTime.Now;

                // Lưu thay đổi
                var result = await _nhanVienService.UpdateAsync(nhanVien);
                if (!result)
                    return BadRequest("Lỗi khi cập nhật thông tin nhân viên");

                return Ok("Cập nhật thông tin nhân viên thành công");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }
    }
}