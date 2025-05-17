using API.DbConects.DTOs.Client.TaiKhoan;
using API.DbConects.Entities.Entities_Tai_Khoan;
using API.Repositories.Interfaces;
using API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Services.Implementations
{
    public class DiaChiService : BaseService<DiaChi>, IDiaChiService
    {
        private readonly IBaseRepository<DiaChi> _repository;

        public DiaChiService(IBaseRepository<DiaChi> repository) : base(repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<DiaChiDTO>> GetDiaChiByKhachHangAsync(Guid idKhachHang)
        {
            var diaChis = await _repository.GetByConditionAsync(d => d.id_khach_hang == idKhachHang);
            return diaChis.Select(d => new DiaChiDTO
            {
                id_dia_chi = d.id_dia_chi,
                id_khach_hang = d.id_khach_hang,
                tinh = d.tinh,
                huyen = d.huyen,
                xa = d.xa,
                dia_chi_cu_the = d.dia_chi_cu_the,
                so_dien_thoai = d.so_dien_thoai,
                ten_nguoi_nhan = d.ten_nguoi_nhan,
                dia_chi_mac_dinh = d.dia_chi_mac_dinh,
                ngay_tao = d.ngay_tao,
                ngay_sua = d.ngay_sua
            });
        }

        public async Task<DiaChiDTO> GetDiaChiMacDinhAsync(Guid idKhachHang)
        {
            var diaChi = await _repository.GetFirstOrDefaultAsync(d => d.id_khach_hang == idKhachHang && d.dia_chi_mac_dinh);
            if (diaChi == null) return null;

            return new DiaChiDTO
            {
                id_dia_chi = diaChi.id_dia_chi,
                id_khach_hang = diaChi.id_khach_hang,
                tinh = diaChi.tinh,
                huyen = diaChi.huyen,
                xa = diaChi.xa,
                dia_chi_cu_the = diaChi.dia_chi_cu_the,
                so_dien_thoai = diaChi.so_dien_thoai,
                ten_nguoi_nhan = diaChi.ten_nguoi_nhan,
                dia_chi_mac_dinh = diaChi.dia_chi_mac_dinh,
                ngay_tao = diaChi.ngay_tao,
                ngay_sua = diaChi.ngay_sua
            };
        }

        public async Task<(bool success, string message)> CreateDiaChiAsync(Guid idKhachHang, CreateDiaChiDTO createDto)
        {
            try
            {
                // Kiểm tra số lượng địa chỉ hiện tại của khách hàng
                var soLuongDiaChi = await _repository.CountAsync(d => d.id_khach_hang == idKhachHang);
                if (soLuongDiaChi >= 5)
                {
                    return (false, "Bạn đã đạt giới hạn tối đa 5 địa chỉ. Vui lòng xóa bớt địa chỉ cũ trước khi thêm địa chỉ mới.");
                }

                var diaChi = new DiaChi
                {
                    id_dia_chi = Guid.NewGuid(),
                    id_khach_hang = idKhachHang,
                    tinh = createDto.tinh,
                    huyen = createDto.huyen,
                    xa = createDto.xa,
                    dia_chi_cu_the = createDto.dia_chi_cu_the,
                    so_dien_thoai = createDto.so_dien_thoai,
                    ten_nguoi_nhan = createDto.ten_nguoi_nhan,
                    dia_chi_mac_dinh = createDto.dia_chi_mac_dinh,
                    ngay_tao = DateTime.Now.ToString("dd/MM/yyyy"),
                    ngay_sua = DateTime.Now.ToString("dd/MM/yyyy")
                };

                // Nếu là địa chỉ đầu tiên, đặt làm địa chỉ mặc định
                if (soLuongDiaChi == 0)
                {
                    diaChi.dia_chi_mac_dinh = true;
                }
                else if (diaChi.dia_chi_mac_dinh)
                {
                    // Nếu địa chỉ mới là mặc định, cập nhật các địa chỉ khác thành không mặc định
                    var diaChiMacDinhCu = await _repository.GetFirstOrDefaultAsync(d => d.id_khach_hang == idKhachHang && d.dia_chi_mac_dinh);
                    if (diaChiMacDinhCu != null)
                    {
                        diaChiMacDinhCu.dia_chi_mac_dinh = false;
                        await _repository.UpdateAsync(diaChiMacDinhCu);
                    }
                }

                var result = await _repository.CreateAsync(diaChi);
                return result
                    ? (true, "Thêm địa chỉ thành công")
                    : (false, "Không thể thêm địa chỉ");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi thêm địa chỉ: {ex.Message}");
            }
        }

        public async Task<(bool success, string message)> UpdateDiaChiAsync(Guid idDiaChi, Guid idKhachHang, UpdateDiaChiDTO updateDto)
        {
            try
            {
                var diaChi = await _repository.GetFirstOrDefaultAsync(d => d.id_dia_chi == idDiaChi && d.id_khach_hang == idKhachHang);
                if (diaChi == null)
                    return (false, "Không tìm thấy địa chỉ");

                if (updateDto.tinh != null) diaChi.tinh = updateDto.tinh;
                if (updateDto.huyen != null) diaChi.huyen = updateDto.huyen;
                if (updateDto.xa != null) diaChi.xa = updateDto.xa;
                if (updateDto.dia_chi_cu_the != null) diaChi.dia_chi_cu_the = updateDto.dia_chi_cu_the;
                if (updateDto.so_dien_thoai != null) diaChi.so_dien_thoai = updateDto.so_dien_thoai;
                if (updateDto.ten_nguoi_nhan != null) diaChi.ten_nguoi_nhan = updateDto.ten_nguoi_nhan;

                if (updateDto.dia_chi_mac_dinh.HasValue && updateDto.dia_chi_mac_dinh.Value && !diaChi.dia_chi_mac_dinh)
                {
                    // Nếu đang cập nhật thành địa chỉ mặc định
                    var diaChiMacDinhCu = await _repository.GetFirstOrDefaultAsync(d => d.id_khach_hang == idKhachHang && d.dia_chi_mac_dinh);
                    if (diaChiMacDinhCu != null)
                    {
                        diaChiMacDinhCu.dia_chi_mac_dinh = false;
                        await _repository.UpdateAsync(diaChiMacDinhCu);
                    }
                    diaChi.dia_chi_mac_dinh = true;
                }

                diaChi.ngay_sua = DateTime.Now.ToString("dd/MM/yyyy");

                var result = await _repository.UpdateAsync(diaChi);
                return result
                    ? (true, "Cập nhật địa chỉ thành công")
                    : (false, "Không thể cập nhật địa chỉ");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi cập nhật địa chỉ: {ex.Message}");
            }
        }

        public async Task<(bool success, string message)> DeleteDiaChiAsync(Guid idDiaChi, Guid idKhachHang)
        {
            try
            {
                var diaChi = await _repository.GetFirstOrDefaultAsync(d => d.id_dia_chi == idDiaChi && d.id_khach_hang == idKhachHang);
                if (diaChi == null)
                    return (false, "Không tìm thấy địa chỉ");

                if (diaChi.dia_chi_mac_dinh)
                {
                    // Nếu xóa địa chỉ mặc định, tìm địa chỉ khác để đặt làm mặc định
                    var diaChiKhac = await _repository.GetFirstOrDefaultAsync(d => d.id_khach_hang == idKhachHang && d.id_dia_chi != idDiaChi);
                    if (diaChiKhac != null)
                    {
                        diaChiKhac.dia_chi_mac_dinh = true;
                        await _repository.UpdateAsync(diaChiKhac);
                    }
                }

                var result = await _repository.DeleteAsync(idDiaChi);
                return result
                    ? (true, "Xóa địa chỉ thành công")
                    : (false, "Không thể xóa địa chỉ");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi xóa địa chỉ: {ex.Message}");
            }
        }

        public async Task<(bool success, string message)> SetDiaChiMacDinhAsync(Guid idDiaChi, Guid idKhachHang)
        {
            try
            {
                var diaChi = await _repository.GetFirstOrDefaultAsync(d => d.id_dia_chi == idDiaChi && d.id_khach_hang == idKhachHang);
                if (diaChi == null)
                    return (false, "Không tìm thấy địa chỉ");

                if (diaChi.dia_chi_mac_dinh)
                    return (true, "Địa chỉ này đã là địa chỉ mặc định");

                // Cập nhật địa chỉ mặc định cũ
                var diaChiMacDinhCu = await _repository.GetFirstOrDefaultAsync(d => d.id_khach_hang == idKhachHang && d.dia_chi_mac_dinh);
                if (diaChiMacDinhCu != null)
                {
                    diaChiMacDinhCu.dia_chi_mac_dinh = false;
                    await _repository.UpdateAsync(diaChiMacDinhCu);
                }

                // Đặt địa chỉ mới làm mặc định
                diaChi.dia_chi_mac_dinh = true;
                var result = await _repository.UpdateAsync(diaChi);
                return result
                    ? (true, "Đặt địa chỉ mặc định thành công")
                    : (false, "Không thể đặt địa chỉ mặc định");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi đặt địa chỉ mặc định: {ex.Message}");
            }
        }
    }
}