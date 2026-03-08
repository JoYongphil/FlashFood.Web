using System.ComponentModel.DataAnnotations;

namespace FlashFood.Web.Models.ViewModels;

public class UserProfileViewModel
{
    [Required(ErrorMessage = "Họ tên là bắt buộc")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
    [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
    public string Phone { get; set; } = string.Empty;

    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tỉnh/Thành là bắt buộc")]
    public string Province { get; set; } = string.Empty;

    [Required(ErrorMessage = "Quận/Huyện là bắt buộc")]
    public string District { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phường/Xã là bắt buộc")]
    public string Ward { get; set; } = string.Empty;

    public string? LastAddressDetail { get; set; }
}
