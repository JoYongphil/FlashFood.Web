#nullable disable

using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading.Tasks;
using FlashFood.Web.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace FlashFood.Web.Areas.Identity.Pages.Account;

public class ForgotPasswordModel(UserManager<AppUser> userManager) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; }

    public class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = await userManager.FindByEmailAsync(Input.Email);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Email không tồn tại trong hệ thống.");
            return Page();
        }

        var code = await userManager.GeneratePasswordResetTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

        return RedirectToPage(
            "/Account/ResetPassword",
            new { area = "Identity", code, email = Input.Email });
    }
}
