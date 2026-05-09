using System.ComponentModel.DataAnnotations;

namespace ResipWeb.Models
{
    public class CauHoiThuongGap
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Câu hỏi")]
        [Required(ErrorMessage = "Vui lòng nhập câu hỏi")]
        public string CauHoi { get; set; }

        [Display(Name = "Câu trả lời")]
        [Required(ErrorMessage = "Vui lòng nhập câu trả lời")]
        public string CauTraLoi { get; set; }

        [Display(Name = "Thứ tự hiển thị")]
        public int ThuTu { get; set; }
    }
}