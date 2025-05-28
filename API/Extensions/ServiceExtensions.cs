using API.DbConects.DTOs.Client.TaiKhoan;
using API.Middleware;
using API.Repositories;
using API.Repositories.Implementations;
using API.Repositories.Interfaces;
using API.Services;
using API.Services.Implementations;
using API.Services.Interfaces;
using API.Services.JwtServices;
using API.Services.Validation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace API.Extensions
{
    public static class ServiceExtensions
    {
        public static void AddCustomAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(opt =>
            {
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(opt =>
            {
                opt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]))
                };
                opt.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                        {
                            context.Response.Headers.Add("Token-Expired", "true");
                        }
                        return Task.CompletedTask;
                    }
                };
            });
        }

        public static void AddCustomServices(this IServiceCollection services)
        {
            // Add repositories
            services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));

            // Add services
            services.AddScoped<IAuthenService, AuthenService>();
            services.AddScoped(typeof(IBaseService<>), typeof(BaseService<>));
            services.AddScoped<IJwtServices, JwtServices>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IHoaDonService, HoaDonService>();
            services.AddScoped<ICuaHangService, CuaHangService>();
            services.AddScoped<IThongKeService, ThongKeService>();
            services.AddScoped<IDiaChiService, DiaChiService>();
            services.AddScoped<IGioHangService, GioHangService>();
            services.AddScoped<ISanPhamService, SanPhamService>();
            services.AddScoped<IKhachHangService, KhachHangService>();
            services.AddScoped<IKhachHangValidationService, KhachHangValidationService>();


            services.AddScoped<TaiKhoanClientValidationService>();
            services.AddScoped<DangNhapValidationService>();
            services.AddScoped<DoiMatKhauValidationService>();
            services.AddMemoryCache();

            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<BrotliCompressionProvider>();
                options.Providers.Add<GzipCompressionProvider>();
            });
        }

        public static void AddCustomMiddleware(this IApplicationBuilder app)
        {
            app.UseMiddleware<ExceptionMiddleware>();
            app.UseStaticFiles();
            app.UseResponseCompression();
            // app.UseResponseCaching();

        }

        public static void AddCustomCors(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    builder =>
                    {
                        builder.WithOrigins("http://localhost:3000", "https://localhost:3000")
                               .AllowAnyMethod()
                               .AllowAnyHeader()
                               .AllowCredentials()
                               //    .SetPreflightMaxAge(TimeSpan.FromHours(1))
                               ;

                    });
            });
        }
    }
}