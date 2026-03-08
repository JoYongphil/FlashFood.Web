using FlashFood.Web.Models.Enums;

namespace FlashFood.Web.Models.Entities;

public class Order
{
    public int Id { get; set; }
    public string OrderCode { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public AppUser? User { get; set; }

    public string CustomerName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string Ward { get; set; } = string.Empty;
    public string AddressDetail { get; set; } = string.Empty;
    public string? Note { get; set; }

    public decimal Subtotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal ShippingFee { get; set; }
    public decimal TotalAmount { get; set; }

    public decimal DistanceKm { get; set; }
    public string? PercentVoucherCode { get; set; }
    public string? FreeShipVoucherCode { get; set; }

    public bool IsPaid { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public OrderStatus Status { get; set; } = OrderStatus.PendingConfirmation;

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}


