using System.Security.Cryptography;
using System.Text;
using API.DbConects.DTOs.Client.TaiKhoan;
using API.DbConects.Entities.Entities_Tai_Khoan;
using API.Repositories.Interfaces;
using API.Services.Interfaces;
using API.Services.JwtServices;
using API.Services.Validation;

namespace API.Services.Implementations
{
    public class AuthenService : BaseService<TaiKhoan>, IAuthenService
    {
        private readonly TaiKhoanClientValidationService _dangKyValidationService;
        private readonly DangNhapValidationService _dangNhapValidationService;
        private readonly DoiMatKhauValidationService _doiMatKhauValidationService;
        private readonly IJwtServices _jwtServices;
        private readonly IBaseRepository<KhachHang> _repositoryKhachHang;
        private readonly IBaseRepository<TaiKhoan> _repository;
        public AuthenService(
            IBaseRepository<TaiKhoan> repository,
            TaiKhoanClientValidationService dangKyValidationService,
            DangNhapValidationService dangNhapValidationService,
            DoiMatKhauValidationService doiMatKhauValidationService,
            IJwtServices jwtServices,
            IBaseRepository<KhachHang> repositoryKhachHang)
            : base(repository)
        {
            _dangKyValidationService = dangKyValidationService;
            _dangNhapValidationService = dangNhapValidationService;
            _doiMatKhauValidationService = doiMatKhauValidationService;
            _jwtServices = jwtServices;
            _repository = repository;
            _repositoryKhachHang = repositoryKhachHang;
        }

        public async Task<(bool success, object message)> DangNhapAsync(DangNhapClientDTO dto)
        {
            var validationResult = await _dangNhapValidationService.ValidateAsync(dto);
            if (!validationResult.IsValid)
                return (false, string.Join(", ", validationResult.Errors));

            var hashedPassword = await BamMatKhau(dto.mat_khau);
            var taiKhoan = await _repository.GetFirstOrDefaultAsync(x => x.ten_dang_nhap == dto.ten_dang_nhap && x.mat_khau == hashedPassword);

            if (taiKhoan == null)
                return (false, "Tên đăng nhập hoặc mật khẩu không chính xác");

            var token = _jwtServices.GenerateJwtToken(taiKhoan.id_tai_khoan, taiKhoan.ten_dang_nhap, taiKhoan.chuc_vu, taiKhoan.ma_tai_khoan);
            return (true, new { token });
        }

        public async Task<(bool success, string message)> DangKyAsync(DangKyClientDTO dto)
        {
            var validationResult = await _dangKyValidationService.ValidateAsync(dto);
            if (!validationResult.IsValid)
                return (false, string.Join(", ", validationResult.Errors));

            var taiKhoan = await _repository.GetFirstOrDefaultAsync(x => x.ten_dang_nhap == dto.ten_dang_nhap);
            if (taiKhoan != null)
                return (false, "Tên đăng nhập đã tồn tại");

            var newTaiKhoan = new TaiKhoan
            {
                id_tai_khoan = Guid.NewGuid(),
                ten_dang_nhap = dto.ten_dang_nhap,
                mat_khau = await BamMatKhau(dto.mat_khau),
                chuc_vu = "KhachHang",
                trang_thai = "HoatDong",
                da_doi_mat_khau = false,
                ma_tai_khoan = TaoMaTaiKhoan()
            };
            var khachHang = new KhachHang
            {
                id_khach_hang = Guid.NewGuid(),
                id_tai_khoan = newTaiKhoan.id_tai_khoan,
                ma_khach_hang = newTaiKhoan.ma_tai_khoan,
                ngay_tao = DateTime.Now,
                trang_thai = "HoatDong",
            };
            var result = await _repository.ExecuteInTransactionAsync(async () =>
            {
                var createTaiKhoanResult = await _repository.CreateAsync(newTaiKhoan);
                if (!createTaiKhoanResult) return false;

                var createKhachHangResult = await _repositoryKhachHang.CreateAsync(khachHang);
                return createKhachHangResult;
            });

            return result ? (true, "Đăng ký thành công") : (false, "Lỗi khi tạo tài khoản");
        }

        public async Task<(bool success, string message)> DoiMatKhauAsync(DoiMatKhauClientDTO dto)
        {
            var validationResult = await _doiMatKhauValidationService.ValidateAsync(dto);
            if (!validationResult.IsValid)
                return (false, string.Join(", ", validationResult.Errors));

            var taiKhoan = await _repository.GetFirstOrDefaultAsync(x => x.ten_dang_nhap == dto.ten_dang_nhap);
            if (taiKhoan == null)
                return (false, "Tên đăng nhập không tồn tại");

            if (taiKhoan.mat_khau != await BamMatKhau(dto.mat_khau_cu))
                return (false, "Mật khẩu cũ không chính xác");

            taiKhoan.mat_khau = await BamMatKhau(dto.mat_khau_moi);
            taiKhoan.da_doi_mat_khau = true;
            var result = await _repository.UpdateAsync(taiKhoan);
            return result ? (true, "Đổi mật khẩu thành công") : (false, "Lỗi khi đổi mật khẩu");
        }
        private string TaoMaTaiKhoan()
        {
            var lastKhachHang = _repositoryKhachHang.GetAllAsync().Result.OrderByDescending(x => x.ma_khach_hang).FirstOrDefault();
            if (lastKhachHang == null)
                return "KH00001";
            int startNumber = int.Parse(lastKhachHang.ma_khach_hang.Substring(2)) + 1;
            return $"KH{startNumber:D5}";
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
    }
}