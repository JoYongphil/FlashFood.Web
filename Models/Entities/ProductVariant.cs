namespace FlashFood.Web.Models.Entities;

public class ProductVariant
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal AdditionalPrice { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }
}


