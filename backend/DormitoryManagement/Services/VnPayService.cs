using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Dormitory.Models.Entities;

namespace DormitoryManagement.Services;

public class VnPayService(IConfiguration configuration)
{
    public bool IsConfigured
    {
        get
        {
            var tmnCode = configuration["VNPay:TmnCode"];
            var hashSecret = configuration["VNPay:HashSecret"];
            return IsRealValue(tmnCode) && IsRealValue(hashSecret);
        }
    }

    public string CreatePaymentUrl(HttpContext httpContext, Invoices invoice, decimal amount)
    {
        if (!IsConfigured)
        {
            throw new InvalidOperationException("VNPay chua duoc cau hinh TmnCode/HashSecret sandbox.");
        }

        if (amount <= 0)
        {
            throw new InvalidOperationException("So tien thanh toan phai lon hon 0.");
        }

        var now = DateTime.Now;
        var txnRef = $"{invoice.Id}-{now:yyyyMMddHHmmssfff}";
        var returnUrl = configuration["VNPay:ReturnUrl"];
        if (string.IsNullOrWhiteSpace(returnUrl))
        {
            returnUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}/api/student-portal/vnpay-return";
        }

        var parameters = new SortedDictionary<string, string>
        {
            ["vnp_Version"] = "2.1.0",
            ["vnp_Command"] = "pay",
            ["vnp_TmnCode"] = configuration["VNPay:TmnCode"]!,
            ["vnp_Amount"] = ((long)Math.Round(amount, 0) * 100).ToString(CultureInfo.InvariantCulture),
            ["vnp_CreateDate"] = now.ToString("yyyyMMddHHmmss"),
            ["vnp_CurrCode"] = "VND",
            ["vnp_IpAddr"] = GetIpAddress(httpContext),
            ["vnp_Locale"] = "vn",
            ["vnp_OrderInfo"] = $"Thanh toan hoa don {invoice.InvoiceCode}",
            ["vnp_OrderType"] = "billpayment",
            ["vnp_ReturnUrl"] = returnUrl,
            ["vnp_TxnRef"] = txnRef
        };

        var hashData = BuildQuery(parameters);
        var secureHash = HmacSha512(configuration["VNPay:HashSecret"]!, hashData);
        var paymentUrl = configuration["VNPay:PaymentUrl"] ?? "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
        return $"{paymentUrl}?{hashData}&vnp_SecureHash={secureHash}";
    }

    public bool ValidateReturn(IQueryCollection query)
    {
        if (!IsConfigured)
        {
            return false;
        }

        var receivedHash = query["vnp_SecureHash"].ToString();
        if (string.IsNullOrWhiteSpace(receivedHash))
        {
            return false;
        }

        var parameters = new SortedDictionary<string, string>();
        foreach (var item in query)
        {
            if (item.Key.Equals("vnp_SecureHash", StringComparison.OrdinalIgnoreCase) ||
                item.Key.Equals("vnp_SecureHashType", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!string.IsNullOrWhiteSpace(item.Value.ToString()))
            {
                parameters[item.Key] = item.Value.ToString();
            }
        }

        var computedHash = HmacSha512(configuration["VNPay:HashSecret"]!, BuildQuery(parameters));
        return string.Equals(computedHash, receivedHash, StringComparison.OrdinalIgnoreCase);
    }

    public int? GetInvoiceIdFromTxnRef(string? txnRef)
    {
        if (string.IsNullOrWhiteSpace(txnRef))
        {
            return null;
        }

        var firstPart = txnRef.Split('-', 2)[0];
        return int.TryParse(firstPart, NumberStyles.Integer, CultureInfo.InvariantCulture, out var invoiceId)
            ? invoiceId
            : null;
    }

    public decimal GetPaidAmount(IQueryCollection query)
    {
        var rawAmount = query["vnp_Amount"].ToString();
        return decimal.TryParse(rawAmount, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount)
            ? amount / 100
            : 0;
    }

    public string BuildClientReturnUrl(bool success, int? invoiceId, string message)
    {
        var baseUrl = configuration["VNPay:ClientReturnUrl"] ?? "/";
        var separator = baseUrl.Contains('?') ? "&" : "?";
        return $"{baseUrl}{separator}paymentStatus={(success ? "success" : "failed")}&invoiceId={invoiceId}&message={WebUtility.UrlEncode(message)}";
    }

    private static string BuildQuery(SortedDictionary<string, string> parameters)
    {
        return string.Join("&", parameters.Select(item =>
            $"{WebUtility.UrlEncode(item.Key)}={WebUtility.UrlEncode(item.Value)}"));
    }

    private static string HmacSha512(string key, string inputData)
    {
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var inputBytes = Encoding.UTF8.GetBytes(inputData);
        using var hmac = new HMACSHA512(keyBytes);
        var hash = hmac.ComputeHash(inputBytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static string GetIpAddress(HttpContext context)
    {
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
    }

    private static bool IsRealValue(string? value)
    {
        return !string.IsNullOrWhiteSpace(value) &&
            !value.StartsWith("YOUR_", StringComparison.OrdinalIgnoreCase) &&
            !value.StartsWith("CHANGE_ME", StringComparison.OrdinalIgnoreCase);
    }
}
