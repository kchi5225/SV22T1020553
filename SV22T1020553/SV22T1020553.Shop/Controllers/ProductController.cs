using Microsoft.AspNetCore.Mvc;
using SV22T1020553.BusinessLayers;
using SV22T1020553.Models.Catalog;
using SV22T1020553.Shop.Models;

namespace SV22T1020553.Shop.Controllers
{
    public class ProductController : Controller
    {
        public async Task<IActionResult> Detail(int id)
        {
            var product = await CatalogDataService.GetProductAsync(id);
            if (product == null || !product.IsSelling)
                return RedirectToAction("Index", "Home");

            var attributes = await CatalogDataService.ListAttributesAsync(id) ?? new List<ProductAttribute>();
            var photos = (await CatalogDataService.ListPhotosAsync(id) ?? new List<ProductPhoto>())
                            .Where(p => !p.IsHidden)
                            .ToList();

            var model = new ProductDetailViewModel
            {
                Product = product,
                Attributes = attributes,
                Photos = photos,
            };

            return View(model);
        }
    }
}
