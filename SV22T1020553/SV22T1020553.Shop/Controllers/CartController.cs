using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020553.BusinessLayers;
using SV22T1020553.Models.Sales;
using SV22T1020553.Shop.Models;

namespace SV22T1020553.Shop.Controllers
{
    public class CartController : Controller
    {
        public IActionResult Index()
        {
            return View(new CartViewModel
            {
                Items = ShoppingCartServices.GetShoppingCart()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        
        public async Task<IActionResult> Add(int productId, int quantity = 1, string? returnUrl = null, string actionType = "AddToCart")
        {
            var product = await CatalogDataService.GetProductAsync(productId);
            if (product == null || !product.IsSelling)
                return RedirectToAction("Index", "Home");

            quantity = quantity < 1 ? 1 : quantity;

            ShoppingCartServices.AddItemToCart(new OrderDetailViewInfo
            {
                ProductID = product.ProductID,
                ProductName = product.ProductName,
                Unit = product.Unit,
                Photo = product.Photo ?? string.Empty,
                Quantity = quantity,
                SalePrice = product.Price
            });

            TempData["SuccessMessage"] = "Đã thêm sản phẩm vào giỏ hàng";

           
            if (actionType == "BuyNow")
            {
                return RedirectToAction("Checkout", "Cart");
            }

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Update(int productId, int quantity)
        {
            var item = ShoppingCartServices.GetCartItem(productId);
            if (item == null)
                return RedirectToAction(nameof(Index));

            if (quantity <= 0)
                ShoppingCartServices.RemoveItemFromCart(productId);
            else
                ShoppingCartServices.UpdateCartItem(productId, quantity, item.SalePrice);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Remove(int productId)
        {
            ShoppingCartServices.RemoveItemFromCart(productId);
            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var cart = ShoppingCartServices.GetShoppingCart();
            if (!cart.Any())
                return RedirectToAction(nameof(Index));

            var currentUser = User.GetUserData();
            int customerId = int.TryParse(currentUser?.UserId, out var parsedCustomerId) ? parsedCustomerId : 0;
            var customer = customerId > 0 ? await PartnerDataService.GetCustomerAsync(customerId) : null;

            var model = new CheckoutViewModel
            {
                DeliveryProvince = customer?.Province ?? string.Empty,
                DeliveryAddress = customer?.Address ?? string.Empty,
                Provinces = await DictionaryDataService.ListProvincesAsync(),
                CartItems = cart
            };

            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            var cart = ShoppingCartServices.GetShoppingCart();
            if (!cart.Any())
                return RedirectToAction(nameof(Index));

            if (!ModelState.IsValid)
            {
                model.Provinces = await DictionaryDataService.ListProvincesAsync();
                model.CartItems = cart;
                return View(model);
            }

            var currentUser = User.GetUserData();
            int customerId = int.TryParse(currentUser?.UserId, out var parsedCustomerId) ? parsedCustomerId : 0;

            if (customerId <= 0)
                return Challenge();

            string fullDeliveryAddress = $"Tên: {model.DeliveryName} - SĐT: {model.DeliveryPhone} - Đ/c: {model.DeliveryAddress}";

            
            if (!string.IsNullOrWhiteSpace(model.Notes))
            {
                fullDeliveryAddress += $" - Ghi chú: {model.Notes}";
            }

            int orderId = await SalesDataService.AddOrderAsync(customerId, model.DeliveryProvince, fullDeliveryAddress, cart);

            
            ShoppingCartServices.ClearCart();

           
            return View("Success", new OrderSuccessViewModel
            {
                OrderID = orderId,
                CustomerName = currentUser?.DisplayName ?? "Quý khách"
            });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Clear()
        {
            
            ShoppingCartServices.ClearCart();
            return RedirectToAction(nameof(Index));
        }

   
        [Authorize]
        public async Task<IActionResult> OrderHistory()
        {
            var currentUser = User.GetUserData();
            int customerId = int.TryParse(currentUser?.UserId, out var parsedId) ? parsedId : 0;

            var input = new OrderSearchInput
            {
                Page = 1,
                PageSize = 1000,
                Status = null,
                SearchValue = ""
            };

            var result = await SalesDataService.ListOrdersAsync(input);

           
            var myOrders = result.DataItems.Where(order => order.CustomerID == customerId).ToList();

            return View(myOrders);
        }
        /// <summary>
        /// Xem chi tiết và trạng thái xử lý của một đơn hàng cụ thể
        /// </summary>
        [Authorize]
        public async Task<IActionResult> OrderDetails(int id)
        {
            
            var order = await SalesDataService.GetOrderAsync(id);

            if (order == null)
                return NotFound();

            
            var currentUser = User.GetUserData();
            if (order.CustomerID.ToString() != currentUser?.UserId)
                return Forbid();

           
            var details = await SalesDataService.ListDetailsAsync(id);

            ViewBag.Order = order;

            return View(details);
        }

        /// <summary>
        /// Hủy đơn hàng (Chỉ áp dụng cho đơn hàng Mới)
        /// </summary>
        [Authorize]
        public async Task<IActionResult> CancelOrder(int id)
        {
           
            var orderView = await SalesDataService.GetOrderAsync(id);
            if (orderView == null)
            {
                return RedirectToAction("OrderHistory");
            }

          
            var currentUser = User.GetUserData();
            int customerId = int.TryParse(currentUser?.UserId, out var parsedId) ? parsedId : 0;

            if (orderView.CustomerID != customerId)
            {
                return RedirectToAction("OrderHistory");
            }

            
            if (orderView.Status == OrderStatusEnum.New)
            {
                var orderToUpdate = new Order
                {
                    OrderID = orderView.OrderID,
                    CustomerID = orderView.CustomerID,
                    EmployeeID = orderView.EmployeeID,
                    OrderTime = orderView.OrderTime,
                    AcceptTime = orderView.AcceptTime,
                    ShipperID = orderView.ShipperID,
                    ShippedTime = orderView.ShippedTime,
                    FinishedTime = orderView.FinishedTime,
                    DeliveryProvince = orderView.DeliveryProvince,
                    DeliveryAddress = orderView.DeliveryAddress,

                    Status = OrderStatusEnum.Cancelled
                };

                
                await SalesDataService.UpdateOrderAsync(orderToUpdate);
            }

            return RedirectToAction("OrderHistory");
        }



    }
}
