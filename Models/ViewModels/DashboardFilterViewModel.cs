namespace FlashFood.Web.Models.ViewModels;

public class DashboardFilterViewModel
{
    public string Period { get; set; } = "month";
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
