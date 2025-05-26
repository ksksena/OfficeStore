using OfficeStore.Models;

namespace OfficeStore.Services
{
    public interface ICartService
    {
        Cart GetCart();
        void AddToCart(int productId, int quantity = 1);
        void RemoveFromCart(int productId);
        void UpdateQuantity(int productId, int quantity);
        void ClearCart();
        Task<bool> CreateOrderAsync(string userId);
    }
}
