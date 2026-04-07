using SV22T1020553.Models.Sales;

namespace SV22T1020553.Shop.Models
{
    public class CartViewModel
    {
        public List<OrderDetailViewInfo> Items { get; set; } = new();
        public decimal TotalAmount => Items.Sum(item => item.Quantity * item.SalePrice);
        public int TotalQuantity => Items.Sum(item => item.Quantity);
    }
}
