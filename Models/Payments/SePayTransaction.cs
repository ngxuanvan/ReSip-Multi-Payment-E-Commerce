namespace ResipWeb.Models.Payments.SePay
{
    public class SePayTransaction
    {
        public int Id { get; set; }

        public long SepayTxnId { get; set; } // payload.id
        public string? OrderCode { get; set; }

        public decimal Amount { get; set; }
        public string? Content { get; set; }
        public string? TransferType { get; set; }
        public string? TransactionDate { get; set; }
        public string? RawGateway { get; set; }

        public string? Note { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
