using Microsoft.Data.SqlClient;
using SV22T1020553.DataLayers.Interfaces;
using SV22T1020553.Models.Security;

namespace SV22T1020553.DataLayers.SQLServer
{
    /// <summary>
    /// Cài đặt các phép xử lý dữ liệu liên quan đến tài khoản nhân viên
    /// </summary>
    public class EmployeeAccountRepository : BaseRepository, IUserAccountRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public EmployeeAccountRepository(string connectionString) : base(connectionString)
        {
        }

        /// <summary>
        /// Kiểm tra đăng nhập của nhân viên
        /// </summary>
        public async Task<UserAccount?> AuthenticateAsync(string userName, string password)
        {
            UserAccount? account = null;

            using var connection = GetConnection();
            await connection.OpenAsync();

            // SỬA LỖI 1: Thêm cột RoleNames (hoặc Roles tùy vào thiết kế DB của bạn) vào câu lệnh SELECT
            // Lưu ý: Hãy đảm bảo cột lưu quyền trong SQL Server của bạn tên là RoleNames (hoặc sửa lại cho đúng tên cột trong bảng Employees của bạn nhé)
            var sql = @"SELECT EmployeeID, FullName, Email, RoleNames
                FROM Employees
                WHERE Email = @Email AND Password = @Password";

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Email", userName);
            command.Parameters.AddWithValue("@Password", password);

            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                account = new UserAccount()
                {
                    UserId = reader["EmployeeID"].ToString() ?? "",
                    UserName = reader["Email"].ToString() ?? "",
                    DisplayName = reader["FullName"].ToString() ?? "",
                    Email = reader["Email"].ToString() ?? "",
                    Photo = "",
                    // SỬA LỖI 2: Xóa dòng gán cứng "admin,datamanager,sales"
                    // Thay bằng cách đọc quyền từ Database lên
                    RoleNames = reader["RoleNames"].ToString() ?? ""
                };
            }

            return account;
        }

        /// <summary>
        /// Đổi mật khẩu của nhân viên
        /// </summary>
        public async Task<bool> ChangePasswordAsync(string userName, string password)
        {
            using var connection = GetConnection();
            await connection.OpenAsync();

            var sql = @"UPDATE Employees
                        SET Password = @Password
                        WHERE Email = @Email";

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Password", password);
            command.Parameters.AddWithValue("@Email", userName);

            int rows = await command.ExecuteNonQueryAsync();
            return rows > 0;
        }

        /// <summary>
        /// (Chưa sử dụng cho Nhân viên) Kiểm tra email
        /// </summary>
        public async Task<bool> CheckEmailAsync(string email)
        {
            // Tạm thời trả về false vì nhân viên không dùng tính năng này
            return await Task.FromResult(false);
        }

        /// <summary>
        /// (Chưa sử dụng cho Nhân viên) Đăng ký tài khoản
        /// </summary>
        public async Task<int> RegisterAsync(string displayName, string email, string password)
        {
            // Tạm thời trả về 0 vì nhân viên không tự đăng ký
            return await Task.FromResult(0);
        }
    }
}