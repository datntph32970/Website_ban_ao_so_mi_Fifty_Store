using API.DbConects.DTOs.Admin.TaiKhoan;
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
    [Authorize(Roles = "Admin")]
    public class NhanVienController : ControllerBase
    {
        private readonly IBaseService<NhanVien> _nhanVienService;
        private readonly IBaseService<TaiKhoan> _taiKhoanService;
        private readonly IJwtServices _jwtServices;
        private readonly IEmailService _emailService;

        public NhanVienController(
            IBaseService<NhanVien> nhanVienService,
            IBaseService<TaiKhoan> taiKhoanService,
            IJwtServices jwtServices,
            IEmailService emailService)
        {
            _nhanVienService = nhanVienService;
            _taiKhoanService = taiKhoanService;
            _jwtServices = jwtServices;
            _emailService = emailService;
        }

        [HttpGet("get-all-nhan-vien")]
        public async Task<IActionResult> GetAllNhanVien()
        {
            var nhanVien = await _nhanVienService.GetAllWithIncludeAsync(
                q => q.Include(n => n.TaiKhoanNhanVien)
            );
            return Ok(nhanVien);
        }

        [HttpGet("get-nhan-vien-by-id")]
        public async Task<IActionResult> GetNhanVienById(Guid id)
        {
            var nhanVien = await _nhanVienService.GetByIdWithIncludeAsync(
                id,
                q => q.Include(n => n.TaiKhoanNhanVien)
            );
            return Ok(nhanVien);
        }

        [HttpPost("create-nhan-vien")]
        public async Task<IActionResult> CreateNhanVien(ThemNhanVienAdminDTO themNhanVienAdminDTO)
        {
            // Validate required fields
            if (string.IsNullOrEmpty(themNhanVienAdminDTO.ten_dang_nhap))
                return BadRequest("Tên đăng nhập không được để trống");

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
            var existingUser = await _taiKhoanService.ExistsAsync(x => x.ten_dang_nhap == themNhanVienAdminDTO.ten_dang_nhap);
            if (existingUser)
                return BadRequest("Tên đăng nhập đã tồn tại");
            var existingEmail = await _nhanVienService.ExistsAsync(x => x.email == themNhanVienAdminDTO.email);
            if (existingEmail)
                return BadRequest("Email đã tồn tại");
            var existingPhone = await _nhanVienService.ExistsAsync(x => x.so_dien_thoai == themNhanVienAdminDTO.so_dien_thoai);
            if (existingPhone)
                return BadRequest("Số điện thoại đã tồn tại");
            var existingCCCD = await _nhanVienService.ExistsAsync(x => x.cccd == themNhanVienAdminDTO.cccd);
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
        public async Task<IActionResult> UpdateQuyenHoacTrangThaiNhanVien(SuaTaiKhoanNhanVienDTO suaTaiKhoanNhanVienDTO)
        {
            var nhanVien = await _nhanVienService.GetByIdAsync(Guid.Parse(suaTaiKhoanNhanVienDTO.id_nhan_vien));
            if (nhanVien == null)
                return NotFound("Không tìm thấy nhân viên");
            var taikhoan = await _taiKhoanService.GetByIdAsync(nhanVien.id_tai_khoan);
            if (taikhoan == null)
                return NotFound("Không tìm thấy tài khoản");

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
        public async Task<IActionResult> DeleteNhanVien(XoaNhanVienAdminDTO xoaNhanVienAdminDTO)
        {
            // Tìm kiếm nhân viên với các bảng liên quan
            var nhanVien = await _nhanVienService.GetByIdWithIncludeAsync(
                Guid.Parse(xoaNhanVienAdminDTO.id_nhan_vien),
                q => q.Include(n => n.XulyHoaDons)
                      .Include(n => n.TaoXuatXus)
                      .Include(n => n.SuaXuatXus)
                      .Include(n => n.TaoSanPhamChiTiets)
                      .Include(n => n.SuaSanPhamChiTiets)
                      .Include(n => n.TaoSanPhams)
                      .Include(n => n.SuaSanPhams)
                      .Include(n => n.TaoThuongHieus)
                      .Include(n => n.SuaThuongHieus)
                      .Include(n => n.TaoMauSacs)
                      .Include(n => n.SuaMauSacs)
                      .Include(n => n.TaoKieuDangs)
                      .Include(n => n.SuaKieuDangs)
                      .Include(n => n.TaoKichCos)
                      .Include(n => n.SuaKichCos)
                      .Include(n => n.TaoHinhAnhs)
                      .Include(n => n.SuaHinhAnhs)
                      .Include(n => n.TaoChatLieus)
                      .Include(n => n.SuaChatLieus)
                      .Include(n => n.TaoGiamGias)
                      .Include(n => n.SuaGiamGias)
                      .Include(n => n.TaoKhuyenMais)
                      .Include(n => n.SuaKhuyenMais)
            );

            // Kiểm tra nếu không tìm thấy nhân viên
            if (nhanVien == null)
            {
                return NotFound("Nhân viên không tồn tại.");
            }

            // Kiểm tra nếu nhân viên có dữ liệu liên quan
            if (nhanVien.XulyHoaDons?.Any() == true ||
                nhanVien.TaoXuatXus?.Any() == true ||
                nhanVien.SuaXuatXus?.Any() == true ||
                nhanVien.TaoSanPhamChiTiets?.Any() == true ||
                nhanVien.SuaSanPhamChiTiets?.Any() == true ||
                nhanVien.TaoSanPhams?.Any() == true ||
                nhanVien.SuaSanPhams?.Any() == true ||
                nhanVien.TaoThuongHieus?.Any() == true ||
                nhanVien.SuaThuongHieus?.Any() == true ||
                nhanVien.TaoMauSacs?.Any() == true ||
                nhanVien.SuaMauSacs?.Any() == true ||
                nhanVien.TaoKieuDangs?.Any() == true ||
                nhanVien.SuaKieuDangs?.Any() == true ||
                nhanVien.TaoKichCos?.Any() == true ||
                nhanVien.SuaKichCos?.Any() == true ||
                nhanVien.TaoHinhAnhs?.Any() == true ||
                nhanVien.SuaHinhAnhs?.Any() == true ||
                nhanVien.TaoChatLieus?.Any() == true ||
                nhanVien.SuaChatLieus?.Any() == true ||
                nhanVien.TaoGiamGias?.Any() == true ||
                nhanVien.SuaGiamGias?.Any() == true ||
                nhanVien.TaoKhuyenMais?.Any() == true ||
                nhanVien.SuaKhuyenMais?.Any() == true)
            {
                return BadRequest("Không thể xóa nhân viên vì đã có dữ liệu liên quan.");
            }

            // Thực hiện xóa nhân viên
            var result = await _nhanVienService.ExecuteInTransactionAsync(async () =>
            {
                var deleteNhanVienResult = await _nhanVienService.DeleteAsync(nhanVien.id_nhan_vien);
                if (!deleteNhanVienResult) return false;
                var taiKhoan = await _taiKhoanService.GetByIdAsync(nhanVien.id_tai_khoan);
                if (taiKhoan != null)
                {
                    var deleteTaiKhoanResult = await _taiKhoanService.DeleteAsync(taiKhoan.id_tai_khoan);
                    if (!deleteTaiKhoanResult) return false;
                }
                return true;
            });
            if (!result)
            {
                return BadRequest("Lỗi khi xóa nhân viên.");
            }

            return Ok("Xóa nhân viên thành công.");
        }
        [HttpGet("search-nhan-vien")]
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
    }
}