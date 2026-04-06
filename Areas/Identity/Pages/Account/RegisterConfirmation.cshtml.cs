#nullable disable

using System.Threading.Tasks;
using FlashFood.Web.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FlashFood.Web.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class RegisterConfirmationModel(UserManager<AppUser> userManager) : PageModel
{
    public string Email { get; set; }

    public async Task<IActionResult> OnGetAsync(string email, string returnUrl = null)
    {
        if (email == null)
        {
            return RedirectToPage("/Index");
        }

        returnUrl ??= Url.Content("~/");

        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return NotFound($"Không tìm thấy người dùng với email '{email}'.");
        }

        Email = email;
        return Page();
    }
}

