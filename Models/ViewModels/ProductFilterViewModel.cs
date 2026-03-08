namespace FlashFood.Web.Models.ViewModels;

public class ProductFilterViewModel
{
    public string? SearchTerm { get; set; }
    public int? CategoryId { get; set; }
    public string Sort { get; set; } = "newest";
}


