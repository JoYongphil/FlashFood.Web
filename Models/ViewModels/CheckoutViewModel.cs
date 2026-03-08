using System.ComponentModel.DataAnnotations;

namespace FlashFood.Web.Models.ViewModels;

public class CheckoutViewModel
{
    [Required]
    public string CustomerName { get; set; } = string.Empty;

    [Required]
    [Phone]
    public string Phone { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Province { get; set; } = string.Empty;

    [Required]
    public string District { get; set; } = string.Empty;

    [Required]
    public string Ward { get; set; } = string.Empty;

    [Required]
    public string AddressDetail { get; set; } = string.Empty;

    public string? Note { get; set; }
    public string? PercentVoucherCode { get; set; }
    public string? FreeShipVoucherCode { get; set; }
    public bool SimulatePaid { get; set; }
    public decimal? ManualDistanceKm { get; set; }
}


