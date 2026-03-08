using FlashFood.Web.Data;
using FlashFood.Web.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FlashFood.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class CategoriesController(ApplicationDbContext dbContext) : Controller
{
    public async Task<IActionResult> Index()
    {
        var categories = await dbContext.Categories.OrderBy(x => x.Name).ToListAsync();
        return View("~/Views/AdminCategories/Index.cshtml", categories);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string name, string description, string? iconSvg)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return RedirectToAction(nameof(Index));
        }

        dbContext.Categories.Add(new Category
        {
            Name = name.Trim(),
            Description = description?.Trim() ?? string.Empty,
            IconSvg = string.IsNullOrWhiteSpace(iconSvg) ? null : iconSvg.Trim()
        });

        await dbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int id, string name, string description, string? iconSvg)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return RedirectToAction(nameof(Index));
        }

        var category = await dbContext.Categories.FirstOrDefaultAsync(x => x.Id == id);
        if (category is not null)
        {
            category.Name = name.Trim();
            category.Description = description?.Trim() ?? string.Empty;
            category.IconSvg = string.IsNullOrWhiteSpace(iconSvg) ? null : iconSvg.Trim();
            await dbContext.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await dbContext.Categories.FirstOrDefaultAsync(x => x.Id == id);
        if (category is not null)
        {
            dbContext.Categories.Remove(category);
            await dbContext.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}
