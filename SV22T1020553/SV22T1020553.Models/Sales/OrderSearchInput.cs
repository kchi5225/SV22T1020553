using SV22T1020553.Models.Common;

namespace SV22T1020553.Models.Sales
{
    /// <summary>
    /// Đầu vào tìm kiếm, phân trang đơn hàng
    /// </summary>
    public class OrderSearchInput : PaginationSearch
    {
        /// <summary>
        /// Trạng thái đơn hàng
        /// </summary>
        public OrderStatusEnum? Status { get; set; }
        /// <summary>
        /// Từ ngày (ngày lập đơn hàng)
        /// </summary>
        public DateTime? DateFrom { get; set; }
        /// <summary>
        /// Đến ngày (ngày lập đơn hàng)
        /// </summary>
        public DateTime? DateTo { get; set; }
    }
}
