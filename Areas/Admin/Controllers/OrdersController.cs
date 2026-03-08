using System.Globalization;
using FlashFood.Web.Data;
using FlashFood.Web.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FlashFood.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class OrdersController(ApplicationDbContext dbContext) : Controller
{
    public async Task<IActionResult> Index(string? orderCode, string? fromDateTime, string? toDateTime, string? amountSort)
    {
        var query = dbContext.Orders
            .Include(x => x.Items)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(orderCode))
        {
            orderCode = orderCode.Trim();
            query = query.Where(x => x.OrderCode.Contains(orderCode));
            ViewBag.OrderCode = orderCode;
        }
        else
        {
            ViewBag.OrderCode = string.Empty;
        }

        DateTime? from = null;
        DateTime? to = null;

        if (!string.IsNullOrWhiteSpace(fromDateTime)
            && DateTime.TryParseExact(fromDateTime, "yyyy-MM-ddTHH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedFrom))
        {
            from = parsedFrom;
            ViewBag.FromDateTime = parsedFrom.ToString("yyyy-MM-ddTHH:mm");
        }
        else
        {
            ViewBag.FromDateTime = string.Empty;
        }

        if (!string.IsNullOrWhiteSpace(toDateTime)
            && DateTime.TryParseExact(toDateTime, "yyyy-MM-ddTHH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedTo))
        {
            to = parsedTo;
            ViewBag.ToDateTime = parsedTo.ToString("yyyy-MM-ddTHH:mm");
        }
        else
        {
            ViewBag.ToDateTime = string.Empty;
        }

        if (from.HasValue && to.HasValue && from.Value > to.Value)
        {
            (from, to) = (to, from);
            ViewBag.FromDateTime = from.Value.ToString("yyyy-MM-ddTHH:mm");
            ViewBag.ToDateTime = to.Value.ToString("yyyy-MM-ddTHH:mm");
        }

        if (from.HasValue)
        {
            query = query.Where(x => x.CreatedAt >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(x => x.CreatedAt <= to.Value);
        }

        amountSort = amountSort?.Trim().ToLowerInvariant();
        query = amountSort switch
        {
            "asc" => query.OrderBy(x => x.TotalAmount).ThenByDescending(x => x.CreatedAt),
            "desc" => query.OrderByDescending(x => x.TotalAmount).ThenByDescending(x => x.CreatedAt),
            _ => query.OrderByDescending(x => x.CreatedAt)
        };

        ViewBag.AmountSort = amountSort switch
        {
            "asc" => "asc",
            "desc" => "desc",
            _ => string.Empty
        };

        var orders = await query.ToListAsync();
        return View("~/Views/AdminOrders/Index.cshtml", orders);
    }


    public async Task<IActionResult> NonCompleted()
    {
        var orders = await dbContext.Orders
            .Where(x => x.Status != OrderStatus.Completed)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync();

        return View("~/Views/AdminOrders/NonCompleted.cshtml", orders);
    }
    public async Task<IActionResult> Details(int id)
    {
        var order = await dbContext.Orders
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (order is null)
        {
            return NotFound();
        }

        ViewBag.Statuses = Enum.GetValues<OrderStatus>()
            .Select(x => new SelectListItem
            {
                Value = ((int)x).ToString(),
                Text = GetStatusLabel(x)
            })
            .ToList();

        return View("~/Views/AdminOrders/Details.cshtml", order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, OrderStatus status)
    {
        var order = await dbContext.Orders.FirstOrDefaultAsync(x => x.Id == id);
        if (order is not null)
        {
            order.Status = status;
            await dbContext.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteReview(int id)
    {
        var review = await dbContext.Reviews.FirstOrDefaultAsync(x => x.Id == id);
        if (review is not null)
        {
            dbContext.Reviews.Remove(review);
            await dbContext.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    private static string GetStatusLabel(OrderStatus status) => status switch
    {
        OrderStatus.PendingConfirmation => "Chờ xác nhận",
        OrderStatus.Preparing => "Đang làm",
        OrderStatus.Delivering => "Đang giao",
        OrderStatus.Delivered => "Đã giao",
        OrderStatus.Completed => "Hoàn thành",
        OrderStatus.Cancelled => "Hủy đơn",
        _ => status.ToString()
    };
}

