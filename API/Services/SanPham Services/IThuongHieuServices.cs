using API.DbConects.DTO.SanPham_DTO;
using API.DbConects.Entities.Entities_San_Pham;
using API.DbConects.Entities.Entities_Tai_Khoan;
using API.Repositories;

namespace API.Services.SanPham_Services
{
    public interface IThuongHieuServices
    {
        Task<ICollection<ThuongHieu>> GetThuongHieuAsync();
        Task<ThuongHieu> GetThuongHieuByIdAsync(Guid id);
        Task<(bool,string)> Add(Them_ThuongHieuDTO thuongHieuDTO, string mataikhoantao);
        Task<(bool, string)> Update(Sua_ThuongHieuDTO thuongHieuDTO, string mataikhoansua);
        Task<bool> Delete(Guid id);
        Task<string> TaoMaThuongHieu();     
    }
    public class ThuongHieuServices : IThuongHieuServices
    {
        private readonly IBaseRepositories<ThuongHieu> _baseRepositories;
        private readonly IBaseRepositories<NhanVien> _nhanvienRepository;

        public ThuongHieuServices(IBaseRepositories<ThuongHieu> baseRepositories, IBaseRepositories<NhanVien> nhanvienRepository)
        {
            _baseRepositories = baseRepositories;
            _nhanvienRepository = nhanvienRepository;
        }

        public async Task<(bool, string)> Add(Them_ThuongHieuDTO thuongHieuDTO,string mataikhoantao)
        {
            Guid idNhanVienTao = _nhanvienRepository.GetAll().Result.FirstOrDefault(nv => nv.ma_nhan_vien == mataikhoantao).id_nhan_vien;
            ThuongHieu thuongHieu = new ThuongHieu()
            {
                id_thuong_hieu = Guid.NewGuid(),
                ma_thuong_hieu = TaoMaThuongHieu().Result,
                ten_thuong_hieu = thuongHieuDTO.ten_thuong_hieu,
                trang_thai = TrangThaiThuongHieuDTO.HoatDong.ToString(),
                id_nguoi_tao = idNhanVienTao,
                ngay_tao = DateTime.Now
            };
            var result = await _baseRepositories.Add(thuongHieu);
            if (result) return (true, "Thêm thương hiệu thành công");
            return (false, "Đã có lỗi khi thêm mới Thương hiệu!");
        }

        public Task<bool> Delete(Guid id)
        {
            throw new NotImplementedException();
        }

        public async Task<ICollection<ThuongHieu>> GetThuongHieuAsync()
        {
            var result = await _baseRepositories.GetAll();
            return result;
        }

        public Task<ThuongHieu> GetThuongHieuByIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public async Task<string> TaoMaThuongHieu()
        {
            const string prefix = "TH";
            const int codeLength = 6;
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();

            string newCode;
            bool isUnique;

            do
            {
                newCode = prefix + new string(Enumerable.Repeat(chars, codeLength)
                    .Select(s => s[random.Next(s.Length)]).ToArray());

                var existingCode = await _baseRepositories.GetAll();
                isUnique = !existingCode.Any(th => th.ma_thuong_hieu == newCode);

            } while (!isUnique);

            return newCode;
        }

        public Task<(bool, string)> Update(Sua_ThuongHieuDTO thuongHieu, string mataikhoansua)
        {
            throw new NotImplementedException();
        }
    }
}
