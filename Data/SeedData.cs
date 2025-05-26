using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OfficeStore.Models;

namespace OfficeStore.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider, UserManager<User> userManager)
        {
            using var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>());

            // Создание администратора с проверкой результата
            if (!context.Users.Any(u => u.Email == "admin@office.com"))
            {
                var admin = new User
                {
                    UserName = "admin",
                    Email = "admin",
                    FirstName = "Админ",
                    LastName = "Системы",
                    EmailConfirmed = true
                };

                // ВАЖНО: Проверяем результат создания пользователя
                var createResult = await userManager.CreateAsync(admin, "Admin123!");

                if (createResult.Succeeded)
                {
                    // Только если пользователь создан успешно, добавляем роль
                    await userManager.AddToRoleAsync(admin, "Administrator");
                }
                else
                {
                    // Выводим ошибки создания пользователя
                    foreach (var error in createResult.Errors)
                    {
                        Console.WriteLine($"Ошибка создания пользователя: {error.Description}");
                    }
                }
            }

            // Остальной код остается без изменений
            if (!context.Suppliers.Any())
            {
                context.Suppliers.AddRange(
                    new Supplier { Name = "ОфисПро", ContactInfo = "office-pro@mail.ru, +7(495)123-45-67" },
                    new Supplier { Name = "КанцТорг", ContactInfo = "info@kanctorg.ru, +7(495)987-65-43" },
                    new Supplier { Name = "Статус", ContactInfo = "sales@status.ru, +7(495)555-12-34" }
                );
                await context.SaveChangesAsync();
            }

            if (!context.Products.Any())
            {
                var suppliers = context.Suppliers.ToList();
                context.Products.AddRange(
                    new Product { Name = "Ручка шариковая синяя", Description = "Классическая шариковая ручка с синими чернилами", Price = 25.00m, Stock = 100, SupplierId = suppliers[0].Id },
                    new Product { Name = "Карандаш простой HB", Description = "Графитовый карандаш твердости HB", Price = 15.00m, Stock = 150, SupplierId = suppliers[0].Id },
                    new Product { Name = "Тетрадь 48 листов", Description = "Тетрадь в клетку, 48 листов", Price = 45.00m, Stock = 80, SupplierId = suppliers[1].Id },
                    new Product { Name = "Папка-регистратор", Description = "Папка-регистратор А4, 75мм", Price = 120.00m, Stock = 50, SupplierId = suppliers[1].Id },
                    new Product { Name = "Степлер металлический", Description = "Степлер для скрепления до 20 листов", Price = 350.00m, Stock = 25, SupplierId = suppliers[2].Id }
                );
                await context.SaveChangesAsync();
            }
            // Создание тестового клиента
            if (!context.Users.Any(u => u.Email == "client@office.com"))
            {
                var client = new User
                {
                    UserName = "client@office.com",
                    Email = "client@office.com",
                    FirstName = "Иван",
                    LastName = "Петров",
                    EmailConfirmed = true
                };

                var createClientResult = await userManager.CreateAsync(client, "Client123!");

                if (createClientResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(client, "Client");
                }
            }

        }
    }
}
