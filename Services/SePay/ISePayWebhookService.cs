using ResipWeb.Models.Payments.SePay;

namespace ResipWeb.Services.SePay
{
    public interface ISePayWebhookService
    {
        Task HandleAsync(SePayWebhookDto payload, string? authorizationHeader);
    }
}
