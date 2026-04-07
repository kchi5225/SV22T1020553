using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020553.BusinessLayers;
using SV22T1020553.Shop.Models;

namespace SV22T1020553.Shop.Controllers
{
    public class AccountController : Controller
    {
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginViewModel());
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;

            if (string.IsNullOrWhiteSpace(model.UserName))
                ModelState.AddModelError(nameof(model.UserName), "Vui lòng nhập email");

            if (string.IsNullOrWhiteSpace(model.Password))
                ModelState.AddModelError(nameof(model.Password), "Vui lòng nhập mật khẩu");

            if (!ModelState.IsValid)
                return View(model);

            var userAccount = await SecurityDataService.AuthenticateCustomerAsync(model.UserName, model.Password);
            if (userAccount == null)
            {
                ModelState.AddModelError("LoginError", "Email hoặc mật khẩu không đúng");
                return View(model);
            }

            var userData = new WebUserData
            {
                UserId = userAccount.UserId,
                UserName = userAccount.UserName,
                DisplayName = userAccount.DisplayName,
                Email = userAccount.Email,
                Photo = userAccount.Photo,
                Roles = userAccount.RoleNames
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .ToList()
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                userData.CreatePrincipal());

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        [HttpPost] 
        [ValidateAntiForgeryToken] 
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction(nameof(Login));
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
             
                bool isEmailExist = await SecurityDataService.CheckEmailInUseAsync(model.Email);

                if (isEmailExist)
                {
                    ModelState.AddModelError(nameof(model.Email), "Email này đã được sử dụng. Vui lòng chọn email khác.");
                    return View(model);
                }

                
                int newCustomerId = await SecurityDataService.RegisterCustomerAsync(
                    model.DisplayName,
                    model.Email,
                    model.Password
                );

                if (newCustomerId > 0)
                {
                    
                    TempData["SuccessMessage"] = "Đăng ký tài khoản thành công! Vui lòng đăng nhập.";
                    return RedirectToAction(nameof(Login));
                }
                else
                {
                   
                    ModelState.AddModelError("", "Đăng ký thất bại do lỗi dữ liệu. Vui lòng thử lại sau.");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
             
                ModelState.AddModelError("", "Đã xảy ra lỗi hệ thống: " + ex.Message);
                return View(model);
            }
        }
    }
}
