using FlashFood.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlashFood.Web.Controllers;

public class CartController(ICartService cartService) : Controller
{
    public IActionResult Index()
    {
        var items = cartService.GetItems();
        ViewBag.Subtotal = cartService.GetSubtotal();
        return View(items);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult UpdateQuantity(string key, int quantity)
    {
        cartService.UpdateQuantity(key, quantity);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Remove(string key)
    {
        cartService.Remove(key);
        return RedirectToAction(nameof(Index));
    }
}


