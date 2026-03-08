#nullable disable

using System.Text;
using System.Threading.Tasks;
using FlashFood.Web.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace FlashFood.Web.Areas.Identity.Pages.Account;

public class ConfirmEmailModel(UserManager<AppUser> userManager) : PageModel
{
    [TempData]
    public string StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(string userId, string code)
    {
        if (userId == null || code == null)
        {
            return RedirectToPage("/Index");
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound($"Không tìm thấy người dùng với ID '{userId}'.");
        }

        code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
        var result = await userManager.ConfirmEmailAsync(user, code);
        StatusMessage = result.Succeeded ? "Xác nhận email thành công." : "Xác nhận email thất bại.";
        return Page();
    }
}
