using System.Text.Json;
using FlashFood.Web.Models.ViewModels;

namespace FlashFood.Web.Services;

public interface ICartService
{
    List<CartItemViewModel> GetItems();
    void Add(CartItemViewModel item);
    void UpdateQuantity(string key, int quantity);
    void Remove(string key);
    void Clear();
    decimal GetSubtotal();
}

public class SessionCartService(IHttpContextAccessor httpContextAccessor) : ICartService
{
    private const string CartSessionKey = "FLASHFOOD_CART";

    public List<CartItemViewModel> GetItems()
    {
        var session = httpContextAccessor.HttpContext?.Session;
        var json = session?.GetString(CartSessionKey);

        return string.IsNullOrWhiteSpace(json)
            ? new List<CartItemViewModel>()
            : JsonSerializer.Deserialize<List<CartItemViewModel>>(json) ?? new List<CartItemViewModel>();
    }

    public void Add(CartItemViewModel item)
    {
        var items = GetItems();
        var existing = items.FirstOrDefault(x => x.Key == item.Key);
        if (existing is not null)
        {
            existing.Quantity += item.Quantity;
        }
        else
        {
            items.Add(item);
        }

        Save(items);
    }

    public void UpdateQuantity(string key, int quantity)
    {
        var items = GetItems();
        var existing = items.FirstOrDefault(x => x.Key == key);
        if (existing is null)
        {
            return;
        }

        if (quantity <= 0)
        {
            items.Remove(existing);
        }
        else
        {
            existing.Quantity = quantity;
        }

        Save(items);
    }

    public void Remove(string key)
    {
        var items = GetItems();
        items.RemoveAll(x => x.Key == key);
        Save(items);
    }

    public void Clear() => Save(new List<CartItemViewModel>());

    public decimal GetSubtotal() => GetItems().Sum(x => x.Total);

    private void Save(List<CartItemViewModel> items)
    {
        var session = httpContextAccessor.HttpContext?.Session;
        if (session is null)
        {
            return;
        }

        session.SetString(CartSessionKey, JsonSerializer.Serialize(items));
    }
}


