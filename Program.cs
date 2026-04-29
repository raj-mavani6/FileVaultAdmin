using Serilog;
using FileVaultAdmin.Data;
using FileVaultAdmin.Models.Settings;
using FileVaultAdmin.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, lc) => lc.ReadFrom.Configuration(ctx.Configuration));

    // MongoDB
    builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDb"));
    builder.Services.AddSingleton<MongoDbContext>();
    builder.Services.AddScoped<AdminService>();

    // Auth - cookie based, admin only
    builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(options =>
        {
            options.LoginPath = "/Auth/Login";
            options.LogoutPath = "/Auth/Logout";
            options.AccessDeniedPath = "/Auth/Login";
            options.ExpireTimeSpan = TimeSpan.FromHours(8);
            options.SlidingExpiration = true;
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.Name = "FileVaultAdmin.Auth";
        });

    builder.Services.AddControllersWithViews();

    var app = builder.Build();

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
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    Log.Information("FileVault Admin Panel starting on {Urls}",
        string.Join(", ", app.Urls.Any() ? app.Urls : new[] { "https://localhost:5003" }));

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Admin Panel startup failed");
}
finally
{
    Log.CloseAndFlush();
}
