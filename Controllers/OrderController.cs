using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeStore.Data;
using OfficeStore.Models;
using System.Security.Claims;

namespace OfficeStore.Controllers
{
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Прямое оформление заказа без корзины
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> BuyNow(int productId, int quantity = 1)
        {
            try
            {
                var product = await _context.Products.FindAsync(productId);
                if (product == null)
                {
                    TempData["Error"] = "Товар не найден!";
                    return RedirectToAction("Index", "Product");
                }

                if (product.Stock < quantity)
                {
                    TempData["Error"] = "Недостаточно товара на складе!";
                    return RedirectToAction("Index", "Product");
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Создаем заказ
                var order = new Order
                {
                    UserId = userId,
                    OrderDate = DateTime.Now,
                    TotalAmount = product.Price * quantity
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // Добавляем детали заказа
                var orderDetail = new OrderDetail
                {
                    OrderId = order.Id,
                    ProductId = productId,
                    Quantity = quantity,
                    UnitPrice = product.Price
                };

                _context.OrderDetails.Add(orderDetail);

                // Уменьшаем остаток товара
                product.Stock -= quantity;

                await _context.SaveChangesAsync();

                TempData["Success"] = $"Заказ оформлен! Товар '{product.Name}' (количество: {quantity}) заказан на сумму {(product.Price * quantity):C}";

                return RedirectToAction("MyOrders");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Ошибка при оформлении заказа: {ex.Message}";
                return RedirectToAction("Index", "Product");
            }
        }

        // Просмотр заказов пользователя
        [Authorize]
        public async Task<IActionResult> MyOrders()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var orders = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        // Детали заказа
        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }
    }
}
