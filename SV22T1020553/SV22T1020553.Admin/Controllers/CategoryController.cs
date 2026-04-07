using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020553.BusinessLayers;
using SV22T1020553.Models.Catalog;
using SV22T1020553.Models.Common;

namespace SV22T1020553.Admin.Controllers
{
    [Authorize]
    public class CategoryController : Controller
    {
        private const string CATEGORY_SEARCH = "CategorySearchInput";

    public IActionResult Index()
        {
            var input = ApplicationContext.GetSessionData<PaginationSearch>(CATEGORY_SEARCH);
            if (input == null)
                input = new PaginationSearch()
                {
                    Page = 1,
                    PageSize = ApplicationContext.PageSize,
                    SearchValue = ""
                };

            return View(input);
        }

        public async Task<IActionResult> Search(PaginationSearch input)
        {
            var result = await CatalogDataService.ListCategoriesAsync(input);
            ApplicationContext.SetSessionData(CATEGORY_SEARCH, input);
            return View(result);
        }

        public IActionResult Create()
        {
            ViewBag.Title = "Bổ sung loại hàng";
            var model = new Category()
            {
                CategoryID = 0
            };
            return View("Edit", model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            ViewBag.Title = "Cập nhật loại hàng";
            var model = await CatalogDataService.GetCategoryAsync(id);

            if (model == null)
                return RedirectToAction("Index");

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SaveData(Category data)
        {
            ViewBag.Title = data.CategoryID == 0
                ? "Bổ sung loại hàng"
                : "Cập nhật loại hàng";

            try
            {
                if (string.IsNullOrWhiteSpace(data.CategoryName))
                    ModelState.AddModelError(nameof(data.CategoryName),
                        "Vui lòng nhập tên loại hàng");

                if (!ModelState.IsValid)
                    return View("Edit", data);

                if (data.CategoryID == 0)
                    await CatalogDataService.AddCategoryAsync(data);
                else
                    await CatalogDataService.UpdateCategoryAsync(data);

                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ModelState.AddModelError("Error", "Hệ thống đang bận vui lòng thử lại sau ");
                return View("Edit", data);
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            if (Request.Method == "POST")
            {
                await CatalogDataService.DeleteCategoryAsync(id);
                return RedirectToAction("Index");
            }

            var model = await CatalogDataService.GetCategoryAsync(id);

            if (model == null)
                return RedirectToAction("Index");

            ViewBag.CanDelete = !(await CatalogDataService.IsUsedCategoryAsync(id));

            return View(model);
        }
    }


}
