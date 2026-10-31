using Microsoft.AspNetCore.Authentication.Cookies;
using StudentInformationSystem.Web.Services;

namespace StudentInformationSystem.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. MVC və Views
            builder.Services.AddControllersWithViews();

            // 2. HttpContext və HttpClient (API-yə müraciət üçün)
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddHttpClient<IApiService, ApiService>()
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
                });

            // 3. Cookie Autentifikasiyası (JWT token cookie daxilində saxlanılır)
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.LogoutPath = "/Account/Logout";
                    options.AccessDeniedPath = "/Account/AccessDenied";
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(120);
                    options.SlidingExpiration = true;
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SameSite = SameSiteMode.Lax;
                });

            builder.Services.AddAuthorization();

            var app = builder.Build();

            // 4. HTTP Sorğu boru kəməri (Pipeline)
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}
