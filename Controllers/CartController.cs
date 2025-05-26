using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeStore.Services;
using System.Security.Claims;

namespace OfficeStore.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        public IActionResult Index()
        {
            var cart = _cartService.GetCart();
            return View(cart);
        }

        [HttpPost]
        public IActionResult AddToCart(int productId, int quantity = 1)
        {
            _cartService.AddToCart(productId, quantity);
            TempData["Success"] = "Товар добавлен в корзину!";
            return RedirectToAction("Index", "Product");
        }

        [HttpPost]
        public IActionResult RemoveFromCart(int productId)
        {
            _cartService.RemoveFromCart(productId);
            TempData["Success"] = "Товар удален из корзины!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult UpdateQuantity(int productId, int quantity)
        {
            _cartService.UpdateQuantity(productId, quantity);
            return RedirectToAction("Index");
        }

        [Authorize]
        public IActionResult Checkout()
        {
            var cart = _cartService.GetCart();
            if (!cart.Items.Any())
            {
                TempData["Error"] = "Корзина пуста!";
                return RedirectToAction("Index");
            }
            return View(cart);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> PlaceOrder()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var success = await _cartService.CreateOrderAsync(userId);

            if (success)
            {
                TempData["Success"] = "Заказ успешно оформлен!";
                return RedirectToAction("OrderSuccess");
            }
            else
            {
                TempData["Error"] = "Ошибка при оформлении заказа. Проверьте наличие товаров.";
                return RedirectToAction("Checkout");
            }
        }

        public IActionResult OrderSuccess()
        {
            return View();
        }
    }
}
