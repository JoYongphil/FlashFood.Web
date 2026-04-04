using System.Text.Json;
using FlashFood.Web.Data;
using FlashFood.Web.Models.Entities;
using FlashFood.Web.Models.Enums;
using FlashFood.Web.Models.ViewModels;
using FlashFood.Web.Models.VnPay;
using FlashFood.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FlashFood.Web.Controllers;

public class CheckoutController(
    ApplicationDbContext dbContext,
    ICartService cartService,
    IGoogleDistanceService distanceService,
    IVoucherService voucherService,
    UserManager<AppUser> userManager,
    IVnPayService vnPayService) : Controller
{
    private const string GuestOrderSessionKey = "FLASHFOOD_GUEST_ORDER_IDS";

    public async Task<IActionResult> Index()
    {
        var cart = cartService.GetItems();
        if (!cart.Any())
        {
            TempData["Error"] = "Gio hang trong.";
            return RedirectToAction("Index", "Products");
        }

        await LoadVoucherViewDataAsync();

        var model = await BuildPrefillCheckoutModelAsync();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(CheckoutViewModel model)
    {
        var cart = cartService.GetItems();
        if (!cart.Any())
        {
            TempData["Error"] = "Gio hang trong.";
            return RedirectToAction("Index", "Products");
        }

        if (!vnPayService.IsConfigured)
        {
            ModelState.AddModelError(string.Empty, "VNPAY chua duoc cau hinh. Hay cap nhat VnPay trong appsettings.json truoc khi thanh toan.");
        }

        if (!ModelState.IsValid)
        {
            await LoadVoucherViewDataAsync();
            return View(model);
        }

        var address = $"{model.AddressDetail}, {model.Ward}, {model.District}, {model.Province}";
        var distanceKm = await distanceService.GetDistanceInKmAsync(address, model.ManualDistanceKm);

        if (!distanceKm.HasValue)
        {
            ModelState.AddModelError(string.Empty, "Khong tinh duoc khoang cach. Hay nhap khoang cach thu cong.");
            await LoadVoucherViewDataAsync();
            return View(model);
        }

        if (distanceKm.Value > 10)
        {
            ModelState.AddModelError(string.Empty, "Khoang cach tren 10km, he thong khong ho tro giao hang.");
            await LoadVoucherViewDataAsync();
            return View(model);
        }

        var subtotal = cart.Sum(x => x.Total);
        var shippingFee = distanceKm.Value <= 5 ? 15000 : 30000;
        decimal discountAmount = 0;

        Voucher? percentVoucher = null;
        Voucher? freeShipVoucher = null;

        if (User.Identity?.IsAuthenticated == true)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrWhiteSpace(userId))
            {
                await voucherService.EnsureVoucherForUserAsync(userId);
                percentVoucher = await voucherService.ValidateVoucherAsync(model.PercentVoucherCode ?? string.Empty, userId, VoucherType.Percentage);
                freeShipVoucher = await voucherService.ValidateVoucherAsync(model.FreeShipVoucherCode ?? string.Empty, userId, VoucherType.FreeShip);

                await UpdateUserProfileFromCheckoutAsync(userId, model);
            }
        }
        else if (!string.IsNullOrWhiteSpace(model.PercentVoucherCode) || !string.IsNullOrWhiteSpace(model.FreeShipVoucherCode))
        {
            ModelState.AddModelError(string.Empty, "Khach chua dang nhap khong duoc ap dung voucher.");
            await LoadVoucherViewDataAsync();
            return View(model);
        }

        if (percentVoucher is not null)
        {
            discountAmount += subtotal * (percentVoucher.Value / 100);
        }

        if (freeShipVoucher is not null)
        {
            shippingFee = 0;
        }

        var total = Math.Max(0, subtotal - discountAmount + shippingFee);

        var order = new Order
        {
            OrderCode = $"FF{DateTime.UtcNow:yyyyMMddHHmmss}{Random.Shared.Next(100, 999)}",
            UserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
            CustomerName = model.CustomerName,
            Phone = model.Phone,
            Email = model.Email,
            Province = model.Province,
            District = model.District,
            Ward = model.Ward,
            AddressDetail = model.AddressDetail,
            Note = model.Note,
            DistanceKm = distanceKm.Value,
            Subtotal = subtotal,
            DiscountAmount = Math.Round(discountAmount, 0),
            ShippingFee = shippingFee,
            TotalAmount = Math.Round(total, 0),
            Status = OrderStatus.PendingPayment,
            IsPaid = false,
            PercentVoucherCode = percentVoucher?.Code,
            FreeShipVoucherCode = freeShipVoucher?.Code,
            Items = cart.Select(x => new OrderItem
            {
                ProductId = x.ProductId,
                ProductVariantId = x.VariantId,
                ProductNameSnapshot = x.ProductName,
                VariantNameSnapshot = x.VariantName,
                UnitPrice = x.UnitPrice,
                Quantity = x.Quantity,
                LineTotal = x.Total
            }).ToList()
        };

        dbContext.Orders.Add(order);

        if (percentVoucher is not null)
        {
            percentVoucher.IsUsed = true;
        }

        if (freeShipVoucher is not null)
        {
            freeShipVoucher.IsUsed = true;
        }

        await dbContext.SaveChangesAsync();
        cartService.Clear();

        if (User.Identity?.IsAuthenticated != true)
        {
            SaveGuestOrderToSession(order.Id);
        }

        return Redirect(BuildPaymentUrl(order));
    }

    public async Task<IActionResult> Success(int id)
    {
        var order = await GetAccessibleOrderAsync(id);
        if (order is null)
        {
            return NotFound();
        }

        return View(order);
    }

    public async Task<IActionResult> ContinuePayment(int id)
    {
        var order = await GetAccessibleOrderAsync(id);
        if (order is null)
        {
            return NotFound();
        }

        if (order.IsPaid)
        {
            return RedirectToAction(nameof(Success), new { id });
        }

        if (order.Status == OrderStatus.Cancelled || order.Status == OrderStatus.Completed)
        {
            TempData["Error"] = "Don hang nay khong con kha dung de thanh toan lai.";
            return RedirectToOrderDetails(order.Id);
        }

        if (!vnPayService.IsConfigured)
        {
            TempData["Error"] = "VNPAY chua duoc cau hinh. Hay cap nhat VnPay trong appsettings.json.";
            return RedirectToOrderDetails(order.Id);
        }

        order.Status = OrderStatus.PendingPayment;
        await dbContext.SaveChangesAsync();

        return Redirect(BuildPaymentUrl(order));
    }

    [HttpGet("/payment/vnpay-return")]
    public async Task<IActionResult> VnPayReturn()
    {
        if (!vnPayService.IsConfigured)
        {
            TempData["Error"] = "VNPAY chua duoc cau hinh.";
            return RedirectToAction("Index", "Products");
        }

        var result = vnPayService.ParseResponse(Request.Query);
        if (!result.IsValidSignature || !result.OrderId.HasValue)
        {
            TempData["Error"] = "Khong xac thuc duoc phan hoi tu VNPAY.";
            return RedirectToAction("Index", "Products");
        }

        var order = await GetAccessibleOrderAsync(result.OrderId.Value);
        if (order is null)
        {
            return NotFound();
        }

        var isAmountValid = IsOrderAmountValid(order, result.Amount);
        if (result.IsValidSignature && isAmountValid)
        {
            await ApplyPaymentResultAsync(order, result.IsSuccess);
        }

        TempData[result.IsSuccess && isAmountValid ? "Success" : "Error"] =
            result.IsSuccess && isAmountValid
                ? "Thanh toan VNPAY thanh cong."
                : "Thanh toan chua thanh cong. Ban co the thu thanh toan lai.";

        return RedirectToAction(nameof(Success), new { id = order.Id });
    }

    [HttpGet("/payment/vnpay-ipn")]
    public async Task<IActionResult> VnPayIpn()
    {
        if (!vnPayService.IsConfigured)
        {
            return Json(new { RspCode = "99", Message = "Configuration invalid" });
        }

        var result = vnPayService.ParseResponse(Request.Query);
        if (!result.IsValidSignature)
        {
            return Json(new { RspCode = "97", Message = "Invalid signature" });
        }

        if (!result.OrderId.HasValue)
        {
            return Json(new { RspCode = "01", Message = "Order not found" });
        }

        var order = await dbContext.Orders.FirstOrDefaultAsync(x => x.Id == result.OrderId.Value);
        if (order is null)
        {
            return Json(new { RspCode = "01", Message = "Order not found" });
        }

        if (!IsOrderAmountValid(order, result.Amount))
        {
            return Json(new { RspCode = "04", Message = "Invalid amount" });
        }

        if (order.IsPaid)
        {
            return Json(new { RspCode = "02", Message = "Order already confirmed" });
        }

        await ApplyPaymentResultAsync(order, result.IsSuccess);
        return Json(new { RspCode = "00", Message = "Confirm success" });
    }

    public async Task<IActionResult> GuestOrders()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Mine", "Orders");
        }

        var trackedIds = GetGuestOrderIdsFromSession();
        var orders = trackedIds.Any()
            ? await dbContext.Orders
                .Where(x => x.UserId == null && trackedIds.Contains(x.Id))
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync()
            : new List<Order>();

        return View(orders);
    }

    public async Task<IActionResult> GuestOrderDetails(int id)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Details", "Orders", new { id });
        }

        var trackedIds = GetGuestOrderIdsFromSession();
        if (!trackedIds.Contains(id))
        {
            TempData["Error"] = "Khong tim thay don trong phien lam viec hien tai.";
            return RedirectToAction(nameof(GuestOrders));
        }

        var order = await dbContext.Orders
            .Include(x => x.Items)
            .ThenInclude(x => x.Product)
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == null);

        if (order is null)
        {
            TempData["Error"] = "Khong tim thay don hang.";
            return RedirectToAction(nameof(GuestOrders));
        }

        return View(order);
    }

    private string BuildPaymentUrl(Order order)
    {
        return vnPayService.CreatePaymentUrl(new VnPayPaymentRequest
        {
            OrderId = order.Id,
            OrderCode = order.OrderCode,
            Amount = order.TotalAmount,
            OrderInfo = $"Thanh toan don hang {order.OrderCode}",
            IpAddress = GetClientIpAddress()
        });
    }

    private string GetClientIpAddress()
    {
        return HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "127.0.0.1";
    }

    private async Task ApplyPaymentResultAsync(Order order, bool isSuccess)
    {
        if (isSuccess)
        {
            order.IsPaid = true;
            if (order.Status == OrderStatus.PendingPayment)
            {
                order.Status = OrderStatus.PendingConfirmation;
            }
        }
        else if (!order.IsPaid && order.Status != OrderStatus.Cancelled && order.Status != OrderStatus.Completed)
        {
            order.Status = OrderStatus.PendingPayment;
        }

        await dbContext.SaveChangesAsync();
    }

    private static bool IsOrderAmountValid(Order order, decimal amount)
    {
        return Math.Round(order.TotalAmount, 0) == Math.Round(amount, 0);
    }

    private IActionResult RedirectToOrderDetails(int orderId)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Details", "Orders", new { id = orderId });
        }

        return RedirectToAction(nameof(GuestOrderDetails), new { id = orderId });
    }

    private async Task<Order?> GetAccessibleOrderAsync(int id)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return await dbContext.Orders
                .Include(x => x.Items)
                .ThenInclude(x => x.Product)
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
        }

        var trackedIds = GetGuestOrderIdsFromSession();
        if (!trackedIds.Contains(id))
        {
            return null;
        }

        return await dbContext.Orders
            .Include(x => x.Items)
            .ThenInclude(x => x.Product)
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == null);
    }

    private async Task<CheckoutViewModel> BuildPrefillCheckoutModelAsync()
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return new CheckoutViewModel();
        }

        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return new CheckoutViewModel();
        }

        var lastOrder = await dbContext.Orders
            .Where(x => x.UserId == user.Id)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new { x.AddressDetail })
            .FirstOrDefaultAsync();

        return new CheckoutViewModel
        {
            CustomerName = user.FullName,
            Phone = user.PhoneNumber ?? string.Empty,
            Email = user.Email ?? string.Empty,
            Province = user.Province ?? string.Empty,
            District = user.District ?? string.Empty,
            Ward = user.Ward ?? string.Empty,
            AddressDetail = lastOrder?.AddressDetail ?? string.Empty
        };
    }

    private async Task UpdateUserProfileFromCheckoutAsync(string userId, CheckoutViewModel model)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (user is null)
        {
            return;
        }

        user.FullName = model.CustomerName.Trim();
        user.PhoneNumber = model.Phone.Trim();
        user.Province = model.Province.Trim();
        user.District = model.District.Trim();
        user.Ward = model.Ward.Trim();

        await dbContext.SaveChangesAsync();
    }

    private async Task LoadVoucherViewDataAsync()
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            ViewBag.ActiveVouchers = new List<Voucher>();
            return;
        }

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userId))
        {
            ViewBag.ActiveVouchers = new List<Voucher>();
            return;
        }

        await voucherService.EnsureVoucherForUserAsync(userId);
        ViewBag.ActiveVouchers = await voucherService.GetActiveVouchersAsync(userId);
    }

    private void SaveGuestOrderToSession(int orderId)
    {
        var ids = GetGuestOrderIdsFromSession();
        if (!ids.Contains(orderId))
        {
            ids.Insert(0, orderId);
        }

        var session = HttpContext.Session;
        session.SetString(GuestOrderSessionKey, JsonSerializer.Serialize(ids));
    }

    private List<int> GetGuestOrderIdsFromSession()
    {
        var session = HttpContext.Session;
        var json = session.GetString(GuestOrderSessionKey);
        if (string.IsNullOrWhiteSpace(json))
        {
            return new List<int>();
        }

        return JsonSerializer.Deserialize<List<int>>(json) ?? new List<int>();
    }
}
