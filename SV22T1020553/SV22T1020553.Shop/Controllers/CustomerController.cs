using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020553.BusinessLayers;
using SV22T1020553.Models.Partner;

namespace SV22T1020553.Shop.Controllers
{
    [Authorize]
    public class CustomerController : Controller
    {
        
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userData = User.GetUserData();
            if (userData == null)
                return RedirectToAction("Login", "Account");

            int customerId = int.TryParse(userData?.UserId, out var id) ? id : 0;

            if (customerId <= 0)
                return RedirectToAction("Login", "Account");

            var customer = await PartnerDataService.GetCustomerAsync(customerId);
            if (customer == null)
                return RedirectToAction("Login", "Account");

            var model = new CustomerProfileViewModel
            {
                CustomerID = customer.CustomerID,
                CustomerName = customer.CustomerName,
                ContactName = customer.ContactName,
                Province = customer.Province,
                Address = customer.Address,
                Phone = customer.Phone,
                Email = customer.Email
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(CustomerProfileViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.CustomerName))
                ModelState.AddModelError(nameof(model.CustomerName), "Vui lòng nhập tên khách hàng");

            if (string.IsNullOrWhiteSpace(model.ContactName))
                ModelState.AddModelError(nameof(model.ContactName), "Vui lòng nhập tên liên hệ");

            if (!ModelState.IsValid)
                return View(model);

            var customer = new Customer
            {
                CustomerID = model.CustomerID,
                CustomerName = model.CustomerName,
                ContactName = model.ContactName,
                Province = model.Province,
                Address = model.Address,
                Phone = model.Phone,
                Email = model.Email
            };

            await PartnerDataService.UpdateCustomerAsync(customer);

            TempData["SuccessMessage"] = "Cập nhật thông tin thành công";
            return RedirectToAction(nameof(Profile));
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View(new ChangePasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.OldPassword))
                ModelState.AddModelError(nameof(model.OldPassword), "Vui lòng nhập mật khẩu cũ");

            if (string.IsNullOrWhiteSpace(model.NewPassword))
                ModelState.AddModelError(nameof(model.NewPassword), "Vui lòng nhập mật khẩu mới");

            if (model.NewPassword != model.ConfirmPassword)
                ModelState.AddModelError(nameof(model.ConfirmPassword), "Xác nhận mật khẩu không đúng");

            if (!ModelState.IsValid)
                return View(model);

            var userData = User.GetUserData();
            int customerId = int.TryParse(userData?.UserId, out var id) ? id : 0;

            if (customerId <= 0)
                return RedirectToAction("Login", "Account");

            var validUser = await SecurityDataService.AuthenticateCustomerAsync(userData.UserName, model.OldPassword);
            if (validUser == null)
            {
                ModelState.AddModelError(nameof(model.OldPassword), "Mật khẩu cũ không đúng");
                return View(model);
            }

            
            bool result = await SecurityDataService.ChangeCustomerPasswordAsync(userData.UserName, model.NewPassword);
            if (!result)
            {
                ModelState.AddModelError("", "Đổi mật khẩu thất bại");
                return View(model);
            }

            TempData["SuccessMessage"] = "Đổi mật khẩu thành công";
            return RedirectToAction(nameof(Profile));
        }
    }
}