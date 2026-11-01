using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using StudentInformationSystem.API.Data;
using StudentInformationSystem.API.Repositories;
using StudentInformationSystem.API.Services.Implementations;
using StudentInformationSystem.API.Services.Interfaces;

namespace StudentInformationSystem.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. Verilənlər Bazası (SQL Server və EF Core)
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? "Server=.\\SQLEXPRESS;Database=StudentInformationSystemDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true;";

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connectionString));

            // 2. Repositories və Services
            builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();
            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IDepartmentService, DepartmentService>();
            builder.Services.AddScoped<IProgrammeService, ProgrammeService>();
            builder.Services.AddScoped<ITermService, TermService>();
            builder.Services.AddScoped<IStudentService, StudentService>();
            builder.Services.AddScoped<ICourseService, CourseService>();
            builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

            // 3. JWT Autentifikasiyası
            var jwtSettings = builder.Configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? "StudentInformationSystem_SuperSecretKey_2026_KeyMustBeLongEnough!";
            var issuer = jwtSettings["Issuer"] ?? "StudentInformationSystem.API";
            var audience = jwtSettings["Audience"] ?? "StudentInformationSystem.Web";

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    ClockSkew = TimeSpan.Zero
                };
            });

            builder.Services.AddAuthorization();

            // 4. CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            // 5. Controllers və OpenAPI (Scalar)
            builder.Services.AddControllers();
            builder.Services.AddOpenApi();

            var app = builder.Build();

            // 6. HTTP Pipeline
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference();
            }

            app.UseHttpsRedirection();

            app.UseCors("AllowAll");

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            // 7. İlkin Məlumatların (Seed Data) və Miqrasiyaların Avtomatik Tətbiqi
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<AppDbContext>();
                    var passwordHasher = services.GetRequiredService<IPasswordHasher>();
                    await DbInitializer.SeedAsync(context, passwordHasher);
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "Verilənlər bazasını inisializasiya edərkən xəta baş verdi.");
                }
            }

            await app.RunAsync();
        }
    }
}
