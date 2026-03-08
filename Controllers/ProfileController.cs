using FlashFood.Web.Data;
using FlashFood.Web.Models.Entities;
using FlashFood.Web.Models.ViewModels;
using FlashFood.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FlashFood.Web.Controllers;

[Authorize]
public class ProfileController(
    UserManager<AppUser> userManager,
    ApplicationDbContext dbContext,
    IVoucherService voucherService) : Controller
{
    public async Task<IActionResult> Index()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Challenge();
        }

        var model = await BuildModelAsync(user);
        await LoadVoucherViewDataAsync(user.Id);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(UserProfileViewModel model)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Challenge();
        }

        if (!ModelState.IsValid)
        {
            await LoadVoucherViewDataAsync(user.Id);
            return View(model);
        }

        user.FullName = model.FullName.Trim();
        user.PhoneNumber = model.Phone.Trim();
        user.Province = model.Province.Trim();
        user.District = model.District.Trim();
        user.Ward = model.Ward.Trim();

        await userManager.UpdateAsync(user);

        TempData["Success"] = "Đã cập nhật thông tin cá nhân.";

        var refreshed = await BuildModelAsync(user);
        await LoadVoucherViewDataAsync(user.Id);
        return View(refreshed);
    }

    private async Task<UserProfileViewModel> BuildModelAsync(AppUser user)
    {
        var lastAddressDetail = await dbContext.Orders
            .Where(x => x.UserId == user.Id)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => x.AddressDetail)
            .FirstOrDefaultAsync();

        return new UserProfileViewModel
        {
            FullName = user.FullName,
            Phone = user.PhoneNumber ?? string.Empty,
            Email = user.Email ?? string.Empty,
            Province = user.Province ?? string.Empty,
            District = user.District ?? string.Empty,
            Ward = user.Ward ?? string.Empty,
            LastAddressDetail = lastAddressDetail
        };
    }

    private async Task LoadVoucherViewDataAsync(string userId)
    {
        await voucherService.EnsureVoucherForUserAsync(userId);
        ViewBag.ActiveVouchers = await voucherService.GetActiveVouchersAsync(userId);
    }
}
