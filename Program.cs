using System.Net;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ResipWeb.Areas.Admin.Repository;
using ResipWeb.Models;
using ResipWeb.Services;
using Serilog;
using ResipWeb.Models.Payments.SePay;
using ResipWeb.Services.SePay;


Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.File("Logs/vnpay-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection("Email"));
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<VnPayService>();


builder.Services.AddHttpClient<MomoService>(); //đăng ký httpclient cho MomoService
builder.Services.AddHttpClient<PayPalService>();
builder.Services.AddHttpClient<ExchangeRateService>();

builder.Services.AddControllersWithViews();

// ✅ THÊM MEMORY CACHE để tối ưu hiệu suất
builder.Services.AddMemoryCache();

builder.Services.Configure<SePayOptions>(builder.Configuration.GetSection("SePay"));

builder.Services.AddScoped<ISePayService, SePayService>();
builder.Services.AddScoped<ISePayWebhookService, SePayWebhookService>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

//Connect VNPay API


// --- Đăng ký dịch vụ mã hóa mật khẩu ---
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

// 🔥 AUTHENTICATION (CẤU HÌNH ĐĂNG NHẬP)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)

    .AddCookie(options =>
    {
        // --- QUAN TRỌNG: Default trang Login của Khách ---
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";

        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.None; // CHẠY HTTP (Development)
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.Name = "ResipWebCookie"; // Đặt tên cookie để dễ debug

        // Ghi đè redirect để xử lý riêng cho Area Admin
        options.Events = new CookieAuthenticationEvents
        {
            OnRedirectToLogin = ctx =>
            {
                // Nếu request tới area Admin -> redirect tới Admin login (kèm returnUrl)
                if (ctx.Request.Path.StartsWithSegments("/Admin", StringComparison.OrdinalIgnoreCase))
                {
                    var returnUrl = WebUtility.UrlEncode(ctx.Request.Path + ctx.Request.QueryString);
                    ctx.Response.Redirect($"/Admin/Account/Login?returnUrl={returnUrl}");
                }
                else
                {
                    var returnUrl = WebUtility.UrlEncode(ctx.Request.Path + ctx.Request.QueryString);
                    ctx.Response.Redirect($"/Account/Login?returnUrl={returnUrl}");
                }
                return Task.CompletedTask;
            },
            OnRedirectToAccessDenied = ctx =>
            {
                if (ctx.Request.Path.StartsWithSegments("/Admin", StringComparison.OrdinalIgnoreCase))
                {
                    var returnUrl = WebUtility.UrlEncode(ctx.Request.Path + ctx.Request.QueryString);
                    ctx.Response.Redirect($"/Admin/Account/AccessDenied?returnUrl={returnUrl}");
                }
                else
                {
                    var returnUrl = WebUtility.UrlEncode(ctx.Request.Path + ctx.Request.QueryString);
                    ctx.Response.Redirect($"/Account/AccessDenied?returnUrl={returnUrl}");
                }
                return Task.CompletedTask;
            }
        };
    });

var app = builder.Build();

// =======================
// MIDDLEWARE (CẤU HÌNH PIPELINE)
// =======================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();   // BẮT BUỘC: Xác thực danh tính
app.UseAuthorization();    // BẮT BUỘC: Phân quyền truy cập

app.UseHttpsRedirection();


// 1. Định nghĩa cho vùng Admin
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

// 2. Định nghĩa cho Trang chủ khách hàng (BẮT BUỘC nằm dưới)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");




app.Run();