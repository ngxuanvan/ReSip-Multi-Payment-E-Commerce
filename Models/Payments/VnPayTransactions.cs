using System.ComponentModel.DataAnnotations;

namespace ResipWeb.Models.Payments
{
    public class VnPayTransaction
    {
        [Key]
        public long Id { get; set; }

        public string TxnRef { get; set; } = "";              // vnp_TxnRef (mã đơn)
        public long Amount { get; set; }                      // vnp_Amount (đã *100)
        public string BankCode { get; set; } = "";            // vnp_BankCode
        public string BankTranNo { get; set; } = "";          // vnp_BankTranNo
        public string CardType { get; set; } = "";            // vnp_CardType
        public string OrderInfo { get; set; } = "";           // vnp_OrderInfo
        public string PayDate { get; set; } = "";             // vnp_PayDate
        public string TransactionNo { get; set; } = "";       // vnp_TransactionNo (mã GD VNPAY)
        public string ResponseCode { get; set; } = "";        // vnp_ResponseCode
        public string TransactionStatus { get; set; } = "";   // vnp_TransactionStatus
        public string TmnCode { get; set; } = "";             // vnp_TmnCode
        public string SecureHash { get; set; } = "";          // vnp_SecureHash
        public string SecureHashType { get; set; } = "";      // vnp_SecureHashType

        public string Source { get; set; } = "RETURN";        // RETURN / IPN
        public bool IsValidSignature { get; set; }            // kết quả verify
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // link thẳng vào DonHang:
        public string MaDonHang { get; set; } = "";           // = TxnRef (nếu bạn dùng TxnRef=MaDonHang)

        public bool HasReturn { get; set; }
        public bool HasIpn { get; set; }

    }
}
