using API.DbConects.DTO.Tai_Khoan_DTO;
using API.DbConects.Entities.Entities_Hoa_Don;
using API.DbConects.Entities.Entities_Tai_Khoan;
using API.Repositories;
using API.Services.JwtServices;
using System.Security.Cryptography;
using System.Text;

namespace API.Services.TaiKhoan_Services
{
    public interface ITaiKhoanServices
    {
        (bool, string) DangKyTaiKhoan(string username,string paswword);
        (bool, string) DangNhapTaiKhoan(string username, string paswword);
        string BamMatKhau(string matKhau);
        //
        public string TaoMaTaiKhoan();
        public TaiKhoan GetTaiKhoan(string tenDangNhap, string matKhau);
        public TaiKhoan GetTaiKhoanById(Guid id);
        public TaiKhoan GetTaiKhoanByMaTaiKhoan(string maTaiKhoan);
        public (bool, string) AddTaiKhoan(TaiKhoan taiKhoan);
        public (bool, string) UpdateTaiKhoan(TaiKhoan taiKhoan);
        public (bool, string) DeleteTaiKhoan(Guid id);
        public (bool, string) ChangePassword(Guid id, string matKhauCu, string matKhauMoi);
        public (bool, string) CapNhatTrangThai(Guid id, string trangThai);
    }
    public class TaiKhoanServices : ITaiKhoanServices
    {
        private readonly IBaseRepositories<TaiKhoan> _taiKhoanRepositories;
        private readonly IBaseRepositories<KhachHang> _khachHangRepositories;
        private readonly IBaseRepositories<NhanVien> _nhanVienRepositories;
        private readonly IJwtServices _jwtServices;

        public TaiKhoanServices(IBaseRepositories<TaiKhoan> taiKhoanRepositories, IBaseRepositories<KhachHang> khachHangRepositories, IBaseRepositories<NhanVien> nhanVienRepositories, IJwtServices jwtServices)
        {
            _taiKhoanRepositories = taiKhoanRepositories;
            _khachHangRepositories = khachHangRepositories;
            _nhanVienRepositories = nhanVienRepositories;
            _jwtServices = jwtServices;
        }

        public (bool, string) AddTaiKhoan(TaiKhoan taiKhoan)
        {
            throw new NotImplementedException();
        }

        public string BamMatKhau(string matKhau)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(matKhau));
                var sb = new StringBuilder();
                foreach (var b in hashedBytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }

        public (bool, string) CapNhatTrangThai(Guid id, string trangThai)
        {
            throw new NotImplementedException();
        }

        public (bool, string) ChangePassword(Guid id, string matKhauCu, string matKhauMoi)
        {
            throw new NotImplementedException();
        }

        public (bool, string) DangKyTaiKhoan(string username, string paswword)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(paswword))
            {
                return (false, "Tên đăng nhập hoặc mật khẩu không được để trống");
            }
            var taiKhoan = _taiKhoanRepositories.GetAll().Result.FirstOrDefault(x => x.ten_dang_nhap == username);
            if (taiKhoan != null)
            {
                return (false, "Tên đăng nhập đã tồn tại");
            }
            var newTaiKhoan = new TaiKhoan
            {
                id_tai_khoan = Guid.NewGuid(),
                ma_tai_khoan = TaoMaTaiKhoan(),
                ten_dang_nhap = username,
                mat_khau = BamMatKhau(paswword),
                trang_thai = TrangThaiTaiKhoan.HoatDong.ToString(), 
                chuc_vu = ChucVuTaiKhoan.KhachHang.ToString()
            };
            var newKhachHang = new KhachHang
            {
                id_khach_hang = Guid.NewGuid(),
                id_tai_khoan = newTaiKhoan.id_tai_khoan,
                ma_khach_hang = newTaiKhoan.ma_tai_khoan,
                ten_khach_hang = username,
                ngay_sinh = DateTime.Now,
                so_dien_thoai = "",
                email = "",
                gioi_tinh = "",
                trang_thai = TrangThaiTaiKhoan.HoatDong.ToString()
            };
            _taiKhoanRepositories.Add(newTaiKhoan);
            _khachHangRepositories.Add(newKhachHang);
            var token = _jwtServices.GenerateJwtToken(newTaiKhoan.id_tai_khoan,newTaiKhoan.ten_dang_nhap,ChucVuTaiKhoan.KhachHang.ToString(),newTaiKhoan.ma_tai_khoan);
            return (true, token);
        }

        public (bool, string) DangNhapTaiKhoan(string username, string paswword)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(paswword))
            {
                return (false, "Tên đăng nhập hoặc mật khẩu không được để trống");
            }
            var taiKhoan = _taiKhoanRepositories.GetAll().Result.FirstOrDefault(x => x.ten_dang_nhap == username);
            if (taiKhoan == null)
            {
                return (false, "Tên đăng nhập không tồn tại");
            }
            if (taiKhoan.mat_khau != BamMatKhau(paswword))
            {
                return (false, "Mật khẩu không đúng");
            }
            var token = _jwtServices.GenerateJwtToken(taiKhoan.id_tai_khoan,taiKhoan.ten_dang_nhap, taiKhoan.chuc_vu,taiKhoan.ma_tai_khoan);
            return (true, token);

        }

        public (bool, string) DeleteTaiKhoan(Guid id)
        {
            throw new NotImplementedException();
        }

        public TaiKhoan GetTaiKhoan(string tenDangNhap, string matKhau)
        {
            throw new NotImplementedException();
        }

        public TaiKhoan GetTaiKhoanById(Guid id)
        {
            throw new NotImplementedException();
        }

        public TaiKhoan GetTaiKhoanByMaTaiKhoan(string maTaiKhoan)
        {
            throw new NotImplementedException();
        }

        public string TaoMaTaiKhoan()
        {
            var lastTaiKhoan = _taiKhoanRepositories.GetAll().Result.OrderByDescending(t => t.ma_tai_khoan).FirstOrDefault();
            int nextNumber = 1;
            if (lastTaiKhoan != null)
            {
                string lastNumberStr = lastTaiKhoan.ma_tai_khoan.Substring(2, lastTaiKhoan.ma_tai_khoan.Length - 10);
                if (int.TryParse(lastNumberStr, out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }
            string datePart = DateTime.Now.ToString("ddMMyyyy"); 
            return $"TK{nextNumber}{datePart}";
        }

        public (bool, string) UpdateTaiKhoan(TaiKhoan taiKhoan)
        {
            throw new NotImplementedException();
        }
    }
}
