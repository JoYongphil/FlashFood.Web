using Microsoft.AspNetCore.Identity;

namespace FlashFood.Web.Models.Entities;

public class AppUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public string? Province { get; set; }
    public string? District { get; set; }
    public string? Ward { get; set; }
}


