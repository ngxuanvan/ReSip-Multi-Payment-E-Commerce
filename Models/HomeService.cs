namespace ResipWeb.Models
{
    public class HomeService
    {
        public int Id { get; set; }

        // Ví dụ: "Dễ dàng đổi trả"
        public string Title { get; set; }

        // Ví dụ: "Trả lại bất kỳ sản phẩm nào trước 15 ngày!"
        public string Description { get; set; }

        // Đường dẫn icon, ví dụ: "assets/uploads/service-5.png"
        public string IconPath { get; set; }
    }
}
