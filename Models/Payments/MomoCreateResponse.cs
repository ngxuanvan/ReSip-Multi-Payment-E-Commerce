namespace ResipWeb.Models.Payments;

public class MomoCreateResponse
{
    public string? payUrl { get; set; } //đường dẫn thanh toán
    public string? deeplink { get; set; } //liên kết thanh toán nhanh
    public int resultCode { get; set; } //mã kết quả 
    public string? message { get; set; } //thông báo kết quả
}
