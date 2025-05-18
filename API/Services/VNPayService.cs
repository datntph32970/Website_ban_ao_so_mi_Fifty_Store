using System.Security.Cryptography;
using System.Text;
using System.Net;
using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using API.DbConects.Entities;
using System.Text.Json;
using System.Net.Http.Headers;

namespace API.Services
{
    public class VNPayService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly VNPayConfig _vnPayConfig;

        public VNPayService(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _httpContextAccessor = httpContextAccessor;
            _vnPayConfig = new VNPayConfig();
            configuration.GetSection("VNPay").Bind(_vnPayConfig);
        }

        public string CreatePaymentUrl(long orderId, long amount, string orderDesc)
        {
            var vnpay = new VNPayLibrary();

            vnpay.AddRequestData("vnp_Version", _vnPayConfig.Version);
            vnpay.AddRequestData("vnp_Command", _vnPayConfig.Command);
            vnpay.AddRequestData("vnp_TmnCode", _vnPayConfig.TmnCode);
            vnpay.AddRequestData("vnp_Amount", (amount * 100).ToString()); // Convert to VND (x100)
            vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", _vnPayConfig.CurrCode);
            vnpay.AddRequestData("vnp_IpAddr", GetIpAddress());
            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", orderDesc);
            vnpay.AddRequestData("vnp_OrderType", "other");
            vnpay.AddRequestData("vnp_ReturnUrl", _vnPayConfig.ReturnUrl);
            vnpay.AddRequestData("vnp_TxnRef", orderId.ToString());

            string paymentUrl = vnpay.CreateRequestUrl(_vnPayConfig.PaymentUrl, _vnPayConfig.HashSecret);

            return paymentUrl;
        }

        public bool ValidateSignature(string inputHash, Dictionary<string, string> vnpayData)
        {
            string rspRaw = GetResponseData(vnpayData);
            string myChecksum = HmacSHA512(_vnPayConfig.HashSecret, rspRaw);
            return myChecksum.Equals(inputHash, StringComparison.InvariantCultureIgnoreCase);
        }

