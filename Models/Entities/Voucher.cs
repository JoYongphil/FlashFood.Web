using FlashFood.Web.Models.Enums;

namespace FlashFood.Web.Models.Entities;

public class Voucher
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public VoucherType Type { get; set; }
    public decimal Value { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }

    public string? UserId { get; set; }
    public AppUser? User { get; set; }
}


