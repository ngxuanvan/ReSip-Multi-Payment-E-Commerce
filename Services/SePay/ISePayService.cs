namespace ResipWeb.Services.SePay
{
    public interface ISePayService
    {
        string BuildQrImageUrl(string orderCode, decimal amount);
    }
}
