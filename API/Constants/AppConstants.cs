namespace API.Constants
{
    public static class AppConstants
    {
        public static class Roles
        {
            public const string Admin = "Admin";
            public const string User = "User";
            public const string NhanVien = "NhanVien";
        }

        public static class ErrorMessages
        {
            public const string InvalidCredentials = "Tên đăng nhập hoặc mật khẩu không đúng";
            public const string UserNotFound = "Không tìm thấy người dùng";
            public const string UserAlreadyExists = "Người dùng đã tồn tại";
            public const string InvalidToken = "Token không hợp lệ";
            public const string TokenExpired = "Token đã hết hạn";
        }

        public static class SuccessMessages
        {
            public const string LoginSuccess = "Đăng nhập thành công";
            public const string RegisterSuccess = "Đăng ký thành công";
            public const string UpdateSuccess = "Cập nhật thành công";
            public const string DeleteSuccess = "Xóa thành công";
        }

        public static class CacheKeys
        {
            public const string UserProfile = "UserProfile_{0}";
            public const string ProductList = "ProductList";
            public const string CategoryList = "CategoryList";
        }
    }
}