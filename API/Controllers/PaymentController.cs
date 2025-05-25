using Microsoft.AspNetCore.Mvc;
using API.Services;
using API.Services.Interfaces;
using API.DbConects.Entities.Entities_Hoa_Don;
using System.Web;
using Microsoft.EntityFrameworkCore;
using System.Net;
using API.DbConects.Entities.Entities_San_Pham;

namespace API.Controllers
{
    [Route("api/payment")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly VNPayService _vnPayService;
        private readonly IHoaDonService _hoaDonService;
        private readonly IBaseService<SanPhamChiTiet> _sanPhamChiTietService;
        private readonly IBaseService<HoaDonChiTiet> _hoaDonChiTietService;
        private readonly string _frontendUrl;

        public PaymentController(VNPayService vnPayService, IHoaDonService hoaDonService, IBaseService<HoaDonChiTiet> hoaDonChiTietService, IBaseService<SanPhamChiTiet> sanPhamChiTietService, IConfiguration configuration)
        {
            _vnPayService = vnPayService;
            _hoaDonService = hoaDonService;
            _hoaDonChiTietService = hoaDonChiTietService;
            _sanPhamChiTietService = sanPhamChiTietService;
            _frontendUrl = configuration["FrontendUrl"] ?? string.Empty;
        }

        [HttpPost("create-payment")]
        public async Task<IActionResult> CreatePayment([FromBody] PaymentRequest request)
        {
            try
            {
                var hoaDon = await _hoaDonService.GetByIdAsync(request.OrderId);
                if (hoaDon == null)
                {
                    return NotFound("Không tìm thấy hóa đơn");
                }

                if (!hoaDon.tong_tien_phai_thanh_toan.HasValue || hoaDon.tong_tien_phai_thanh_toan <= 0)
                {
                    return BadRequest("Tổng tiền thanh toán không hợp lệ");
                }

                // Store the order code in note field for reference
                hoaDon.ghi_chu = $"VNPay Payment - Order Code: {hoaDon.ma_hoa_don} {hoaDon.ghi_chu}";
                await _hoaDonService.UpdateAsync(hoaDon);

                // Extract numeric part from ma_hoa_don (remove "HD" prefix)
                string numericPart = hoaDon.ma_hoa_don.Replace("HD", "");

                var paymentUrl = _vnPayService.CreatePaymentUrl(
                    long.Parse(numericPart), // Parse string to long
                    (long)hoaDon.tong_tien_phai_thanh_toan.Value,
                    $"Thanh toán đơn hàng {hoaDon.ma_hoa_don}"
                );

                return Ok(new { paymentUrl });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("vnpay-return")]
        public async Task<IActionResult> PaymentReturn()
        {
            try
            {
                var vnpayData = HttpContext.Request.Query
                    .ToDictionary(x => x.Key, x => x.Value.ToString());

                string vnp_SecureHash = vnpayData.FirstOrDefault(x => x.Key == "vnp_SecureHash").Value;

                if (string.IsNullOrEmpty(vnp_SecureHash))
                {
                    return Redirect($"{_frontendUrl}/payment-failed?message=Invalid signature");
                }

                bool validSignature = _vnPayService.ValidateSignature(vnp_SecureHash, vnpayData);
                if (!validSignature)
                {
                    return Redirect($"{_frontendUrl}/payment-failed?message=Invalid signature");
                }

                string vnp_ResponseCode = vnpayData.FirstOrDefault(x => x.Key == "vnp_ResponseCode").Value;
                string vnp_TransactionStatus = vnpayData.FirstOrDefault(x => x.Key == "vnp_TransactionStatus").Value;
                string vnp_TxnRef = vnpayData.FirstOrDefault(x => x.Key == "vnp_TxnRef").Value;
                string vnp_Amount = vnpayData.FirstOrDefault(x => x.Key == "vnp_Amount").Value;
                string vnp_OrderInfo = vnpayData.FirstOrDefault(x => x.Key == "vnp_OrderInfo").Value;

                if (vnp_ResponseCode == "00" && vnp_TransactionStatus == "00")
                {
                    // Find order by transaction ID in ghi_chu field
                    var hoaDon = await _hoaDonService.GetFirstOrDefaultAsync(h =>
                        h.ghi_chu != null &&
                        h.ghi_chu.Contains($"VNPay Transaction ID: {vnp_TxnRef}") &&
                        h.trang_thai_hoa_don == "ChuaThanhToan");

                    if (hoaDon == null)
                    {
                        return Redirect($"{_frontendUrl}/payment-failed?message=Order not found or already processed");
                    }

                    // Kiểm tra số tiền
                    var expectedAmount = (long)(hoaDon.tong_tien_phai_thanh_toan ?? 0) * 100;
                    if (expectedAmount.ToString() != vnp_Amount)
                    {
                        return Redirect($"{_frontendUrl}/payment-failed?message=Invalid amount");
                    }

                    // Load order details
                    var hoaDonWithDetails = await _hoaDonService.GetByIdWithIncludeAsync(hoaDon.id_hoa_don,
                        q => q.Include(hd => hd.HoaDonChiTiets)
                             .ThenInclude(hct => hct.SanPhamChiTiet));

                    // Cập nhật trạng thái đơn hàng
                    hoaDon.trang_thai_hoa_don = "DangChoXuLy";
                    hoaDon.ngay_sua = DateTime.Now;

                    // Cập nhật trạng thái và số lượng sản phẩm
                    if (hoaDonWithDetails?.HoaDonChiTiets != null)
                    {
                        foreach (var chiTiet in hoaDonWithDetails.HoaDonChiTiets)
                        {
                            // Cập nhật số lượng sản phẩm
                            if (chiTiet.SanPhamChiTiet != null)
                            {
                                chiTiet.SanPhamChiTiet.so_luong -= chiTiet.so_luong;
                                await _sanPhamChiTietService.UpdateAsync(chiTiet.SanPhamChiTiet);
                            }

                            chiTiet.trang_thai = "DangChoXuLy";
                            chiTiet.ngay_sua = DateTime.Now;
                            await _hoaDonChiTietService.UpdateAsync(chiTiet);
                        }
                    }

                    await _hoaDonService.UpdateAsync(hoaDon);
                    await _hoaDonService.GuiEmailCapNhatTrangThaiAsync(hoaDon.id_hoa_don, hoaDon.trang_thai_hoa_don);

                    // Chuyển hướng về trang thành công
                    return Redirect($"{_frontendUrl}/payment-success?orderCode={hoaDon.ma_hoa_don}");
                }

                // Thanh toán thất bại
                return Redirect($"{_frontendUrl}/payment-failed?message=Paymentfailed&responseCode={vnp_ResponseCode}");
            }
            catch (Exception ex)
            {
                return Redirect($"{_frontendUrl}/payment-failed?message={WebUtility.UrlEncode(ex.Message)}");
            }
        }

        [HttpGet("vnpay-ipn")]
        public async Task<IActionResult> PaymentNotification()
        {
            var vnpayData = HttpContext.Request.Query
                .ToDictionary(x => x.Key, x => x.Value.ToString());

            string vnp_SecureHash = vnpayData.FirstOrDefault(x => x.Key == "vnp_SecureHash").Value;

            if (string.IsNullOrEmpty(vnp_SecureHash))
            {
                return BadRequest(new { RspCode = "97", Message = "Invalid signature" });
            }

            bool validSignature = _vnPayService.ValidateSignature(vnp_SecureHash, vnpayData);
            if (!validSignature)
            {
                return BadRequest(new { RspCode = "97", Message = "Invalid signature" });
            }

            string vnp_ResponseCode = vnpayData.FirstOrDefault(x => x.Key == "vnp_ResponseCode").Value;
            string vnp_TransactionStatus = vnpayData.FirstOrDefault(x => x.Key == "vnp_TransactionStatus").Value;
            string vnp_TxnRef = vnpayData.FirstOrDefault(x => x.Key == "vnp_TxnRef").Value;
            long vnp_Amount = long.Parse(vnpayData.FirstOrDefault(x => x.Key == "vnp_Amount").Value) / 100;

            // Add "HD" prefix back to find the order
            string orderCode = "HD" + vnp_TxnRef;

            // Find order by order code
            var hoaDon = await _hoaDonService.GetFirstOrDefaultAsync(h => h.ma_hoa_don == orderCode);
            bool orderValid = hoaDon != null;
            bool amountValid = orderValid && hoaDon.tong_tien_phai_thanh_toan == vnp_Amount;

            if (!orderValid)
                return BadRequest(new { RspCode = "01", Message = "Order not found" });

            if (!amountValid)
                return BadRequest(new { RspCode = "04", Message = "Invalid amount" });

            if (vnp_ResponseCode == "00" && vnp_TransactionStatus == "00")
            {
                return Ok(new { RspCode = "00", Message = "Confirm Success" });
            }

            return BadRequest(new { RspCode = "99", Message = "Input data required" });
        }
    }

    public class PaymentRequest
    {
        public Guid OrderId { get; set; }
        public string OrderDescription { get; set; }
    }
}