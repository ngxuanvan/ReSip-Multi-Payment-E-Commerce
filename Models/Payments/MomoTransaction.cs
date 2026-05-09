using System.ComponentModel.DataAnnotations;

namespace ResipWeb.Models.Payments
{
    public class MomoTransaction
    {
        [Key]
        public int Id { get; set; }

        // Map theo query MoMo trả về
        public string PartnerCode { get; set; } = "";
        public string OrderId { get; set; } = "";        // MaDonHang 
        public string RequestId { get; set; } = "";
        public long Amount { get; set; }
        public string OrderInfo { get; set; } = "";
        public string OrderType { get; set; } = "";
        public long TransId { get; set; }
        public int ResultCode { get; set; }
        public string Message { get; set; } = "";
        public string PayType { get; set; } = "";
        public long ResponseTime { get; set; }
        public string ExtraData { get; set; } = "";
        public string Signature { get; set; } = "";

        // Thêm để quản trị dễ
        public string Source { get; set; } = "RETURN";   // RETURN hoặc IPN
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
