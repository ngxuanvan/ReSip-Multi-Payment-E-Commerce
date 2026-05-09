using System.ComponentModel.DataAnnotations;

namespace ResipWeb.Models.Payments
{
    public class PayPalTransaction
    {
        [Key]
        public int Id { get; set; }

        public string OrderId { get; set; } = "";        // MaDonHang (để tìm kiếm khớp với đơn hàng)
        public string PayPalOrderId { get; set; } = "";  // Mã giao dịch từ PayPal (Capture ID)
        public string Token { get; set; } = "";          // Token (Order ID từ PayPal)
        public string PayerID { get; set; } = "";        // Mã định danh người mua
        public string PayerEmail { get; set; } = "";     // Email người mua (để tìm kiếm)
        public string AmountUsd { get; set; } = "";      // Số tiền USD thực tế
        public string Status { get; set; } = "PENDING";  // Trạng thái giao dịch (COMPLETED, FAILED...)
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
