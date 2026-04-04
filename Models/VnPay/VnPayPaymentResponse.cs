namespace FlashFood.Web.Models.VnPay;

public class VnPayPaymentResponse
{
    public bool IsValidSignature { get; set; }
    public bool IsSuccess { get; set; }
    public int? OrderId { get; set; }
    public decimal Amount { get; set; }
    public string TransactionReference { get; set; } = string.Empty;
    public string ResponseCode { get; set; } = string.Empty;
    public string TransactionStatus { get; set; } = string.Empty;
}
