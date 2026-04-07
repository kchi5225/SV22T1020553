using SV22T1020553.Models; 
using SV22T1020553.Models.DataDictionary;
using SV22T1020553.Models.Sales;

namespace SV22T1020553.Shop.Models
{
    public class CheckoutViewModel
    {
        public string DeliveryProvince { get; set; } = string.Empty;
        public string DeliveryAddress { get; set; } = string.Empty;

        
        public string? DeliveryName { get; set; }
        public string? DeliveryPhone { get; set; }
        public string? Notes { get; set; }
       

        public List<Province>? Provinces { get; set; } 
        public List<OrderDetailViewInfo> CartItems { get; set; } = new List<OrderDetailViewInfo>();
    }
}