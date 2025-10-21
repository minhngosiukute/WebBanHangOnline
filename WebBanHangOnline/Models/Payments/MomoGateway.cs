using Newtonsoft.Json;
using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace WebBanHangOnline.Models.Payments
{
    // Cấu hình MoMo đọc từ web.config
    public class MomoOptions
    {
        public string ApiCreate { get; set; }
        public string PartnerCode { get; set; }
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public string ReturnUrl { get; set; }
        public string IpnUrl { get; set; }

        public static MomoOptions FromConfig()
        {
            return new MomoOptions
            {
                ApiCreate = ConfigurationManager.AppSettings["momo_ApiCreate"],
                PartnerCode = ConfigurationManager.AppSettings["momo_PartnerCode"],
                AccessKey = ConfigurationManager.AppSettings["momo_AccessKey"],
                SecretKey = ConfigurationManager.AppSettings["momo_SecretKey"],
                ReturnUrl = ConfigurationManager.AppSettings["momo_ReturnUrl"],
                IpnUrl = ConfigurationManager.AppSettings["momo_IpnUrl"],
            };
        }
    }

    // ===== DTOs =====
    public class MomoCreateRequest
    {
        public string partnerCode { get; set; }
        public string partnerName { get; set; }
        public string storeId { get; set; }
        public string requestType { get; set; } // "captureWallet"
        public string ipnUrl { get; set; }
        public string redirectUrl { get; set; }
        public string orderId { get; set; }
        public string amount { get; set; }
        public string lang { get; set; }
        public string orderInfo { get; set; }
        public string requestId { get; set; }
        public string extraData { get; set; }
        public string signature { get; set; }
    }
    public class MomoCreateResponse
    {
        public int resultCode { get; set; }     // 0 = ok
        public string message { get; set; }
        public string payUrl { get; set; }
        public string deeplink { get; set; }
        public string qrCodeUrl { get; set; }
        public string signature { get; set; }
    }
    public class MomoIpnPayload
    {
        public string partnerCode { get; set; }
        public string accessKey { get; set; }
        public string orderId { get; set; }
        public string requestId { get; set; }
        public long amount { get; set; }
        public string orderInfo { get; set; }
        public string orderType { get; set; }
        public long transId { get; set; }
        public int resultCode { get; set; }
        public string message { get; set; }
        public string payType { get; set; }
        public long responseTime { get; set; }
        public string extraData { get; set; }
        public string signature { get; set; }
    }

    // ===== Gateway =====
    public class MomoGateway
    {
        private static readonly HttpClient _http = new HttpClient();
        private readonly MomoOptions _opt;

        public MomoGateway(MomoOptions opt)
        {
            _opt = opt ?? throw new ArgumentNullException(nameof(opt));
        }

        // Tạo link thanh toán
        public string CreatePaymentUrl(string orderCode, long totalAmountVnd, string partnerName = "YourStore", string storeId = "YourStoreId")
        {
            var requestId = Guid.NewGuid().ToString("N");
            var requestType = "captureWallet";
            var amountStr = totalAmountVnd.ToString();
            var orderInfo = "Thanh toan don hang: " + orderCode;
            var extraData = ""; // có thể base64 nếu cần

            var rawSignature =
                $"accessKey={_opt.AccessKey}&amount={amountStr}&extraData={extraData}&ipnUrl={_opt.IpnUrl}&orderId={orderCode}&orderInfo={orderInfo}&partnerCode={_opt.PartnerCode}&redirectUrl={_opt.ReturnUrl}&requestId={requestId}&requestType={requestType}";
            var signature = HmacSHA256(rawSignature, _opt.SecretKey);

            var payload = new MomoCreateRequest
            {
                partnerCode = _opt.PartnerCode,
                partnerName = partnerName,
                storeId = storeId,
                requestType = requestType,
                ipnUrl = _opt.IpnUrl,
                redirectUrl = _opt.ReturnUrl,
                orderId = orderCode,
                amount = amountStr,
                lang = "vi",
                orderInfo = orderInfo,
                requestId = requestId,
                extraData = extraData,
                signature = signature
            };

            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var res = _http.PostAsync(_opt.ApiCreate, content).Result;
            var resJson = res.Content.ReadAsStringAsync().Result;
            var data = JsonConvert.DeserializeObject<MomoCreateResponse>(resJson);

            if (data != null && data.resultCode == 0 && !string.IsNullOrEmpty(data.payUrl))
                return data.payUrl;

            throw new Exception("MoMo create error: " + resJson);
        }

        // Xác thực chữ ký cho Return/IPN
        public bool VerifySignature(string rawSignature, string receivedSignature)
            => HmacSHA256(rawSignature, _opt.SecretKey) == receivedSignature;

        // Build rawSignature cho Return
        public string BuildReturnRawSignature(NameValueCollection q)
        {
            string NV(string v) => v ?? "";
            return $"accessKey={_opt.AccessKey}" +
                   $"&amount={NV(q["amount"])}" +
                   $"&extraData={NV(q["extraData"])}" +
                   $"&message={NV(q["message"])}" +
                   $"&orderId={NV(q["orderId"])}" +
                   $"&orderInfo={NV(q["orderInfo"])}" +
                   $"&orderType={NV(q["orderType"])}" +
                   $"&partnerCode={NV(q["partnerCode"])}" +
                   $"&payType={NV(q["payType"])}" +
                   $"&requestId={NV(q["requestId"])}" +
                   $"&responseTime={NV(q["responseTime"])}" +
                   $"&resultCode={NV(q["resultCode"])}" +
                   $"&transId={NV(q["transId"])}";
        }

        // Build rawSignature cho IPN
        public string BuildIpnRawSignature(MomoIpnPayload p)
        {
            string NV(string v) => v ?? "";
            return $"accessKey={_opt.AccessKey}" +
                   $"&amount={p.amount}" +
                   $"&extraData={NV(p.extraData)}" +
                   $"&message={NV(p.message)}" +
                   $"&orderId={NV(p.orderId)}" +
                   $"&orderInfo={NV(p.orderInfo)}" +
                   $"&orderType={NV(p.orderType)}" +
                   $"&partnerCode={NV(p.partnerCode)}" +
                   $"&payType={NV(p.payType)}" +
                   $"&requestId={NV(p.requestId)}" +
                   $"&responseTime={p.responseTime}" +
                   $"&resultCode={p.resultCode}" +
                   $"&transId={p.transId}";
        }

        // Helpers
        private static string HmacSHA256(string rawData, string secretKey)
        {
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey)))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }
    }
}
