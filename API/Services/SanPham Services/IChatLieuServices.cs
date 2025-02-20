using API.DbConects.DTO.SanPham_DTO;
using API.DbConects.Entities.Entities_San_Pham;
using API.DbConects.Entities.Entities_Tai_Khoan;
using API.Repositories;

namespace API.Services.SanPham_Services
{
    public interface IChatLieuService
    {
        Task<ICollection<ChatLieu>> GetChatLieuAsync();
        Task<ChatLieu> GetChatLieuByIdAsync(Guid id);
        Task<(bool, string)> Add(ThemChatLieuDTO chatLieuDTO, string mataikhoantao);
        Task<(bool, string)> Update(SuaChatLieuDTO chatLieuDTO, string mataikhoansua);
        Task<bool> Delete(Guid id);
        Task<string> TaoMaChatLieu();
    }

    public class ChatLieuService : IChatLieuService
    {
        private readonly IBaseRepositories<ChatLieu> _baseRepositories;
        private readonly IBaseRepositories<NhanVien> _nhanvienRepository;

        public ChatLieuService(IBaseRepositories<ChatLieu> baseRepositories, IBaseRepositories<NhanVien> nhanvienRepository)
        {
            _baseRepositories = baseRepositories;
            _nhanvienRepository = nhanvienRepository;
        }

        public async Task<(bool, string)> Add(ThemChatLieuDTO chatLieuDTO, string mataikhoantao)
        {
            var nhanVien = _nhanvienRepository.GetAll().Result.FirstOrDefault(nv => nv.ma_nhan_vien == mataikhoantao);
            if (nhanVien == null) return (false, "Người tạo không hợp lệ!");

            ChatLieu chatLieu = new ChatLieu()
            {
                id_chat_lieu = Guid.NewGuid(),
                ma_chat_lieu = await TaoMaChatLieu(),
                ten_chat_lieu = chatLieuDTO.TenChatLieu,
                trang_thai = TrangThaiChatLieuDTO.HoatDong.ToString(),
                id_nguoi_tao = nhanVien.id_nhan_vien,
                ngay_tao = DateTime.Now
            };

            var result = await _baseRepositories.Add(chatLieu);
            return result ? (true, "Thêm chất liệu thành công!") : (false, "Lỗi khi thêm chất liệu.");
        }

        public async Task<ICollection<ChatLieu>> GetChatLieuAsync() => await _baseRepositories.GetAll();
        public Task<ChatLieu> GetChatLieuByIdAsync(Guid id) => throw new NotImplementedException();
        public Task<bool> Delete(Guid id) => throw new NotImplementedException();
        public Task<(bool, string)> Update(SuaChatLieuDTO chatLieuDTO, string mataikhoansua) => throw new NotImplementedException();

        public async Task<string> TaoMaChatLieu()
        {
            const string prefix = "CL";
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
                isUnique = !existingCodes.Any(cl => cl.ma_chat_lieu == newCode);

            } while (!isUnique);

            return newCode;
        }
    }
}
