using FlashFood.Web.Data;
using FlashFood.Web.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace FlashFood.Web.Controllers;

[Authorize]
public class OrdersController(ApplicationDbContext dbContext) : Controller
{
    public async Task<IActionResult> Mine()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        var orders = await dbContext.Orders
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return View(orders);
    }

    public async Task<IActionResult> Details(int id)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var order = await dbContext.Orders
            .Include(x => x.Items)
            .ThenInclude(x => x.Product)
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

        if (order is null)
        {
            return NotFound();
        }

        return View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var order = await dbContext.Orders.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

        if (order is not null && (order.Status == OrderStatus.PendingConfirmation || order.Status == OrderStatus.PendingPayment))
        {
            order.Status = OrderStatus.Cancelled;
            await dbContext.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Mine));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmReceived(int id)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var order = await dbContext.Orders.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

        if (order is not null && order.Status == OrderStatus.Delivered)
        {
            order.Status = OrderStatus.Completed;
            await dbContext.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Mine));
    }

    public async Task<IActionResult> Invoice(int id)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var order = await dbContext.Orders
            .Include(x => x.Items)
            .ThenInclude(x => x.Product)
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

        if (order is null)
        {
            return NotFound();
        }

        var stream = new MemoryStream();
        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Size(PageSizes.A4);
                page.Content().Column(column =>
                {
                    column.Item().Text($"HÓA ÐON {order.OrderCode}").Bold().FontSize(20);
                    column.Item().Text($"Th?i gian: {order.CreatedAt:dd/MM/yyyy HH:mm}");
                    column.Item().Text($"Khách hàng: {order.CustomerName}");
                    column.Item().PaddingVertical(10).LineHorizontal(1);

                    foreach (var item in order.Items)
                    {
                        var variant = string.IsNullOrWhiteSpace(item.VariantNameSnapshot)
                            ? string.Empty
                            : $" ({item.VariantNameSnapshot})";
                        column.Item().Text($"- {item.ProductNameSnapshot}{variant} x{item.Quantity}: {item.LineTotal:N0}d");
                    }

                    column.Item().PaddingVertical(10).LineHorizontal(1);
                    column.Item().Text($"T?m tính: {order.Subtotal:N0}d");
                    column.Item().Text($"Gi?m giá: {order.DiscountAmount:N0}d");
                    column.Item().Text($"Phí ship: {order.ShippingFee:N0}d");
                    column.Item().Text($"T?ng ti?n: {order.TotalAmount:N0}d").Bold();
                });
            });
        }).GeneratePdf(stream);

        stream.Position = 0;
        return File(stream.ToArray(), "application/pdf", $"hoa-don-{order.OrderCode}.pdf");
    }
}





