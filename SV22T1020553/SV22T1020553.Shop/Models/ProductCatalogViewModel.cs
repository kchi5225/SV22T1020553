using SV22T1020553.Models.Catalog;
using SV22T1020553.Models.Common;

namespace SV22T1020553.Shop.Models
{
    public class ProductCatalogViewModel
    {
        public ProductSearchInput SearchInput { get; set; } = new();
        public PagedResult<Product> Products { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
    }
}
