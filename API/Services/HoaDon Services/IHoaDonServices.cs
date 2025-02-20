
using API.DbConects.DTO.HoaDonDTO;
using API.DbConects.Entities.Entities_Hoa_Don;
using API.DbConects.Entities.Entities_Tai_Khoan;
using API.Repositories;

namespace API.Services.HoaDon_Services
{
    public interface IHoaDonServices
    {
        Task<ICollection<HoaDon>> GetHoaDonAsync();
        Task<HoaDon> GetHoaDonByIdAsync(Guid id);
        Task<(bool, string)> Add(Them_HoaDonDTO hoaDonDTO, string mataikhoantao);
        Task<(bool, string)> Update(Sua_HoaDonDTO hoaDonDTO, string mataikhoansua);
        Task<bool> Delete(Guid id);
        Task<string> TaoMaHoaDon();
    }

    public class HoaDonServices : IHoaDonServices
    {
        private readonly IBaseRepositories<HoaDon> _hoaDonRepository;
        private readonly IBaseRepositories<NhanVien> _nhanvienRepository;

        public HoaDonServices(IBaseRepositories<HoaDon> hoaDonRepository, IBaseRepositories<NhanVien> nhanvienRepository)
        {
            _hoaDonRepository = hoaDonRepository;
            _nhanvienRepository = nhanvienRepository;
        }

        public async Task<(bool, string)> Add(Them_HoaDonDTO hoaDonDTO, string mataikhoantao)
        {
            var nhanVien = await _nhanvienRepository.GetAll();
            var idNhanVienTao = nhanVien.FirstOrDefault(nv => nv.ma_nhan_vien == mataikhoantao)?.id_nhan_vien;
            if (idNhanVienTao == null) return (false, "Nhân viên tạo không hợp lệ");

            HoaDon hoaDon = new HoaDon()
            {
                id_hoa_don = Guid.NewGuid(),
                ma_hoa_don = await TaoMaHoaDon(),
                ngay_tao = DateTime.Now,
                id_nhan_vien = idNhanVienTao.Value,
                tong_tien_phai_thanh_toan = hoaDonDTO.TongTienPhaiThanhToan,
                id_trang_thai_hoa_don = hoaDonDTO.ID_TrangThaiHoaDon
            };

            var result = await _hoaDonRepository.Add(hoaDon);
            return result ? (true, "Thêm hóa đơn thành công") : (false, "Lỗi khi thêm hóa đơn");
        }

        public async Task<ICollection<HoaDon>> GetHoaDonAsync()
        {
            return await _hoaDonRepository.GetAll();
        }

        public async Task<HoaDon> GetHoaDonByIdAsync(Guid id)
        {
            var hoaDon = await _hoaDonRepository.GetById(id);
            return hoaDon ?? throw new KeyNotFoundException("Không tìm thấy hóa đơn");
        }

        public async Task<(bool, string)> Update(Sua_HoaDonDTO hoaDonDTO, string mataikhoansua)
        {
            var hoaDon = await _hoaDonRepository.GetById(hoaDonDTO.ID_KhachHang);
            if (hoaDon == null) return (false, "Hóa đơn không tồn tại");

            hoaDon.tong_tien_don_hang = hoaDonDTO.TongTienPhaiThanhToan;
            if (Guid.TryParse(hoaDonDTO.TrangThaiHoaDon.ToString(), out Guid trangThaiId))
            {
                hoaDon.id_trang_thai_hoa_don = trangThaiId;
            }
            else
            {
                return (false, "Trạng thái hóa đơn không hợp lệ");
            }


            hoaDon.ngay_tao = DateTime.Now;

            var result = await _hoaDonRepository.Update(hoaDon);
            return result ? (true, "Cập nhật hóa đơn thành công") : (false, "Lỗi khi cập nhật hóa đơn");
        }

        public async Task<bool> Delete(Guid id)
        {
            var hoaDon = await _hoaDonRepository.GetById(id);
            if (hoaDon == null) return false;
            return await _hoaDonRepository.Delete(hoaDon.id_hoa_don);
        }

        public async Task<string> TaoMaHoaDon()
        {
            const string prefix = "HD";
            const int codeLength = 6;
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();

            string newCode;
            bool isUnique;

            do
            {
                newCode = prefix + new string(Enumerable.Repeat(chars, codeLength)
                    .Select(s => s[random.Next(s.Length)]).ToArray());

                var existingCode = await _hoaDonRepository.GetAll();
                isUnique = !existingCode.Any(hd => hd.ma_hoa_don == newCode);

            } while (!isUnique);

            return newCode;
        }
    }
}