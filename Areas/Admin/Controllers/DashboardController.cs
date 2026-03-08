using FlashFood.Web.Data;
using FlashFood.Web.Models.Enums;
using FlashFood.Web.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FlashFood.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class DashboardController(ApplicationDbContext dbContext) : Controller
{
    public async Task<IActionResult> Index(DashboardFilterViewModel filter)
    {
        filter.Period = string.IsNullOrWhiteSpace(filter.Period) ? "month" : filter.Period;

        var localTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, localTimeZone);

        var (fromLocal, toLocal) = filter.Period switch
        {
            "day" => (nowLocal.Date, nowLocal.Date.AddDays(1).AddTicks(-1)),
            "year" => (new DateTime(nowLocal.Year, 1, 1), new DateTime(nowLocal.Year + 1, 1, 1).AddTicks(-1)),
            _ => (new DateTime(nowLocal.Year, nowLocal.Month, 1), new DateTime(nowLocal.Year, nowLocal.Month, 1).AddMonths(1).AddTicks(-1))
        };

        if (filter.FromDate.HasValue && filter.ToDate.HasValue)
        {
            fromLocal = filter.FromDate.Value.Date;
            toLocal = filter.ToDate.Value.Date.AddDays(1).AddTicks(-1);
        }

        if (toLocal < fromLocal)
        {
            (fromLocal, toLocal) = (toLocal, fromLocal);
        }

        var fromUtc = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(fromLocal, DateTimeKind.Unspecified), localTimeZone);
        var toUtc = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(toLocal, DateTimeKind.Unspecified), localTimeZone);

        var query = dbContext.Orders.Where(x =>
            x.CreatedAt >= fromUtc &&
            x.CreatedAt <= toUtc &&
            x.Status != OrderStatus.Cancelled);

        var totalRevenue = await query.SumAsync(x => (decimal?)x.TotalAmount) ?? 0;
        var totalOrders = await query.CountAsync();

        var bestItem = await dbContext.OrderItems
            .Where(x =>
                x.Order != null &&
                x.Order.CreatedAt >= fromUtc &&
                x.Order.CreatedAt <= toUtc &&
                x.Order.Status != OrderStatus.Cancelled)
            .GroupBy(x => x.ProductNameSnapshot)
            .Select(g => new { Name = g.Key, Qty = g.Sum(x => x.Quantity) })
            .OrderByDescending(x => x.Qty)
            .FirstOrDefaultAsync();

        var nonCompletedOrders = await dbContext.Orders
            .Where(x => x.Status != OrderStatus.Completed && x.Status != OrderStatus.Cancelled)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync();

        var model = new DashboardViewModel
        {
            TotalRevenue = totalRevenue,
            TotalOrders = totalOrders,
            BestSellingProduct = bestItem?.Name ?? "Chưa có dữ liệu",
            BestSellingQuantity = bestItem?.Qty ?? 0,
            From = fromLocal,
            To = toLocal
        };

        ViewBag.Filter = filter;
        ViewBag.NonCompletedOrders = nonCompletedOrders;
        return View("~/Views/AdminDashboard/Index.cshtml", model);
    }
}

