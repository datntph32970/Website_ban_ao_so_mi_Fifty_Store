using API.DbConects.DTO.SanPham_DTO;
using API.DbConects.Entities.Entities_San_Pham;
using API.DbConects.Entities.Entities_Tai_Khoan;
using API.Repositories;

namespace API.Services.SanPham_Services
{
    public interface ISanPhamService
    {
        Task<IEnumerable<SanPhamDTO>> GetAllAsync();
        Task<SanPhamDTO> GetByIdAsync(Guid id);
        Task AddAsync(SanPhamDTO sanPhamDto);
        Task UpdateAsync(Guid id, SanPhamDTO sanPhamDto);
        Task DeleteAsync(Guid id);
    }
    public class SanPhamService : ISanPhamService
    {
        private readonly IBaseRepositories<SanPham> _repository;
        private readonly IBaseRepositories<NhanVien> _repositoryNhanVien;

        public SanPhamService(IBaseRepositories<SanPham> repository, IBaseRepositories<NhanVien> repositoryNhanVien)
        {
            _repositoryNhanVien = repositoryNhanVien;
            _repository = repository;
        }

        public async Task<IEnumerable<SanPhamDTO>> GetAllAsync()
        {
            var sanPhams = await _repository.GetAll();
            return sanPhams.Select(sp => new SanPhamDTO
            {
                IdSanPham = sp.id_san_pham,
                MaSanPham = sp.ma_san_pham,
                TenSanPham = sp.ten_san_pham,
                MoTa = sp.mo_ta,
                TrangThai = sp.trang_thai,
                IdChatLieu = sp.id_chat_lieu,
                IdKieuDang = sp.id_kieu_dang,
                IdThuongHieu = sp.id_thuong_hieu,
                IdXuatXu = sp.id_xuat_xu,
                IdNguoiTao = sp.id_nguoi_tao,
                NgayTao = sp.ngay_tao
            });
        }

        public async Task<SanPhamDTO> GetByIdAsync(Guid id)
        {
            var sp = await _repository.GetById(id);
            if (sp == null) return null;

            return new SanPhamDTO
            {
                IdSanPham = sp.id_san_pham,
                MaSanPham = sp.ma_san_pham,
                TenSanPham = sp.ten_san_pham,
                MoTa = sp.mo_ta,
                TrangThai = sp.trang_thai,
                IdChatLieu = sp.id_chat_lieu,
                IdKieuDang = sp.id_kieu_dang,
                IdThuongHieu = sp.id_thuong_hieu,
                IdXuatXu = sp.id_xuat_xu,
                IdNguoiTao = sp.id_nguoi_tao,
                NgayTao = sp.ngay_tao

            };
        }

        public async Task AddAsync(SanPhamDTO sanPhamDto)
        {


            var nhanVienExists = await _repositoryNhanVien.GetById(sanPhamDto.IdNguoiTao);
            if (nhanVienExists == null)
            {
                throw new Exception("Nhân viên tạo sản phẩm không tồn tại!");
            }

            var sanPham = new SanPham
            {
                id_san_pham = Guid.NewGuid(),
                ma_san_pham = sanPhamDto.MaSanPham,
                ten_san_pham = sanPhamDto.TenSanPham,
                mo_ta = sanPhamDto.MoTa,
                trang_thai = sanPhamDto.TrangThai,
                id_chat_lieu = sanPhamDto.IdChatLieu,
                id_kieu_dang = sanPhamDto.IdKieuDang,
                id_thuong_hieu = sanPhamDto.IdThuongHieu,
                id_xuat_xu = sanPhamDto.IdXuatXu,
                id_nguoi_tao = sanPhamDto.IdNguoiTao,
                ngay_tao = DateTime.UtcNow
            };

            await _repository.Add(sanPham);
        }

        public async Task UpdateAsync(Guid id, SanPhamDTO sanPhamDto)
        {
            var sanPham = await _repository.GetById(id);

            if (sanPham == null) {
                throw new KeyNotFoundException($"Không tìm thấy sản phẩm với ID: {id}");
            };
           
            sanPham.ma_san_pham = sanPhamDto.MaSanPham;
            sanPham.ten_san_pham = sanPhamDto.TenSanPham;
            sanPham.mo_ta = sanPhamDto.MoTa;
            sanPham.trang_thai = sanPhamDto.TrangThai;
            sanPham.id_chat_lieu = sanPhamDto.IdChatLieu;
            sanPham.id_kieu_dang = sanPhamDto.IdKieuDang;
            sanPham.id_thuong_hieu = sanPhamDto.IdThuongHieu;
            sanPham.id_xuat_xu = sanPhamDto.IdXuatXu;
            sanPham.id_nguoi_tao = sanPhamDto.IdNguoiTao;
           

            await _repository.Update(sanPham);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _repository.Delete(id);
        }
    }
}
