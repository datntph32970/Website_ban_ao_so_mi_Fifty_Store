using Microsoft.AspNetCore.Mvc.Filters;
using API.Services.Interfaces;
using API.DbConects.Entities.Entities_Khuyen_Mai;
using API.DbConects.Entities.Entities_San_Pham;
using System.Linq;
using System.Threading.Tasks;

namespace API.Filters
{
    public class DiscountValidationFilter : IAsyncActionFilter
    {
        private readonly IServiceProvider _serviceProvider;

        public DiscountValidationFilter(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            using (var scope = _serviceProvider.CreateScope())
            {

                var sanPhamService = scope.ServiceProvider.GetRequiredService<ISanPhamService>();
                var hoaDonService = scope.ServiceProvider.GetRequiredService<IHoaDonService>();

                await sanPhamService.RemoveInvalidDiscountsAsync();
                await sanPhamService.RemoveInvalidPromotionsAsync();
                await hoaDonService.XoaHoaDonChuaThanhToanQuaHan();
            }

            await next();
        }
    }
}