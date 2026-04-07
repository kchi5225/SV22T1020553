using Microsoft.Data.SqlClient;
using SV22T1020553.Admin;
using SV22T1020553.DataLayers.Interfaces;
using SV22T1020553.Models.DataDictionary;
using SV22T1020553.Models.Security;

namespace SV22T1020553.DataLayers.SQLServer
{
    /// <summary>
    /// Cài đặt các phép xử lý dữ liệu liên quan đến tài khoản khách hàng
    /// </summary>
    public class CustomerAccountRepository : BaseRepository, IUserAccountRepository
    {
        /// <summary>
        /// Ctor
        /// </summary>
        public CustomerAccountRepository(string connectionString) : base(connectionString)
        {
        }

        /// <summary>
        /// Kiểm tra đăng nhập
        /// </summary>
        public async Task<UserAccount?> AuthenticateAsync(string userName, string password)
        {
            UserAccount? account = null;

            using var connection = GetConnection();
            await connection.OpenAsync();

            var sql = @"SELECT CustomerID, CustomerName, Email, Password
                FROM Customers
                WHERE Email = @Email
                  AND (IsLocked = 0 OR IsLocked IS NULL)";

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Email", userName);

            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                string dbPassword = reader["Password"].ToString() ?? "";

                // ✔️ BẮT BUỘC phải hash
                string hashedInput = CryptHelper.HashMD5(password.Trim());

                // ✔️ so sánh hash
                if (hashedInput != dbPassword)
                    return null;

                account = new UserAccount()
                {
                    UserId = reader["CustomerID"].ToString() ?? "",
                    UserName = reader["Email"].ToString() ?? "",
                    DisplayName = reader["CustomerName"].ToString() ?? "",
                    Email = reader["Email"].ToString() ?? "",
                    Photo = "",
                    RoleNames = "Customer"
                };
            }

            return account;
        }

        /// <summary>
        /// Đổi mật khẩu
        /// </summary>
        public async Task<bool> ChangePasswordAsync(string userName, string password)
        {
            using var connection = GetConnection();
            await connection.OpenAsync();

            var sql = @"UPDATE Customers
                        SET Password = @Password
                        WHERE Email = @Email";

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Password", CryptHelper.HashMD5(password));
            command.Parameters.AddWithValue("@Email", userName);

            int rows = await command.ExecuteNonQueryAsync();
            return rows > 0;
        }
        /// <summary>
        /// Kiểm tra xem email đã tồn tại trong hệ thống chưa
        /// </summary>
        public async Task<bool> CheckEmailAsync(string email)
        {
            using var connection = GetConnection();
            await connection.OpenAsync();

            var sql = @"SELECT COUNT(*) FROM Customers WHERE Email = @Email";

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Email", email);

            // Thực thi và lấy về giá trị đầu tiên (số lượng dòng)
            int count = Convert.ToInt32(await command.ExecuteScalarAsync());

            return count > 0;
        }

        /// <summary>
        /// Đăng ký tài khoản khách hàng mới
        /// </summary>
        public async Task<int> RegisterAsync(string displayName, string email, string password)
        {
            using var connection = GetConnection();
            await connection.OpenAsync();

            // Lưu ý: Các trường như Province, Address, Phone nếu trong CSDL yêu cầu NOT NULL thì cần truyền chuỗi rỗng.
            // Mình đã thêm ContactName, Province, Address, Phone với giá trị mặc định là rỗng để tránh lỗi SQL.
            var sql = @"
                INSERT INTO Customers (CustomerName, ContactName, Province, Address, Phone, Email, Password, IsLocked)
                VALUES (@CustomerName, @ContactName, @Province, @Address, @Phone, @Email, @Password, 0);

                SELECT CAST(SCOPE_IDENTITY() as int);
            ";

            using var command = new SqlCommand(sql, connection);

            // Map tham số
            command.Parameters.AddWithValue("@CustomerName", displayName);
            command.Parameters.AddWithValue("@ContactName", displayName); // Thường để giống CustomerName khi mới đăng ký
            command.Parameters.AddWithValue("@Province", DBNull.Value);
            //command.Parameters.AddWithValue("@Province", string.IsNullOrEmpty(province) ? "" : province);
            command.Parameters.AddWithValue("@Address", "");
            command.Parameters.AddWithValue("@Phone", "");
            command.Parameters.AddWithValue("@Email", email);

            // ✔️ BẮT BUỘC mã hóa MD5 trước khi lưu giống như cách bạn làm ở hàm ChangePasswordAsync
            command.Parameters.AddWithValue("@Password", CryptHelper.HashMD5(password.Trim()));

            // Thực thi và lấy ID của dòng vừa thêm
            var result = await command.ExecuteScalarAsync();

            if (result != null && int.TryParse(result.ToString(), out int newId))
            {
                return newId;
            }

            return 0;
        }
    }

}