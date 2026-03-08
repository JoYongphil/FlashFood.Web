using FlashFood.Web.Data;
using FlashFood.Web.Models.Entities;
using FlashFood.Web.Models.ViewModels;
using FlashFood.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FlashFood.Web.Controllers;

public class ProductsController(ApplicationDbContext dbContext, ICartService cartService, IVoucherService voucherService) : Controller
{
    public async Task<IActionResult> Index(ProductFilterViewModel filter)
    {
        if (User.Identity?.IsAuthenticated == true && User.IsInRole("Admin"))
        {
            return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
        }

        if (User.Identity?.IsAuthenticated == true)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrWhiteSpace(userId))
            {
                await voucherService.EnsureVoucherForUserAsync(userId);
            }
        }

        var query = dbContext.Products
            .Include(x => x.Category)
            .Include(x => x.Reviews)
            .Where(x => x.IsAvailable)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim();
            query = query.Where(x => x.Name.Contains(term));
        }

        if (filter.CategoryId.HasValue)
        {
            query = query.Where(x => x.CategoryId == filter.CategoryId.Value);
        }

        query = filter.Sort switch
        {
            "price_asc" => query.OrderBy(x => x.BasePrice),
            "price_desc" => query.OrderByDescending(x => x.BasePrice),
            _ => query.OrderByDescending(x => x.CreatedAt)
        };

        ViewBag.Categories = await dbContext.Categories.OrderBy(x => x.Name).ToListAsync();
        ViewBag.Filter = filter;

        var products = await query.ToListAsync();
        return View(products);
    }

    public async Task<IActionResult> Details(int id)
    {
        var product = await dbContext.Products
            .Include(x => x.Category)
            .Include(x => x.Variants)
            .Include(x => x.Reviews.OrderByDescending(r => r.CreatedAt))
            .FirstOrDefaultAsync(x => x.Id == id);

        if (product is null)
        {
            return NotFound();
        }

        return View(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddToCart(int productId, int? variantId, int quantity = 1, List<int>? selectedVariantIds = null)
    {
        var product = await dbContext.Products.Include(x => x.Variants).FirstOrDefaultAsync(x => x.Id == productId);
        if (product is null || !product.IsAvailable)
        {
            TempData["Error"] = "Sản phẩm không khả dụng.";
            return RedirectToAction(nameof(Index));
        }

        var addQuantity = Math.Max(1, quantity);
        var selectedIds = selectedVariantIds?
            .Distinct()
            .ToList() ?? new List<int>();

        CartItemViewModel cartItem;

        if (selectedIds.Any())
        {
            var selectedVariants = product.Variants
                .Where(x => selectedIds.Contains(x.Id))
                .OrderBy(x => x.Id)
                .ToList();

            if (!selectedVariants.Any())
            {
                TempData["Error"] = "Biến thể không hợp lệ.";
                return RedirectToAction(nameof(Details), new { id = product.Id });
            }

            var variantKey = string.Join("-", selectedVariants.Select(x => x.Id));
            var variantName = string.Join(", ", selectedVariants.Select(x => x.Name));
            var additionalPrice = selectedVariants.Sum(x => x.AdditionalPrice);

            cartItem = new CartItemViewModel
            {
                ProductId = product.Id,
                VariantId = null,
                VariantKey = variantKey,
                ProductName = product.Name,
                VariantName = variantName,
                UnitPrice = product.BasePrice + additionalPrice,
                Quantity = addQuantity,
                ImageUrl = product.ImageUrl
            };
        }
        else
        {
            var variant = variantId.HasValue ? product.Variants.FirstOrDefault(x => x.Id == variantId.Value) : null;
            cartItem = new CartItemViewModel
            {
                ProductId = product.Id,
                VariantId = variant?.Id,
                VariantKey = null,
                ProductName = product.Name,
                VariantName = variant?.Name,
                UnitPrice = product.BasePrice + (variant?.AdditionalPrice ?? 0),
                Quantity = addQuantity,
                ImageUrl = product.ImageUrl
            };
        }

        cartService.Add(cartItem);

        TempData["Success"] = $"Đã thêm {addQuantity} món vào giỏ hàng.";
        return RedirectToAction(nameof(Details), new { id = product.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddReview(int productId, int rating, string comment)
    {
        var product = await dbContext.Products.FirstOrDefaultAsync(x => x.Id == productId);
        if (product is null)
        {
            return NotFound();
        }

        var reviewerName = User.Identity?.IsAuthenticated == true
            ? User.Identity?.Name ?? "User"
            : "User";

        dbContext.Reviews.Add(new Review
        {
            ProductId = productId,
            Rating = Math.Clamp(rating, 1, 5),
            Comment = comment?.Trim() ?? string.Empty,
            ReviewerName = reviewerName
        });

        await dbContext.SaveChangesAsync();
        TempData["Success"] = "Cảm ơn bạn đã đánh giá.";
        return RedirectToAction(nameof(Details), new { id = productId });
    }
}
