using SV22T1020553.Models.Sales;

namespace SV22T1020553.Shop
{
    public static class ShoppingCartServices
    {
        private const string CART = "ShoppingCart";

        public static List<OrderDetailViewInfo> GetShoppingCart()
        {
            var cart = ApplicationContext.GetSessionData<List<OrderDetailViewInfo>>(CART);
            if (cart == null)
            {
                cart = new List<OrderDetailViewInfo>();
                ApplicationContext.SetSessionData(CART, cart);
            }

            return cart;
        }

        public static OrderDetailViewInfo? GetCartItem(int productID)
        {
            return GetShoppingCart().FirstOrDefault(m => m.ProductID == productID);
        }

        public static void AddItemToCart(OrderDetailViewInfo data)
        {
            var cart = GetShoppingCart();
            var existItem = cart.FirstOrDefault(m => m.ProductID == data.ProductID);

            if (existItem == null)
                cart.Add(data);
            else
            {
                existItem.Quantity += data.Quantity;
                existItem.SalePrice = data.SalePrice;
            }

            ApplicationContext.SetSessionData(CART, cart);
        }

        public static void UpdateCartItem(int productID, int quantity, decimal salePrice)
        {
            // 1. Lấy giỏ hàng ra một lần duy nhất và gán vào biến 'cart'
            var cart = GetShoppingCart();

            // 2. Tìm món hàng cần sửa trong cái 'cart' đó
            var existItem = cart.FirstOrDefault(m => m.ProductID == productID);
            if (existItem == null)
                return;

            // 3. Cập nhật số lượng mới
            existItem.Quantity = quantity;
            existItem.SalePrice = salePrice;

            // 4. Lưu lại chính cái 'cart' đã được sửa vào Session
            ApplicationContext.SetSessionData(CART, cart);
        }

        public static void RemoveItemFromCart(int productID)
        {
            var cart = GetShoppingCart();
            var item = cart.FirstOrDefault(m => m.ProductID == productID);
            if (item == null)
                return;

            cart.Remove(item);
            ApplicationContext.SetSessionData(CART, cart);
        }

        public static void ClearCart()
        {
            ApplicationContext.SetSessionData(CART, new List<OrderDetailViewInfo>());
        }
    }
}
