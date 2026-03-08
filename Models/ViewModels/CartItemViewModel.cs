namespace FlashFood.Web.Models.ViewModels;

public class CartItemViewModel
{
    public int ProductId { get; set; }
    public int? VariantId { get; set; }
    public string? VariantKey { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? VariantName { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public string ImageUrl { get; set; } = string.Empty;

    public decimal Total => UnitPrice * Quantity;
    public string Key => $"{ProductId}:{VariantKey ?? VariantId?.ToString() ?? "none"}";
}
