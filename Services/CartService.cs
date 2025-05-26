using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OfficeStore.Data;
using OfficeStore.Models;

namespace OfficeStore.Services
{
    public class CartService : ICartService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _context;
        private const string CartSessionKey = "Cart";

        public CartService(IHttpContextAccessor httpContextAccessor, ApplicationDbContext context)
        {
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }

        public Cart GetCart()
        {
            var session = _httpContextAccessor.HttpContext.Session;
            var cartJson = session.GetString(CartSessionKey);

            if (string.IsNullOrEmpty(cartJson))
                return new Cart();

            return JsonConvert.DeserializeObject<Cart>(cartJson) ?? new Cart();
        }

        private void SaveCart(Cart cart)
        {
            var session = _httpContextAccessor.HttpContext.Session;
            var cartJson = JsonConvert.SerializeObject(cart);
            session.SetString(CartSessionKey, cartJson);
        }

        public void AddToCart(int productId, int quantity = 1)
        {
            var product = _context.Products.Find(productId);
            if (product != null && product.Stock >= quantity)
            {
                var cart = GetCart();
                cart.AddItem(product, quantity);
                SaveCart(cart);
            }
        }

        public void RemoveFromCart(int productId)
        {
            var cart = GetCart();
            cart.RemoveItem(productId);
            SaveCart(cart);
        }

        public void UpdateQuantity(int productId, int quantity)
        {
            var cart = GetCart();
            cart.UpdateQuantity(productId, quantity);
            SaveCart(cart);
        }

        public void ClearCart()
        {
            var session = _httpContextAccessor.HttpContext.Session;
            session.Remove(CartSessionKey);
        }

        public async Task<bool> CreateOrderAsync(string userId)
        {
            var cart = GetCart();
            if (!cart.Items.Any()) return false;

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Создаем заказ
                var order = new Order
                {
                    UserId = userId,
                    OrderDate = DateTime.Now,
                    TotalAmount = cart.GetTotalPrice()
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // Создаем детали заказа
                foreach (var item in cart.Items)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product == null || product.Stock < item.Quantity)
                    {
                        await transaction.RollbackAsync();
                        return false;
                    }

                    // Уменьшаем остаток товара
                    product.Stock -= item.Quantity;

                    // Добавляем деталь заказа
                    var orderDetail = new OrderDetail
                    {
                        OrderId = order.Id,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.Price
                    };

                    _context.OrderDetails.Add(orderDetail);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                ClearCart();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }
    }
}
