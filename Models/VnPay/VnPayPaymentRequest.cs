namespace FlashFood.Web.Models.VnPay;

public class VnPayPaymentRequest
{
    public int OrderId { get; set; }
    public string OrderCode { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string OrderInfo { get; set; } = string.Empty;
    public string IpAddress { get; set; } = "127.0.0.1";
}
