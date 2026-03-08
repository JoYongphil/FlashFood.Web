namespace FlashFood.Web.Models.ViewModels;

public class DashboardViewModel
{
    public decimal TotalRevenue { get; set; }
    public int TotalOrders { get; set; }
    public string BestSellingProduct { get; set; } = "Chua có d? li?u";
    public int BestSellingQuantity { get; set; }
    public DateTime From { get; set; }
    public DateTime To { get; set; }
}


