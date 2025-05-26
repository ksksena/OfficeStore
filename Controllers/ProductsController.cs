using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeStore.Data;
using OfficeStore.Models;

namespace OfficeStore.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string sortOrder, string searchString, string currentFilter)
        {
            try
            {
                if (searchString != null)
                {
                    currentFilter = searchString;
                }
                else
                {
                    searchString = currentFilter;
                }

                ViewData["CurrentFilter"] = searchString;
                ViewData["CurrentSort"] = sortOrder;
                ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
                ViewData["PriceSortParm"] = sortOrder == "price" ? "price_desc" : "price";
                ViewData["StockSortParm"] = sortOrder == "stock" ? "stock_desc" : "stock";

                var products = from p in _context.Products.Include(p => p.Supplier)
                               select p;

                // Поиск
                if (!String.IsNullOrEmpty(searchString))
                {
                    products = products.Where(p => p.Name.Contains(searchString)
                                                || p.Description.Contains(searchString)
                                                || p.Supplier.Name.Contains(searchString));
                }

                // Сортировка
                switch (sortOrder)
                {
                    case "name_desc":
                        products = products.OrderByDescending(p => p.Name);
                        break;
                    case "price":
                        products = products.OrderBy(p => p.Price);
                        break;
                    case "price_desc":
                        products = products.OrderByDescending(p => p.Price);
                        break;
                    case "stock":
                        products = products.OrderBy(p => p.Stock);
                        break;
                    case "stock_desc":
                        products = products.OrderByDescending(p => p.Stock);
                        break;
                    default:
                        products = products.OrderBy(p => p.Name);
                        break;
                }

                var productList = await products.ToListAsync();
                return View(productList);
            }
            catch (Exception ex)
            {
                // Если есть проблемы с БД, возвращаем пустой список
                ViewData["Error"] = $"Ошибка загрузки товаров: {ex.Message}";
                return View(new List<Product>());
            }
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Supplier)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }
    }
}
