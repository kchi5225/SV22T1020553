using Microsoft.AspNetCore.Mvc;
using SV22T1020553.Models.Common;
using SV22T1020553.Models.Partner;
using SV22T1020553.BusinessLayers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace SV22T1020553.Admin.Controllers
{
    [Authorize]
    public class SupplierController : Controller
    {
        //private const int PAGESIZE = 10;
        private const string SUPPLIER_SEARCH_INPUT = "SupplierSearchInput";

        /// <summary>
        /// Hiển thị danh sách nhà cung cấp
        /// </summary>
        public IActionResult Index()
        {
            var input = ApplicationContext.GetSessionData<PaginationSearch>(SUPPLIER_SEARCH_INPUT);
            if (input == null)
            {
                input = new PaginationSearch()
                {
                    Page = 1,
                    PageSize = ApplicationContext.PageSize,
                    SearchValue = ""
                };
            }
            return View(input);
        }

        /// <summary>
        /// Tìm kiếm nhà cung cấp
        /// </summary>
        public async Task<IActionResult> Search(PaginationSearch input)
        {
            var result = await PartnerDataService.ListSuppliersAsync(input);

            ApplicationContext.SetSessionData(SUPPLIER_SEARCH_INPUT, input);

            return View(result); 
        }

        /// <summary>
        /// Thêm mới nhà cung cấp
        /// </summary>
        public IActionResult Create()
        {
            ViewBag.Title = "Bổ sung nhà cung cấp";

            var model = new Supplier()
            {
                SupplierID = 0
            };

            return View("Edit", model);
        }

        /// <summary>
        /// Hiển thị biểu mẫu cập nhật nhà cung cấp
        /// </summary>
        public async Task<IActionResult> Edit(int id)
        {
            ViewBag.Title = "Cập nhật nhà cung cấp";

            var model = await PartnerDataService.GetSupplierAsync(id);

            if (model == null)
                return RedirectToAction("Index");

            return View(model);
        }

        /// <summary>
        /// Lưu dữ liệu nhà cung cấp (Insert + Update)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SaveData(Supplier data)
        {
            ViewBag.Title = data.SupplierID == 0
                ? "Bổ sung nhà cung cấp"
                : "Cập nhật nhà cung cấp";

            try
            {
                // Validate dữ liệu
                if (string.IsNullOrWhiteSpace(data.SupplierName))
                    ModelState.AddModelError(nameof(data.SupplierName),
                        "Vui lòng nhập tên nhà cung cấp");

                if (string.IsNullOrWhiteSpace(data.Province))
                    ModelState.AddModelError(nameof(data.Province),
                        "Vui lòng chọn tỉnh/thành");

                // Chuẩn hóa dữ liệu null
                if (string.IsNullOrEmpty(data.ContactName)) data.ContactName = "";
                if (string.IsNullOrEmpty(data.Phone)) data.Phone = "";
                if (string.IsNullOrEmpty(data.Email)) data.Email = "";
                if (string.IsNullOrEmpty(data.Address)) data.Address = "";

                if (!ModelState.IsValid)
                    return View("Edit", data);


                // Lưu database
                if (data.SupplierID == 0)
                    await PartnerDataService.AddSupplierAsync(data);
                else
                    await PartnerDataService.UpdateSupplierAsync(data);

                return RedirectToAction("Index");
            }
            catch
            {
                ModelState.AddModelError("Error",
                    "Hệ thống đang bận vui lòng thử lại sau");

                return View("Edit", data);
            }
        }

        /// <summary>
        /// Hiển thị trang xác nhận xóa nhà cung cấp
        /// </summary>
        public async Task<IActionResult> Delete(int id)
        {
            if (Request.Method == "POST")
            {
                await PartnerDataService.DeleteSupplierAsync(id);
                return RedirectToAction("Index");
            }

            var model = await PartnerDataService.GetSupplierAsync(id);

            if (model == null)
                return RedirectToAction("Index");

            ViewBag.AllowDelete =
                !(await PartnerDataService.IsUsedSupplierAsync(id));

            return View(model);
        }
    }
}