        private string GetResponseData(Dictionary<string, string> vnpayData)
        {
            var data = new StringBuilder();
            if (vnpayData.ContainsKey("vnp_SecureHashType"))
            {
                vnpayData.Remove("vnp_SecureHashType");
            }
            if (vnpayData.ContainsKey("vnp_SecureHash"))
            {
                vnpayData.Remove("vnp_SecureHash");
            }

            foreach (KeyValuePair<string, string> kv in vnpayData.OrderBy(x => x.Key, StringComparer.InvariantCulture))
            {
                if (!string.IsNullOrEmpty(kv.Value))
                {
                    data.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value) + "&");
                }
            }

            if (data.Length > 0)
            {
                data.Remove(data.Length - 1, 1);
            }

            return data.ToString();
        }

        private string HmacSHA512(string key, string inputData)
        {
            var hash = new StringBuilder();
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] inputBytes = Encoding.UTF8.GetBytes(inputData);
            using (var hmac = new HMACSHA512(keyBytes))
            {
                byte[] hashValue = hmac.ComputeHash(inputBytes);
                foreach (var theByte in hashValue)
                {
                    hash.Append(theByte.ToString("x2"));
                }
            }

            return hash.ToString();
        }

        private string GetIpAddress()
        {
            string ipAddress;
            try
            {
                ipAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

                if (string.IsNullOrEmpty(ipAddress))
                    ipAddress = "Unknown";
            }
            catch (Exception ex)
            {
                ipAddress = "Invalid IP:" + ex.Message;
            }

            return ipAddress;
        }

        public class RefundResult
        {
            public bool Success { get; set; }
            public string Message { get; set; }
            public string TransactionId { get; set; }
        }
        // Thêm phương thức này vào VNPayService
        public async Task<RefundResult> RefundPayment(string transactionId, long amount, string description)
        {
            try
            {
                // Log thông tin giao dịch hoàn tiền để debug
                Console.WriteLine($"Bắt đầu yêu cầu hoàn tiền: {transactionId}, Số tiền: {amount}, Mô tả: {description}");

                // Tạo tham số hoàn tiền
                var vnpHashSecret = _vnPayConfig.HashSecret;
                var vnpTmnCode = _vnPayConfig.TmnCode;
                var vnpApiUrl = _vnPayConfig.ApiUrl;
                var vnpVersion = _vnPayConfig.Version;

                var vnpRequestId = DateTime.Now.Ticks.ToString();

                // Sử dụng ngày hiện tại, không dùng UTC để tránh lỗi
                DateTime now = DateTime.Now;
                var vnpCreateDate = now.ToString("yyyyMMddHHmmss");

                // Sử dụng ngày giao dịch gốc hoặc lấy từ transactionId nếu có thể
                // Nếu không, sử dụng ngày hiện tại cho vnp_TransDate
                var vnpTransDate = now.ToString("yyyyMMddHHmmss");

                var vnpIpAddr = GetIpAddress();

                // Tạo dữ liệu gửi sang VNPay
                var data = new Dictionary<string, string>
                {
                    ["vnp_RequestId"] = vnpRequestId,
                    ["vnp_Version"] = vnpVersion,
                    ["vnp_Command"] = "refund",
                    ["vnp_TmnCode"] = vnpTmnCode,
                    ["vnp_TransactionType"] = "02", // Hoàn toàn phần
                    ["vnp_TxnRef"] = transactionId,
                    ["vnp_Amount"] = (amount * 100).ToString(), // Số tiền * 100 (VNPay tính theo đơn vị xu)
                    ["vnp_OrderInfo"] = description,
                    ["vnp_TransDate"] = vnpTransDate,
                    ["vnp_CreateDate"] = vnpCreateDate,
                    ["vnp_IpAddr"] = vnpIpAddr,
                    ["vnp_CreateBy"] = "System"
                };

                // Tạo chuỗi checksum - chỉ sử dụng các tham số không rỗng
                var signData = string.Join("&", data
                    .Where(kv => !string.IsNullOrEmpty(kv.Value))
                    .OrderBy(kv => kv.Key)
                    .Select(kv => $"{kv.Key}={WebUtility.UrlEncode(kv.Value)}"));

                var checksum = HmacSHA512(vnpHashSecret, signData);
                data["vnp_SecureHash"] = checksum;

                // Log dữ liệu gửi đi để debug
                Console.WriteLine($"API URL: {vnpApiUrl}");
                Console.WriteLine($"Dữ liệu gửi: {JsonSerializer.Serialize(data)}");

                // Gọi API VNPay
                using (var client = new HttpClient())
                {
                    // Thiết lập timeout dài hơn
                    client.Timeout = TimeSpan.FromSeconds(30);
                    client.BaseAddress = new Uri(vnpApiUrl);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    // Tạo content với form data
                    var content = new FormUrlEncodedContent(data);

                    try
                    {
                        var response = await client.PostAsync("", content);

                        // Log response status
                        Console.WriteLine($"Status code: {response.StatusCode}");

                        var responseContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"Response content: {responseContent}");

                        if (response.IsSuccessStatusCode)
                        {
                            try
                            {
                                var options = new JsonSerializerOptions
                                {
                                    PropertyNameCaseInsensitive = true
                                };

                                var responseData = JsonSerializer.Deserialize<Dictionary<string, string>>(responseContent, options);

                                // Kiểm tra kết quả từ VNPay
                                if (responseData != null && responseData.TryGetValue("vnp_ResponseCode", out var responseCode) && responseCode == "00")
                                {
                                    return new RefundResult
                                    {
                                        Success = true,
                                        Message = "Hoàn tiền thành công",
                                        TransactionId = responseData.TryGetValue("vnp_TransactionNo", out var transNo) ? transNo : transactionId
                                    };
                                }
                                else
                                {
                                    string errorMsg = "Unknown error";
                                    if (responseData != null)
                                    {
                                        if (responseData.TryGetValue("vnp_Message", out var message))
                                            errorMsg = message;
                                        else if (responseData.TryGetValue("vnp_ResponseCode", out var code))
                                            errorMsg = $"Mã lỗi: {code}";
                                    }

                                    return new RefundResult
                                    {
                                        Success = false,
                                        Message = $"Lỗi từ VNPay: {errorMsg}",
                                        TransactionId = transactionId
                                    };
                                }
                            }
                            catch (JsonException jsonEx)
                            {
                                Console.WriteLine($"Lỗi parse JSON: {jsonEx.Message}");
                                return new RefundResult
                                {
                                    Success = false,
                                    Message = $"Lỗi khi xử lý dữ liệu phản hồi: {jsonEx.Message}",
                                    TransactionId = transactionId
                                };
                            }
                        }
                        else
                        {
                            // Đối với Sandbox, đôi khi có thể trả về 500 nhưng vẫn hoạt động
                            // Trả về thất bại nhưng không phải lỗi hệ thống
                            return new RefundResult
                            {
                                Success = false,
                                Message = $"VNPay từ chối yêu cầu hoàn tiền: {response.StatusCode} - {response.ReasonPhrase}. " +
                                         "Lưu ý: Môi trường sandbox có thể không hỗ trợ đầy đủ API hoàn tiền.",
                                TransactionId = transactionId
                            };
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Lỗi khi gọi API: {e.Message}");
                        if (e.InnerException != null)
                        {
                            Console.WriteLine($"Inner Exception: {e.InnerException.Message}");
                        }

                        return new RefundResult
                        {
                            Success = false,
                            Message = $"Lỗi kết nối đến VNPay: {e.Message}",
                            TransactionId = transactionId
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception trong RefundPayment: {ex}");
                return new RefundResult
                {
                    Success = false,
                    Message = $"Lỗi hệ thống: {ex.Message}",
                    TransactionId = transactionId
                };
            }
        }


    }

    public class VNPayLibrary
    {
        private SortedList<string, string> _requestData = new SortedList<string, string>(new VnPayCompare());
        private SortedList<string, string> _responseData = new SortedList<string, string>(new VnPayCompare());

        public void AddRequestData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _requestData.Add(key, value);
            }
        }

        public string CreateRequestUrl(string baseUrl, string vnp_HashSecret)
        {
            StringBuilder data = new StringBuilder();
            foreach (KeyValuePair<string, string> kv in _requestData)
            {
                if (!string.IsNullOrEmpty(kv.Value))
                {
                    data.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value) + "&");
                }
            }
            string queryString = data.ToString();

            baseUrl += "?" + queryString;
            String signData = queryString;
            if (signData.Length > 0)
            {
                signData = signData.Remove(data.Length - 1, 1);
            }
            string vnp_SecureHash = HmacSHA512(vnp_HashSecret, signData);
            baseUrl += "vnp_SecureHash=" + vnp_SecureHash;

            return baseUrl;
        }

        private string HmacSHA512(string key, string inputData)
        {
            var hash = new StringBuilder();
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] inputBytes = Encoding.UTF8.GetBytes(inputData);
            using (var hmac = new HMACSHA512(keyBytes))
            {
                byte[] hashValue = hmac.ComputeHash(inputBytes);
                foreach (var theByte in hashValue)
                {
                    hash.Append(theByte.ToString("x2"));
                }
            }

            return hash.ToString();
        }
    }

    public class VnPayCompare : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            if (x == y) return 0;
            if (x == null) return -1;
            if (y == null) return 1;
            var vnpCompare = CompareInfo.GetCompareInfo("en-US");
            return vnpCompare.Compare(x, y, CompareOptions.Ordinal);
        }
    }
}