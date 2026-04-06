#nullable disable

using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FlashFood.Web.Models.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace FlashFood.Web.Areas.Identity.Pages.Account;

public class ExternalLoginModel(
    SignInManager<AppUser> signInManager,
    UserManager<AppUser> userManager,
    ILogger<ExternalLoginModel> logger) : PageModel
{
    [TempData]
    public string ErrorMessage { get; set; }

    public IActionResult OnGet()
    {
        return RedirectToPage("./Login");
    }

    public IActionResult OnPost(string provider, string returnUrl = null)
    {
        var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
        var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return new ChallengeResult(provider, properties);
    }

    public async Task<IActionResult> OnGetCallbackAsync(string returnUrl = null, string remoteError = null)
    {
        returnUrl ??= Url.Content("~/");

        if (!string.IsNullOrWhiteSpace(remoteError))
        {
            ErrorMessage = $"Dang nhap ngoai that bai: {remoteError}";
            return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
        }

        var info = await signInManager.GetExternalLoginInfoAsync();
        if (info is null)
        {
            ErrorMessage = "Khong the tai thong tin dang nhap tu nha cung cap ngoai.";
            return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
        }

        var signInResult = await signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
        if (signInResult.Succeeded)
        {
            var linkedUser = await userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
            if (linkedUser is not null && await userManager.IsInRoleAsync(linkedUser, "Admin"))
            {
                return LocalRedirect("/Admin/Dashboard");
            }

            return LocalRedirect(returnUrl);
        }

        if (signInResult.IsLockedOut)
        {
            return RedirectToPage("./Lockout");
        }

        var email = info.Principal.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrWhiteSpace(email))
        {
            ErrorMessage = "Tai khoan Google khong tra ve email hop le.";
            return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
        }

        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new AppUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FullName = info.Principal.FindFirstValue(ClaimTypes.Name) ?? email.Split('@')[0]
            };

            var createResult = await userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                ErrorMessage = string.Join(" ", createResult.Errors.Select(x => x.Description));
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }
        }
        else if (!user.EmailConfirmed)
        {
            user.EmailConfirmed = true;
            await userManager.UpdateAsync(user);
        }

        var existingLogins = await userManager.GetLoginsAsync(user);
        var isAlreadyLinked = existingLogins.Any(x => x.LoginProvider == info.LoginProvider && x.ProviderKey == info.ProviderKey);
        if (!isAlreadyLinked)
        {
            var addLoginResult = await userManager.AddLoginAsync(user, info);
            if (!addLoginResult.Succeeded)
            {
                ErrorMessage = string.Join(" ", addLoginResult.Errors.Select(x => x.Description));
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }
        }

        await signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);
        logger.LogInformation("User created or linked an account with {LoginProvider}.", info.LoginProvider);

        if (await userManager.IsInRoleAsync(user, "Admin"))
        {
            return LocalRedirect("/Admin/Dashboard");
        }

        return LocalRedirect(returnUrl);
    }
}