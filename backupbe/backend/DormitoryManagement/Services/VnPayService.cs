using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Dormitory.Models.Entities;

namespace DormitoryManagement.Services;

public class VnPayService(IConfiguration configuration)
{
    private const string Version = "2.1.0";
    private const string DefaultPaymentUrl = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
    private const string DefaultClientPath = "/";

    public bool IsConfigured
    {
        get
        {
            var tmnCode = configuration["VNPay:TmnCode"];
            var hashSecret = configuration["VNPay:HashSecret"];
            return IsRealValue(tmnCode) && IsRealValue(hashSecret);
        }
    }

    public string CreatePaymentUrl(HttpContext httpContext, Invoices invoice, decimal amount, string clientPath)
    {
        if (!IsConfigured)
        {
            throw new InvalidOperationException("VNPay chưa được cấu hình TmnCode/HashSecret sandbox.");
        }

        if (amount <= 0)
        {
            throw new InvalidOperationException("Số tiền thanh toán phải lớn hơn 0.");
        }

        var now = DateTime.Now;
        var txnRef = $"{invoice.Id:D8}{now:yyyyMMddHHmmssfff}";
        var returnUrl = configuration["VNPay:ReturnUrl"];
        if (string.IsNullOrWhiteSpace(returnUrl))
        {
            returnUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}/api/student-portal/vnpay-return";
        }

        var parameters = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["vnp_Version"] = Version,
            ["vnp_Command"] = "pay",
            ["vnp_TmnCode"] = configuration["VNPay:TmnCode"]!,
            ["vnp_Amount"] = ((long)Math.Round(amount, 0) * 100).ToString(CultureInfo.InvariantCulture),
            ["vnp_CreateDate"] = now.ToString("yyyyMMddHHmmss"),
            ["vnp_CurrCode"] = "VND",
            ["vnp_ExpireDate"] = now.AddMinutes(15).ToString("yyyyMMddHHmmss"),
            ["vnp_IpAddr"] = GetIpAddress(httpContext),
            ["vnp_Locale"] = "vn",
            ["vnp_OrderInfo"] = $"Thanh toan hoa don ky tuc xa {invoice.Id}",
            ["vnp_OrderType"] = "other",
            ["vnp_ReturnUrl"] = returnUrl,
            ["vnp_TxnRef"] = txnRef
        };

        var hashData = BuildHashData(parameters);
        var secureHash = HmacSha512(configuration["VNPay:HashSecret"]!, hashData);
        var paymentUrl = configuration["VNPay:PaymentUrl"] ?? DefaultPaymentUrl;
        var query = BuildQueryString(parameters);
        return $"{paymentUrl}?{query}&vnp_SecureHash={secureHash}";
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

        var parameters = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var item in query)
        {
            if (!item.Key.StartsWith("vnp_", StringComparison.OrdinalIgnoreCase) ||
                item.Key.Equals("vnp_SecureHash", StringComparison.OrdinalIgnoreCase) ||
                item.Key.Equals("vnp_SecureHashType", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var value = item.Value.ToString();
            if (!string.IsNullOrWhiteSpace(value))
            {
                parameters[item.Key] = value;
            }
        }

        var computedHash = HmacSha512(configuration["VNPay:HashSecret"]!, BuildHashData(parameters));
        return string.Equals(computedHash, receivedHash, StringComparison.OrdinalIgnoreCase);
    }

    public int? GetInvoiceIdFromTxnRef(string? txnRef)
    {
        if (string.IsNullOrWhiteSpace(txnRef))
        {
            return null;
        }

        var normalized = txnRef.Trim();
        if (normalized.Length >= 8 &&
            normalized.Take(8).All(char.IsDigit) &&
            int.TryParse(normalized[..8], NumberStyles.Integer, CultureInfo.InvariantCulture, out var paddedInvoiceId))
        {
            return paddedInvoiceId;
        }

        if (normalized.StartsWith("I", StringComparison.OrdinalIgnoreCase))
        {
            var endIndex = normalized.IndexOf('T', StringComparison.OrdinalIgnoreCase);
            normalized = endIndex > 1
                ? normalized[1..endIndex]
                : normalized[1..];
        }
        else if (normalized.Contains('-'))
        {
            normalized = normalized.Split('-', 2)[0];
        }

        return int.TryParse(normalized, NumberStyles.Integer, CultureInfo.InvariantCulture, out var invoiceId)
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

    public string GetClientPath(IQueryCollection query)
    {
        return NormalizeClientPath(query["clientPath"].ToString());
    }

    public string BuildClientReturnUrl(bool success, int? invoiceId, string message, string? clientPath)
    {
        var baseUrl = NormalizeClientPath(clientPath);
        var separator = baseUrl.Contains('?') ? "&" : "?";
        return $"{baseUrl}{separator}paymentStatus={(success ? "success" : "failed")}&invoiceId={invoiceId}&message={WebUtility.UrlEncode(message)}";
    }

    public string BuildClientReturnUrl(bool success, int? invoiceId, string message)
    {
        var configuredPath = configuration["VNPay:ClientReturnUrl"];
        return BuildClientReturnUrl(success, invoiceId, message, configuredPath);
    }

    private static string BuildHashData(SortedDictionary<string, string> parameters)
    {
        return string.Join("&", parameters.Select(item =>
            $"{WebUtility.UrlEncode(item.Key)}={WebUtility.UrlEncode(item.Value).Replace("+", "%20")}"));
    }

    private static string BuildQueryString(SortedDictionary<string, string> parameters)
    {
        return string.Join("&", parameters.Select(item =>
            $"{WebUtility.UrlEncode(item.Key)}={WebUtility.UrlEncode(item.Value).Replace("+", "%20")}"));
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

    private static string NormalizeClientPath(string? clientPath)
    {
        if (string.IsNullOrWhiteSpace(clientPath))
        {
            return DefaultClientPath;
        }

        var normalized = clientPath.Trim();
        if (Uri.TryCreate(normalized, UriKind.Absolute, out var absoluteUri))
        {
            normalized = string.IsNullOrWhiteSpace(absoluteUri.PathAndQuery)
                ? DefaultClientPath
                : absoluteUri.PathAndQuery;
        }

        if (!normalized.StartsWith('/'))
        {
            return DefaultClientPath;
        }

        return normalized.StartsWith("//", StringComparison.Ordinal) ? DefaultClientPath : normalized;
    }

}
