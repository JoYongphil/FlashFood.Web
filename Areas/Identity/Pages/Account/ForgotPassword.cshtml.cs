#nullable disable

using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using FlashFood.Web.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace FlashFood.Web.Areas.Identity.Pages.Account;

public class ForgotPasswordModel(UserManager<AppUser> userManager, IEmailSender emailSender) : PageModel
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
            ModelState.AddModelError(string.Empty, "Email khong ton tai trong he thong.");
            return Page();
        }

        if (!await userManager.IsEmailConfirmedAsync(user))
        {
            ModelState.AddModelError(string.Empty, "Email nay chua xac thuc, vui long xac nhan email truoc khi dat lai mat khau.");
            return Page();
        }

        var code = await userManager.GeneratePasswordResetTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        var callbackUrl = Url.Page(
            "/Account/ResetPassword",
            pageHandler: null,
            values: new { area = "Identity", code, email = Input.Email },
            protocol: Request.Scheme);

        await emailSender.SendEmailAsync(
            Input.Email,
            "Dat lai mat khau",
            $"Ban co the dat lai mat khau bang cach <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>nhan vao day</a>.");

        return RedirectToPage("./ForgotPasswordConfirmation");
    }
}
