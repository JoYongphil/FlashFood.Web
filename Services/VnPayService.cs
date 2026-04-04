using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using FlashFood.Web.Models.Options;
using FlashFood.Web.Models.VnPay;
using Microsoft.Extensions.Options;
using System.Net;

namespace FlashFood.Web.Services;

public interface IVnPayService
{
    bool IsConfigured { get; }
    string CreatePaymentUrl(VnPayPaymentRequest request);
    VnPayPaymentResponse ParseResponse(IQueryCollection query);
}

public class VnPayService(IOptions<VnPayOptions> options) : IVnPayService
{
    private readonly VnPayOptions _options = options.Value;

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(_options.TmnCode) &&
        !string.IsNullOrWhiteSpace(_options.HashSecret) &&
        !string.IsNullOrWhiteSpace(_options.BaseUrl) &&
        !string.IsNullOrWhiteSpace(_options.ReturnUrl);

    public string CreatePaymentUrl(VnPayPaymentRequest request)
    {
        if (!IsConfigured)
        {
            throw new InvalidOperationException("VNPAY has not been configured.");
        }

        var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);
        var txnRef = BuildTransactionReference(request.OrderId);

        var data = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["vnp_Amount"] = ((long)Math.Round(request.Amount * 100m, 0)).ToString(CultureInfo.InvariantCulture),
            ["vnp_Command"] = "pay",
            ["vnp_CreateDate"] = now.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture),
            ["vnp_CurrCode"] = "VND",
            ["vnp_ExpireDate"] = now.AddMinutes(15).ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture),
            ["vnp_IpAddr"] = request.IpAddress,
            ["vnp_Locale"] = "vn",
            ["vnp_OrderInfo"] = request.OrderInfo,
            ["vnp_OrderType"] = "other",
            ["vnp_ReturnUrl"] = _options.ReturnUrl,
            ["vnp_TmnCode"] = _options.TmnCode,
            ["vnp_TxnRef"] = txnRef,
            ["vnp_Version"] = "2.1.0"
        };

        var signData = BuildQueryString(data);
        var secureHash = ComputeHash(signData);
        var paymentUrl = $"{_options.BaseUrl}?{signData}&vnp_SecureHash={VnPayEncode(secureHash)}";

        return paymentUrl;
    }

    public VnPayPaymentResponse ParseResponse(IQueryCollection query)
    {
        var data = new SortedDictionary<string, string>(StringComparer.Ordinal);

        foreach (var item in query)
        {
            if (!item.Key.StartsWith("vnp_", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (item.Key is "vnp_SecureHash" or "vnp_SecureHashType")
            {
                continue;
            }

            var value = item.Value.ToString();
            if (!string.IsNullOrWhiteSpace(value))
            {
                data[item.Key] = value;
            }
        }

        var secureHash = query["vnp_SecureHash"].ToString();
        var signData = BuildQueryString(data);
        var computedHash = ComputeHash(signData);
        var txnRef = GetValue(data, "vnp_TxnRef");
        var orderId = ParseOrderId(txnRef);
        var amountRaw = GetValue(data, "vnp_Amount");
        var responseCode = GetValue(data, "vnp_ResponseCode");
        var transactionStatus = GetValue(data, "vnp_TransactionStatus");

        return new VnPayPaymentResponse
        {
            IsValidSignature = string.Equals(secureHash, computedHash, StringComparison.OrdinalIgnoreCase),
            IsSuccess =
                string.Equals(responseCode, "00", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(transactionStatus, "00", StringComparison.OrdinalIgnoreCase),
            OrderId = orderId,
            Amount = decimal.TryParse(amountRaw, out var parsedAmount) ? parsedAmount / 100m : 0,
            TransactionReference = txnRef,
            ResponseCode = responseCode,
            TransactionStatus = transactionStatus
        };
    }

    private static string BuildTransactionReference(int orderId)
    {
        return $"{orderId}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
    }

    private static int? ParseOrderId(string txnRef)
    {
        if (string.IsNullOrWhiteSpace(txnRef))
        {
            return null;
        }

        var parts = txnRef.Split('_', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return parts.Length > 0 && int.TryParse(parts[0], out var orderId) ? orderId : null;
    }

    private string ComputeHash(string data)
    {
        var keyBytes = Encoding.UTF8.GetBytes(_options.HashSecret);
        var dataBytes = Encoding.UTF8.GetBytes(data);
        using var hmac = new HMACSHA512(keyBytes);
        var hashBytes = hmac.ComputeHash(dataBytes);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    private static string GetValue(IReadOnlyDictionary<string, string> data, string key)
    {
        return data.TryGetValue(key, out var value) ? value : string.Empty;
    }

    private static string BuildQueryString(IEnumerable<KeyValuePair<string, string>> data)
    {
        return string.Join("&", data.Select(x =>
        {
            var key = VnPayEncode(x.Key);
            var value = VnPayEncode(x.Value);
            return $"{key}={value}";
        }));
    }

    private static string VnPayEncode(string value)
    {
        return WebUtility.UrlEncode(value);
    }
}
