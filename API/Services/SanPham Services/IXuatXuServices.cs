using API.DbConects.DTO.SanPham_DTO;
using API.DbConects.Entities.Entities_San_Pham;
using API.DbConects.Entities.Entities_Tai_Khoan;
using API.Repositories;

namespace API.Services.SanPham_Services
{
    public interface IXuatXuService
    {
        Task<ICollection<XuatXu>> GetXuatXuAsync();
        Task<XuatXu> GetXuatXuByIdAsync(Guid id);
        Task<(bool, string)> Add(ThemXuatXuDTO xuatXuDTO, string mataikhoantao);
        Task<(bool, string)> Update(SuaXuatXuDTO xuatXuDTO, string mataikhoansua);
        Task<bool> Delete(Guid id);
        Task<string> TaoMaXuatXu();
    }

    public class XuatXuService : IXuatXuService
    {
        private readonly IBaseRepositories<XuatXu> _baseRepositories;
        private readonly IBaseRepositories<NhanVien> _nhanvienRepository;

        public XuatXuService(IBaseRepositories<XuatXu> baseRepositories, IBaseRepositories<NhanVien> nhanvienRepository)
        {
            _baseRepositories = baseRepositories;
            _nhanvienRepository = nhanvienRepository;
        }

        public async Task<(bool, string)> Add(ThemXuatXuDTO xuatXuDTO, string mataikhoantao)
        {
            var nhanVien = _nhanvienRepository.GetAll().Result.FirstOrDefault(nv => nv.ma_nhan_vien == mataikhoantao);
            if (nhanVien == null) return (false, "Người tạo không hợp lệ!");

            XuatXu xuatXu = new XuatXu()
            {
                id_xuat_xu = Guid.NewGuid(),
                ma_xuat_xu = await TaoMaXuatXu(),
                ten_xuat_xu = xuatXuDTO.TenXuatXu,
                trang_thai = TrangThaiXuatXuDTO.HoatDong.ToString(),
                id_nguoi_tao = nhanVien.id_nhan_vien,
                ngay_tao = DateTime.Now
            };

            var result = await _baseRepositories.Add(xuatXu);
            return result ? (true, "Thêm xuất xứ thành công!") : (false, "Lỗi khi thêm xuất xứ.");
        }

        public async Task<ICollection<XuatXu>> GetXuatXuAsync() => await _baseRepositories.GetAll();

        public Task<XuatXu> GetXuatXuByIdAsync(Guid id) => throw new NotImplementedException();

        public Task<bool> Delete(Guid id) => throw new NotImplementedException();

        public Task<(bool, string)> Update(SuaXuatXuDTO xuatXuDTO, string mataikhoansua) => throw new NotImplementedException();

        public async Task<string> TaoMaXuatXu()
        {
            const string prefix = "XX";
            return await GenerateUniqueCode(prefix);
        }

        private async Task<string> GenerateUniqueCode(string prefix)
        {
            const int codeLength = 6;
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            string newCode;
            bool isUnique;

            do
            {
                newCode = prefix + new string(Enumerable.Repeat(chars, codeLength)
                    .Select(s => s[random.Next(s.Length)]).ToArray());

                var existingCodes = await _baseRepositories.GetAll();
                isUnique = !existingCodes.Any(xx => xx.ma_xuat_xu == newCode);

            } while (!isUnique);

            return newCode;
        }
    }
}
