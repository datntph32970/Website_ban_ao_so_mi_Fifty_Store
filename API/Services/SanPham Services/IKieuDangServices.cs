using API.DbConects.DTO.SanPham_DTO;
using API.DbConects.Entities.Entities_San_Pham;
using API.DbConects.Entities.Entities_Tai_Khoan;
using API.Repositories;

public interface IKieuDangService
{
    Task<ICollection<KieuDang>> GetKieuDangAsync();
    Task<KieuDang> GetKieuDangByIdAsync(Guid id);
    Task<(bool, string)> Add(ThemKieuDangDTO kieuDangDTO, string mataikhoantao);
    Task<(bool, string)> Update(SuaKieuDangDTO kieuDangDTO, string mataikhoansua);
    Task<bool> Delete(Guid id);
    Task<string> TaoMaKieuDang();
}

public class KieuDangService : IKieuDangService
{
    private readonly IBaseRepositories<KieuDang> _baseRepositories;
    private readonly IBaseRepositories<NhanVien> _nhanvienRepository;

    public KieuDangService(IBaseRepositories<KieuDang> baseRepositories, IBaseRepositories<NhanVien> nhanvienRepository)
    {
        _baseRepositories = baseRepositories;
        _nhanvienRepository = nhanvienRepository;
    }

    public async Task<(bool, string)> Add(ThemKieuDangDTO kieuDangDTO, string mataikhoantao)
    {
        var nhanVien = _nhanvienRepository.GetAll().Result.FirstOrDefault(nv => nv.ma_nhan_vien == mataikhoantao);
        if (nhanVien == null) return (false, "Người tạo không hợp lệ!");

        KieuDang kieuDang = new KieuDang()
        {
            id_kieu_dang = Guid.NewGuid(),
            ma_kieu_dang = await TaoMaKieuDang(),
            ten_kieu_dang = kieuDangDTO.TenKieuDang,
            trang_thai = TrangThaiKieuDangDTO.HoatDong.ToString(),
            id_nguoi_tao = nhanVien.id_nhan_vien,
            ngay_tao = DateTime.Now
        };

        var result = await _baseRepositories.Add(kieuDang);
        return result ? (true, "Thêm kiểu dáng thành công!") : (false, "Lỗi khi thêm kiểu dáng.");
    }

    public async Task<ICollection<KieuDang>> GetKieuDangAsync() => await _baseRepositories.GetAll();
    public Task<KieuDang> GetKieuDangByIdAsync(Guid id) => throw new NotImplementedException();
    public Task<bool> Delete(Guid id) => throw new NotImplementedException();
    public Task<(bool, string)> Update(SuaKieuDangDTO kieuDangDTO, string mataikhoansua) => throw new NotImplementedException();

    public async Task<string> TaoMaKieuDang()
    {
        const string prefix = "KD";
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
            isUnique = !existingCodes.Any(kd => kd.ma_kieu_dang == newCode);

        } while (!isUnique);

        return newCode;
    }
}
