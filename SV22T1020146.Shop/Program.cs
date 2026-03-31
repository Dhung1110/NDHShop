using Microsoft.AspNetCore.Authentication.Cookies;
using SV22T1020146.BusinessLayers;
using SV22T1020146.Shop;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddHttpContextAccessor();

builder.Services.AddControllersWithViews()
    .AddMvcOptions(option =>
    {
        option.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
    });

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(option =>
    {
        option.LoginPath = "/Account/Login";
        option.AccessDeniedPath = "/Account/AccessDenied";
    });

builder.Services.AddSession();

// Build
var app = builder.Build();

// Pipeline
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

// Route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Culture
var cultureInfo = new CultureInfo("vi-VN");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

// ?? QUAN TR?NG
ApplicationContext.Configure(
    app.Services.GetRequiredService<IHttpContextAccessor>(),
    app.Services.GetRequiredService<IWebHostEnvironment>(),
    app.Configuration
);

// ?? QUAN TR?NG
string connectionString = builder.Configuration.GetConnectionString("LiteCommerceDB")
    ?? throw new Exception("Missing connection string");

Configuration.Initialize(connectionString);

app.Run();