using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV22T1020553.BusinessLayers
{
    /// <summary>
    /// lớp lưu giữ các thông tin cấu hình sử dụng trong Business Layer
    /// </summary>
    public static class Configuration
    {
        private static string _connectionString = "";

        /// <summary>
        /// hàm có chức năng khởi tạo cấu hình cho Business Layer
        /// Hàm này phải được gọi trước khi chạy ứng dụng
        /// </summary>
        /// <param name="connectionString"></param>
        public static void Initialize(string connectionString)
        {
            _connectionString = connectionString;
        }
        /// <summary>
        /// lấy chuỗi tham số kết nối đến cơ sở dữ liệu 
        /// nhờ hàm này lấy ra  (configuration.ConnectionString)
        /// </summary>

        public static string ConnectionString => _connectionString;
    }
}
