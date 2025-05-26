using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OfficeStore.Data;
using OfficeStore.Models;

namespace OfficeStore.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminController> _logger;

        public AdminController(ApplicationDbContext context, ILogger<AdminController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var stats = new AdminDashboardViewModel
            {
                TotalProducts = await _context.Products.CountAsync(),
                TotalSuppliers = await _context.Suppliers.CountAsync(),
                TotalOrders = await _context.Orders.CountAsync(),
                TotalUsers = await _context.Users.CountAsync()
            };
            return View(stats);
        }

        public async Task<IActionResult> Products()
        {
            var products = await _context.Products
                .Include(p => p.Supplier)
                .OrderBy(p => p.Name)
                .ToListAsync();
            return View(products);
        }
        // Просмотр всех заказов (для админа)
        public async Task<IActionResult> Orders()
        {
            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        // Детали заказа для админа
        public async Task<IActionResult> OrderDetails(int? id)
        {
            if (id == null)
            {
                TempData["Error"] = "Заказ не найден!";
                return RedirectToAction(nameof(Orders));
            }

            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .ThenInclude(p => p.Supplier)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                TempData["Error"] = "Заказ не найден!";
                return RedirectToAction(nameof(Orders));
            }

            return View(order);
        }


        public async Task<IActionResult> CreateProduct()
        {
            ViewBag.Suppliers = new SelectList(await _context.Suppliers.ToListAsync(), "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct(Product product)
        {
            _logger.LogInformation($"Попытка создания товара: {product.Name}");

            try
            {
                // Убираем проверку ModelState для отладки
                _logger.LogInformation($"Данные товара: Name={product.Name}, Price={product.Price}, Stock={product.Stock}, SupplierId={product.SupplierId}");

                // Проверяем, что поставщик существует
                var supplier = await _context.Suppliers.FindAsync(product.SupplierId);
                if (supplier == null)
                {
                    TempData["Error"] = "Выбранный поставщик не найден!";
                    ViewBag.Suppliers = new SelectList(await _context.Suppliers.ToListAsync(), "Id", "Name");
                    return View(product);
                }

                // Создаем новый товар
                var newProduct = new Product
                {
                    Name = product.Name,
                    Description = product.Description ?? "",
                    Price = product.Price,
                    Stock = product.Stock,
                    SupplierId = product.SupplierId
                };

                _context.Products.Add(newProduct);
                _logger.LogInformation("Товар добавлен в контекст");

                var result = await _context.SaveChangesAsync();
                _logger.LogInformation($"SaveChanges вернул: {result}");

                if (result > 0)
                {
                    TempData["Success"] = $"Товар '{product.Name}' успешно добавлен!";
                    _logger.LogInformation($"Товар успешно сохранен с ID: {newProduct.Id}");
                    return RedirectToAction(nameof(Products));
                }
                else
                {
                    TempData["Error"] = "Товар не был сохранен в базе данных!";
                    _logger.LogWarning("SaveChanges вернул 0 - товар не сохранен");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при создании товара: {ex.Message}");
                TempData["Error"] = $"Ошибка при добавлении товара: {ex.Message}";
            }

            ViewBag.Suppliers = new SelectList(await _context.Suppliers.ToListAsync(), "Id", "Name", product.SupplierId);
            return View(product);
        }

        public async Task<IActionResult> EditProduct(int? id)
        {
            if (id == null)
            {
                TempData["Error"] = "Товар не найден!";
                return RedirectToAction(nameof(Products));
            }

            var product = await _context.Products
                .Include(p => p.Supplier) // Добавляем Include для отображения текущего поставщика
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                TempData["Error"] = "Товар не найден!";
                return RedirectToAction(nameof(Products));
            }

            // ВАЖНО: Передаем список поставщиков
            ViewBag.Suppliers = new SelectList(await _context.Suppliers.ToListAsync(), "Id", "Name", product.SupplierId);
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(int id, Product product)
        {
            if (id != product.Id)
            {
                TempData["Error"] = "Ошибка идентификации товара!";
                return RedirectToAction(nameof(Products));
            }

            try
            {
                _logger.LogInformation($"Попытка обновления товара ID: {id}");

                var existingProduct = await _context.Products.FindAsync(id);
                if (existingProduct == null)
                {
                    TempData["Error"] = "Товар не найден!";
                    return RedirectToAction(nameof(Products));
                }

                // Обновляем свойства
                existingProduct.Name = product.Name;
                existingProduct.Description = product.Description ?? "";
                existingProduct.Price = product.Price;
                existingProduct.Stock = product.Stock;
                existingProduct.SupplierId = product.SupplierId;

                _context.Products.Update(existingProduct);
                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    TempData["Success"] = $"Товар '{product.Name}' успешно обновлен!";
                    _logger.LogInformation($"Товар ID {id} успешно обновлен");
                    return RedirectToAction(nameof(Products));
                }
                else
                {
                    TempData["Error"] = "Изменения не были сохранены!";
                    _logger.LogWarning($"SaveChanges вернул 0 при обновлении товара ID {id}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при обновлении товара ID {id}: {ex.Message}");
                TempData["Error"] = $"Ошибка при обновлении товара: {ex.Message}";
            }

            ViewBag.Suppliers = new SelectList(await _context.Suppliers.ToListAsync(), "Id", "Name", product.SupplierId);
            return View(product);
        }



        public async Task<IActionResult> DeleteProduct(int? id)
        {
            if (id == null)
            {
                TempData["Error"] = "Товар не найден!";
                return RedirectToAction(nameof(Products));
            }

            var product = await _context.Products
                .Include(p => p.Supplier)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null)
            {
                TempData["Error"] = "Товар не найден!";
                return RedirectToAction(nameof(Products));
            }

            return View(product);
        }

        [HttpPost, ActionName("DeleteProduct")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProductConfirmed(int id)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                {
                    TempData["Error"] = "Товар не найден!";
                    return RedirectToAction(nameof(Products));
                }

                _context.Products.Remove(product);
                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    TempData["Success"] = $"Товар '{product.Name}' успешно удален!";
                    _logger.LogInformation($"Товар ID {id} успешно удален");
                }
                else
                {
                    TempData["Error"] = "Товар не был удален!";
                    _logger.LogWarning($"SaveChanges вернул 0 при удалении товара ID {id}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при удалении товара ID {id}: {ex.Message}");
                TempData["Error"] = $"Ошибка при удалении товара: {ex.Message}";
            }

            return RedirectToAction(nameof(Products));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}
