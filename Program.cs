using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OfficeStore.Data;
using OfficeStore.Models;
using OfficeStore.Services;

var builder = WebApplication.CreateBuilder(args);

// ��������� Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ��������� Identity � ������
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// ������ ��� ��� ������ � Razor Pages (���� ����� ���������� ��������)
builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// ��������� ������ ��� �������
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ������������ �������
builder.Services.AddScoped<ICartService, CartService>();



var app = builder.Build();

// ����� ��������� �������� ����� � ���������� �������
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<User>>();

    // �������� ����� ��� ������� ����������
    string[] roleNames = { "Administrator", "Client" };
    IdentityResult roleResult;

    foreach (var roleName in roleNames)
    {
        var roleExist = await roleManager.RoleExistsAsync(roleName);
        if (!roleExist)
        {
            roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    // ���������� ��������� �������
    await SeedData.Initialize(services, userManager);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
