using Microsoft.AspNetCore.Mvc;
using SV22T1020553.BusinessLayers;
using SV22T1020553.Models.Catalog;
using SV22T1020553.Models.Common;
using SV22T1020553.Shop.Models;
using System.Diagnostics;

namespace SV22T1020553.Shop.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Index(ProductSearchInput input)
        {
            input.PageSize = input.PageSize <= 0 ? 12 : input.PageSize;
            input.OnlySelling = true;

            var model = new ProductCatalogViewModel
            {
                SearchInput = input,
                Products = await CatalogDataService.ListProductsAsync(input),
                Categories = (await CatalogDataService.ListCategoriesAsync(new PaginationSearch
                {
                    Page = 1,
                    PageSize = 0,
                    SearchValue = ""
                })).DataItems
            };

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
