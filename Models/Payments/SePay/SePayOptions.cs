namespace ResipWeb.Models.Payments.SePay
{
    public class SePayOptions
    {
        public string QrBaseUrl { get; set; } = "https://qr.sepay.vn/img";
        public string Bank { get; set; } = "MBBank";
        public string Acc { get; set; } = "";

        public string WebhookUrl { get; set; } = "";

        // ApiKey | SecretKey ( đang dùng ApiKey)
        public string WebhookAuthType { get; set; } = "ApiKey";

        // Khi auth = ApiKey: Header Authorization: "Apikey <WebhookApiKey>"
        public string WebhookApiKey { get; set; } = "";
    }
}
