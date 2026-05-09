# ReSip - Multi-Payment E-Commerce

Chào mừng bạn đến với **ReSip**, dự án website thương mại điện tử chuyên cung cấp các sản phẩm bình nước Silicon cao cấp. Dự án được phát triển bằng ASP.NET Core MVC với trọng tâm là tích hợp đa dạng các cổng thanh toán hiện đại.

## 🚀 Tính năng chính
*   **Quản lý sản phẩm**: Hiển thị danh mục sản phẩm (bình nước, ống hút...), chi tiết sản phẩm với Slider ảnh chuyên nghiệp.
*   **Hệ thống thanh toán đa kênh**:
    *   **MoMo**: Tích hợp thanh toán qua QR Code và ví điện tử.
    *   **VNPay**: Hỗ trợ thẻ ATM nội địa và ngân hàng số.
    *   **PayPal**: Thanh toán quốc tế qua thẻ tín dụng.
    *   **COD**: Thanh toán khi nhận hàng.
*   **Giỏ hàng & Đơn hàng**: Quy trình checkout tối ưu, quản lý lịch sử đơn hàng cho người dùng.
*   **Quản trị (Admin)**: Quản lý sản phẩm, đơn hàng, tin tức và cấu hình website dễ dàng.
*   **Hệ thống Blog/Tin tức**: Cập nhật thông tin hữu ích cho khách hàng.

## 🛠 Công nghệ sử dụng
*   **Backend**: ASP.NET Core MVC (.NET 6/7/8)
*   **Database**: SQL Server / Entity Framework Core
*   **Frontend**: HTML5, CSS3, JavaScript, Bootstrap, jQuery, Owl Carousel
*   **Payment APIs**: MoMo API, VNPay API, PayPal SDK

## 📋 Hướng dẫn cài đặt
1.  **Clone dự án**:
    ```bash
    git clone https://github.com/ngxuanvan/ReSip-Multi-Payment-E-Commerce.git
    ```
2.  **Cấu hình Database**: 
    Cập nhật chuỗi kết nối (ConnectionString) trong file `appsettings.json`.
3.  **Cấu hình API Thanh toán**:
    Cập nhật các thông tin MerchantId, AccessKey, SecretKey của MoMo, VNPay, PayPal trong `appsettings.json`.
4.  **Chạy dự án**:
    Mở file `.sln` bằng Visual Studio và nhấn `F5` hoặc chạy lệnh `dotnet run`.

## 📸 Ảnh chụp màn hình
*(Bạn có thể bổ sung ảnh chụp giao diện vào đây sau)*

---
Được phát triển bởi **Nguyễn Xuân Văn**
