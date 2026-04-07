using SV22T1020553.Models.Catalog;

namespace SV22T1020553.Shop.Models
{
    public class ProductDetailViewModel
    {
        public Product Product { get; set; } = new();
        public List<ProductAttribute> Attributes { get; set; } = new();
        public List<ProductPhoto> Photos { get; set; } = new();
        public int Quantity { get; set; } = 1;
        public string? CategoryName { get; set; }
        public string? SupplierName { get; set; }
    }

}

