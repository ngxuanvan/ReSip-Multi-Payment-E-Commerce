namespace ResipWeb.Models.Payments.SePay
{
    public class SePayWebhookDto
    {
        public long id { get; set; }
        public string? gateway { get; set; }
        public string? transactionDate { get; set; }
        public string? accountNumber { get; set; }

        // code thanh toán (nếu SePay nhận diện được)
        public string? code { get; set; }

        // nội dung chuyển khoản
        public string? content { get; set; }

        // "in" / "out"
        public string? transferType { get; set; }

        public decimal transferAmount { get; set; }

        public decimal? accumulated { get; set; }
        public string? subAccount { get; set; }
    }
}
