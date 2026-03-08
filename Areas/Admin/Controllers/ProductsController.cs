using FlashFood.Web.Data;
using FlashFood.Web.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FlashFood.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class ProductsController(ApplicationDbContext dbContext) : Controller
{
    public async Task<IActionResult> Index(int? categoryId)
    {
        var categories = await dbContext.Categories.OrderBy(x => x.Name).ToListAsync();

        var query = dbContext.Products
            .Include(x => x.Category)
            .AsQueryable();

        if (categoryId.HasValue)
        {
            query = query.Where(x => x.CategoryId == categoryId.Value);
        }

        var products = await query
            .OrderBy(x => x.Name)
            .ToListAsync();

        ViewBag.Categories = categories;
        ViewBag.CurrentCategoryId = categoryId;

        return View("~/Views/AdminProducts/Index.cshtml", products);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Categories = new SelectList(await dbContext.Categories.ToListAsync(), "Id", "Name");
        return View("~/Views/AdminProducts/Create.cshtml", new Product());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Product model, string? variantsRaw)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Categories = new SelectList(await dbContext.Categories.ToListAsync(), "Id", "Name", model.CategoryId);
            return View("~/Views/AdminProducts/Create.cshtml", model);
        }

        var variants = ParseVariantInputs(variantsRaw)
            .Select(x => new ProductVariant
            {
                Name = x.Name,
                AdditionalPrice = x.AdditionalPrice
            })
            .ToList();

        model.Variants = variants;
        dbContext.Products.Add(model);
        await dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { categoryId = model.CategoryId });
    }

    public async Task<IActionResult> Edit(int id)
    {
        var product = await dbContext.Products.Include(x => x.Variants).FirstOrDefaultAsync(x => x.Id == id);
        if (product is null)
        {
            return NotFound();
        }

        ViewBag.Categories = new SelectList(await dbContext.Categories.ToListAsync(), "Id", "Name", product.CategoryId);
        ViewBag.VariantsRaw = string.Join(Environment.NewLine, product.Variants.Select(x => $"{x.Name}|{x.AdditionalPrice}"));

        return View("~/Views/AdminProducts/Edit.cshtml", product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Product model, string? variantsRaw)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Categories = new SelectList(await dbContext.Categories.ToListAsync(), "Id", "Name", model.CategoryId);
            return View("~/Views/AdminProducts/Edit.cshtml", model);
        }

        var existing = await dbContext.Products
            .Include(x => x.Variants)
            .FirstOrDefaultAsync(x => x.Id == model.Id);

        if (existing is null)
        {
            return NotFound();
        }

        existing.Name = model.Name;
        existing.BasePrice = model.BasePrice;
        existing.Description = model.Description;
        existing.ImageUrl = model.ImageUrl;
        existing.CategoryId = model.CategoryId;
        existing.IsAvailable = model.IsAvailable;

        var inputVariants = ParseVariantInputs(variantsRaw);
        var existingVariants = existing.Variants.ToList();

        foreach (var input in inputVariants)
        {
            var matched = existingVariants.FirstOrDefault(x =>
                string.Equals(x.Name, input.Name, StringComparison.OrdinalIgnoreCase));

            if (matched is not null)
            {
                matched.Name = input.Name;
                matched.AdditionalPrice = input.AdditionalPrice;
                continue;
            }

            existing.Variants.Add(new ProductVariant
            {
                Name = input.Name,
                AdditionalPrice = input.AdditionalPrice
            });
        }

        foreach (var variant in existingVariants)
        {
            var stillInInput = inputVariants.Any(x =>
                string.Equals(x.Name, variant.Name, StringComparison.OrdinalIgnoreCase));

            if (stillInInput)
            {
                continue;
            }

            var relatedOrderItems = await dbContext.OrderItems
                .Where(x => x.ProductVariantId == variant.Id)
                .ToListAsync();

            foreach (var item in relatedOrderItems)
            {
                item.ProductVariantId = null;
            }

            dbContext.ProductVariants.Remove(variant);
        }

        await dbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Index), new { categoryId = existing.CategoryId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int? returnCategoryId)
    {
        var product = await dbContext.Products.FirstOrDefaultAsync(x => x.Id == id);
        if (product is not null)
        {
            dbContext.Products.Remove(product);
            await dbContext.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index), new { categoryId = returnCategoryId });
    }

    private static List<VariantInput> ParseVariantInputs(string? raw)
    {
        var result = new List<VariantInput>();
        if (string.IsNullOrWhiteSpace(raw))
        {
            return result;
        }

        foreach (var line in raw.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var parts = line.Split('|', StringSplitOptions.TrimEntries);
            if (parts.Length < 2 || string.IsNullOrWhiteSpace(parts[0]) || !decimal.TryParse(parts[1], out var additional))
            {
                continue;
            }

            var name = parts[0].Trim();
            if (result.Any(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            result.Add(new VariantInput(name, additional));
        }

        return result;
    }

    private sealed record VariantInput(string Name, decimal AdditionalPrice);
}
