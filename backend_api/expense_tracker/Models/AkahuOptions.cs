namespace expense_tracker.Models;

public class AkahuOptions
{
    public string AppToken { get; set; } = "";
    public string UserToken { get; set; } = "";
    public string BaseUrl { get; set; } = "https://api.akahu.io/v1";
}
