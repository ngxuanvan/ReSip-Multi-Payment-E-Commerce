namespace ResipWeb.Models.Payments;

public class MomoCreateRequest
{
    public string partnerCode { get; set; } = default!;
    public string partnerName { get; set; } = "Resip";
    public string storeId { get; set; } = "ResipStore";
    public string requestId { get; set; } = default!;
    public long amount { get; set; }
    public string orderId { get; set; } = default!;
    public string orderInfo { get; set; } = default!;
    public string redirectUrl { get; set; } = default!;
    public string ipnUrl { get; set; } = default!;
    public string lang { get; set; } = "vi";
    public string extraData { get; set; } = "";
    public string requestType { get; set; } = "payWithATM"; //payWithCC //F
    public string signature { get; set; } = default!;
}
