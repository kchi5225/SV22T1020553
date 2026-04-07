using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020553.Admin;
using SV22T1020553.Models.Common;
using SV22T1020553.Models.Partner;
using System.Threading.Tasks;

namespace SV22T1020553.Admin.Controllers
{
    [Authorize]
    public class ShipperController : Controller
    {
        //private const int PAGESIZE = 10;
        private const string SHIPPER_SEARCH = "ShipperSearchInput";

        /// <summary>
        /// Hiển thị danh sách shipper
        /// </summary>
        public IActionResult Index()
        {
            var input = ApplicationContext.GetSessionData<PaginationSearch>(SHIPPER_SEARCH);
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
        /// Tìm kiếm shipper
        /// </summary>
        public async Task<IActionResult> Search(PaginationSearch input)
        {
            var result = await PartnerDataService.ListShippersAsync(input);

            ApplicationContext.SetSessionData(SHIPPER_SEARCH, input);

            return View(result); 
        }

        /// <summary>
        /// Thêm mới shipper
        /// </summary>
        public IActionResult Create()
        {
            ViewBag.Title = "Bổ sung người giao hàng";

            var model = new Shipper()
            {
                ShipperID = 0
            };

            return View("Edit", model);
        }

        /// <summary>
        /// Hiển thị biểu mẫu cập nhật shipper
        /// </summary>
        public async Task<IActionResult> Edit(int id)
        {
            ViewBag.Title = "Cập nhật người giao hàng";

            var model = await PartnerDataService.GetShipperAsync(id);

            if (model == null)
                return RedirectToAction("Index");

            return View(model);
        }

        /// <summary>
        /// Lưu dữ liệu shipper (Insert + Update)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SaveData(Shipper data)
        {
            ViewBag.Title = data.ShipperID == 0
                ? "Bổ sung người giao hàng"
                : "Cập nhật người giao hàng";

            try
            {
                // Validate dữ liệu
                if (string.IsNullOrWhiteSpace(data.ShipperName))
                    ModelState.AddModelError(nameof(data.ShipperName),
                        "Vui lòng nhập tên người giao hàng");

                if (string.IsNullOrEmpty(data.Phone))
                    data.Phone = "";

                if (!ModelState.IsValid)
                    return View("Edit", data);


                // Lưu database
                if (data.ShipperID == 0)
                {
                    await PartnerDataService.AddShipperAsync(data);
                }
                else
                {
                    await PartnerDataService.UpdateShipperAsync(data);
                }

                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ModelState.AddModelError("Error",
                    "Hệ thống đang bận vui lòng thử lại sau");

                return View("Edit", data);
            }
        }

        /// <summary>
        /// Hiển thị trang xác nhận xóa shipper
        /// </summary>
        public async Task<IActionResult> Delete(int id)
        {
            if (Request.Method == "POST")
            {
                await PartnerDataService.DeleteShipperAsync(id);
                return RedirectToAction("Index");
            }

            var model = await PartnerDataService.GetShipperAsync(id);

            if (model == null)
                return RedirectToAction("Index");

            ViewBag.AllowDelete =
                !(await PartnerDataService.IsUsedShipperAsync(id));

            return View(model);
        }
    }
}