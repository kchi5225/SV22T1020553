using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020553.Admin;
using SV22T1020553.BusinessLayers;

namespace SV22T1020553.Admin.Controllers
{
    /// <summary>
    /// Các chức năng liên quan đến tài khoản
    /// </summary>
    [Authorize]
    public class AccountController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            ViewBag.Title = "Thay đổi mật khẩu";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword, string confirmPassword)
        {
            ViewBag.Title = "Thay đổi mật khẩu";

            
            if (string.IsNullOrWhiteSpace(oldPassword))
                ModelState.AddModelError("Error", "Vui lòng nhập mật khẩu cũ");

            if (string.IsNullOrWhiteSpace(newPassword))
                ModelState.AddModelError("Error", "Vui lòng nhập mật khẩu mới");

            if (newPassword != confirmPassword)
                ModelState.AddModelError("Error", "Mật khẩu xác nhận không khớp");

            
            var currentUser = User.GetUserData();
            if (currentUser == null || string.IsNullOrWhiteSpace(currentUser.UserName))
            {
                ModelState.AddModelError("Error", "Không xác định được tài khoản đăng nhập");
                return View();
            }

            if (!ModelState.IsValid)
                return View();

           
            var checkOldPass = await SecurityDataService.AuthenticateEmployeeAsync(currentUser.UserName, CryptHelper.HashMD5(oldPassword));
            if (checkOldPass == null)
            {
                ModelState.AddModelError("Error", "Mật khẩu cũ không chính xác");
                return View();
            }

            
            string newPasswordHash = CryptHelper.HashMD5(newPassword);
            bool success = await SecurityDataService.ChangeEmployeePasswordAsync(currentUser.UserName, newPasswordHash);

            if (!success)
            {
                ModelState.AddModelError("Error", "Không thể cập nhật mật khẩu vào cơ sở dữ liệu");
                return View();
            }

            ViewBag.SuccessMessage = "Đổi mật khẩu thành công!";
            return View();
        }

        /// <summary>
        /// Đăng xuất
        /// </summary>
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        /// <summary>
        /// Đăng nhập
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            ViewBag.Title = "Không có quyền truy cập";
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password)
        {
            ViewBag.Username = username;

            
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("Error", "Vui lòng nhập đầy đủ email và mật khẩu");
                return View();
            }

            var userAccount = await SecurityDataService.AuthenticateEmployeeAsync(username, CryptHelper.HashMD5(password));

            if (userAccount == null)
            {
                ModelState.AddModelError("Error", "Đăng nhập thất bại");
                return View();
            }

            var userData = new WebUserData()
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

            var principal = userData.CreatePrincipal();

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
            return RedirectToAction("Index", "Home");
        }
    }
}
