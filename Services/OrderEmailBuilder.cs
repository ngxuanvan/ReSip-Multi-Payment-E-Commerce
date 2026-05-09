using System.Text;
using ResipWeb.Models;

namespace ResipWeb.Services
{
    public static class OrderEmailBuilder
    {
        public static string BuildOrderEmailHtml(
            DonHang donHang,
            List<GioHang> cartItems,
            decimal tongThanhToan,
            decimal phiShip,
            string phuongThucThanhToan = "COD")
        {
            var sb = new StringBuilder();
            sb.Append($@"
<!DOCTYPE html>
<html lang='vi'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Xác nhận đơn hàng</title>
</head>
<body style='margin: 0; padding: 0; font-family: Arial, sans-serif; background-color: #f4f4f4;'>
    <table width='100%' cellpadding='0' cellspacing='0' style='background-color: #f4f4f4; padding: 20px 0;'>
        <tr>
            <td align='center'>
                <table width='600' cellpadding='0' cellspacing='0' style='background-color: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 2px 8px rgba(0,0,0,0.1);'>
                    
                    <!-- Header với gradient xanh lá và xanh dương -->
                    <tr>
                        <td style='background: linear-gradient(135deg, #4CAF50 0%, #00BCD4 100%); padding: 40px 30px; text-align: center;'>
                            <h1 style='margin: 0; color: #ffffff; font-size: 28px; font-weight: bold;'>
                                <span style='color: #4CAF50; background-color: #ffffff; padding: 2px 8px; border-radius: 4px;'>Re</span><span style='color: #00BCD4; background-color: #ffffff; padding: 2px 8px; border-radius: 4px;'>Sip</span>
                            </h1>
                            <p style='margin: 15px 0 0 0; color: #ffffff; font-size: 16px;'>Xác nhận đơn hàng</p>
                        </td>
                    </tr>
                    
                    <!-- Thông báo thành công -->
                    <tr>
                        <td style='padding: 30px; text-align: center;'>
                            <div style='display: inline-block; background-color: #E8F5E9; border-radius: 50%; width: 80px; height: 80px; line-height: 80px; margin-bottom: 20px;'>
                                <span style='color: #4CAF50; font-size: 50px;'>✓</span>
                            </div>
                            <h2 style='margin: 0 0 10px 0; color: #333333; font-size: 24px;'>Đặt hàng thành công!</h2>
                            <p style='margin: 0; color: #666666; font-size: 14px;'>Cảm ơn bạn đã tin tưởng và mua hàng tại Resip</p>
                        </td>
                    </tr>
                    
                    <!-- Thông tin đơn hàng -->
                    <tr>
                        <td style='padding: 0 30px 20px 30px;'>
                            <table width='100%' cellpadding='12' cellspacing='0' style='background-color: #f8f9fa; border-radius: 6px;'>
                                <tr>
                                    <td style='border-bottom: 1px solid #e0e0e0;'>
                                        <table width='100%'>
                                            <tr>
                                                <td style='color: #666666; font-size: 14px;'>Mã đơn hàng:</td>
                                                <td align='right' style='color: #00BCD4; font-weight: bold; font-size: 16px;'>{donHang.MaDonHang}</td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                                <tr>
                                    <td style='border-bottom: 1px solid #e0e0e0;'>
                                        <table width='100%'>
                                            <tr>
                                                <td style='color: #666666; font-size: 14px;'>Người nhận:</td>
                                                <td align='right' style='color: #333333; font-weight: bold; font-size: 14px;'>{donHang.HoTen}</td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                                <tr>
                                    <td style='border-bottom: 1px solid #e0e0e0;'>
                                        <table width='100%'>
                                            <tr>
                                                <td style='color: #666666; font-size: 14px;'>Điện thoại:</td>
                                                <td align='right' style='color: #333333; font-weight: bold; font-size: 14px;'>{donHang.DienThoai}</td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                                <tr>
                                    <td style='border-bottom: 1px solid #e0e0e0;'>
                                        <table width='100%'>
                                            <tr>
                                                <td style='color: #666666; font-size: 14px; vertical-align: top;'>Địa chỉ:</td>
                                                <td align='right' style='color: #333333; font-weight: bold; font-size: 14px; max-width: 300px;'>{donHang.DiaChi}</td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <table width='100%'>
                                            <tr>
                                                <td style='color: #666666; font-size: 14px;'>Phương thức thanh toán:</td>
                                                <td align='right' style='color: #4CAF50; font-weight: bold; font-size: 14px;'>{GetPhuongThucThanhToanLabel(phuongThucThanhToan)}</td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    
                    <!-- Chi tiết sản phẩm -->
                    <tr>
                        <td style='padding: 20px 30px;'>
                            <h3 style='margin: 0 0 15px 0; color: #333333; font-size: 18px; border-left: 4px solid #4CAF50; padding-left: 10px;'>Chi tiết đơn hàng</h3>
                            <table width='100%' cellpadding='12' cellspacing='0' style='border: 1px solid #e0e0e0; border-radius: 6px;'>
                                <thead>
                                    <tr style='background: linear-gradient(135deg, #4CAF50 0%, #00BCD4 100%);'>
                                        <th style='color: #ffffff; font-size: 14px; text-align: left; padding: 15px 12px;'>Sản phẩm</th>
                                        <th style='color: #ffffff; font-size: 14px; text-align: center; padding: 15px 12px;'>SL</th>
                                        <th style='color: #ffffff; font-size: 14px; text-align: right; padding: 15px 12px;'>Đơn giá</th>
                                        <th style='color: #ffffff; font-size: 14px; text-align: right; padding: 15px 12px;'>Thành tiền</th>
                                    </tr>
                                </thead>
                                <tbody>
");

            foreach (var i in cartItems)
            {
                var thanhTien = i.SoLuong * i.SanPham.GiaBan;
                sb.Append($@"
                                    <tr style='border-bottom: 1px solid #e0e0e0;'>
                                        <td style='color: #333333; font-size: 14px; padding: 12px;'>{i.SanPham.TenSanPham}</td>
                                        <td style='color: #666666; font-size: 14px; text-align: center; padding: 12px;'>{i.SoLuong}</td>
                                        <td style='color: #666666; font-size: 14px; text-align: right; padding: 12px;'>{i.SanPham.GiaBan:n0}₫</td>
                                        <td style='color: #333333; font-weight: bold; font-size: 14px; text-align: right; padding: 12px;'>{thanhTien:n0}₫</td>
                                    </tr>
");
            }

            var tongSanPham = tongThanhToan - phiShip;
            sb.Append($@"
                                </tbody>
                            </table>
                        </td>
                    </tr>
                    
                    <!-- Tổng thanh toán -->
                    <tr>
                        <td style='padding: 0 30px 30px 30px;'>
                            <table width='100%' cellpadding='10' cellspacing='0' style='background-color: #f8f9fa; border-radius: 6px;'>
                                <tr>
                                    <td style='color: #666666; font-size: 14px; padding: 10px 15px;'>Tổng tiền hàng:</td>
                                    <td align='right' style='color: #333333; font-size: 14px; padding: 10px 15px;'>{tongSanPham:n0}₫</td>
                                </tr>
                                <tr>
                                    <td style='color: #666666; font-size: 14px; padding: 10px 15px; border-bottom: 1px solid #e0e0e0;'>Phí vận chuyển:</td>
                                    <td align='right' style='color: #333333; font-size: 14px; padding: 10px 15px; border-bottom: 1px solid #e0e0e0;'>{phiShip:n0}₫</td>
                                </tr>
                                <tr>
                                    <td style='color: #333333; font-size: 16px; font-weight: bold; padding: 15px;'>Tổng thanh toán:</td>
                                    <td align='right' style='background: linear-gradient(135deg, #4CAF50 0%, #00BCD4 100%); -webkit-background-clip: text; -webkit-text-fill-color: transparent; background-clip: text; font-size: 20px; font-weight: bold; padding: 15px;'>{tongThanhToan:n0}₫</td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    
                    <!-- Lưu ý -->
                    <tr>
                        <td style='padding: 0 30px 30px 30px;'>
                            <div style='background-color: #E3F2FD; border-left: 4px solid #00BCD4; padding: 15px; border-radius: 4px;'>
                                <p style='margin: 0 0 10px 0; color: #333333; font-size: 14px; font-weight: bold;'>📋 Lưu ý:</p>
                                <p style='margin: 0; color: #666666; font-size: 13px; line-height: 1.6;'>
                                    • Đơn hàng của bạn đang được xử lý<br/>
                                    • Chúng tôi sẽ liên hệ với bạn trong thời gian sớm nhất<br/>
                                    • Vui lòng kiểm tra email thường xuyên để cập nhật tình trạng đơn hàng
                                </p>
                            </div>
                        </td>
                    </tr>
                    
                    <!-- Footer -->
                    <tr>
                        <td style='background-color: #333333; padding: 30px; text-align: center;'>
                            <p style='margin: 0 0 10px 0; color: #ffffff; font-size: 16px; font-weight: bold;'>
                                <span style='color: #4CAF50;'>Re</span><span style='color: #00BCD4;'>Sip</span>
                            </p>
                            <p style='margin: 0 0 15px 0; color: #999999; font-size: 13px;'>
                                Cung cấp những sản phẩm gọn gàng, tiện ích và thân thiện môi trường
                            </p>
                            <div style='margin: 15px 0;'>
                                <a href='#' style='color: #4CAF50; text-decoration: none; margin: 0 10px; font-size: 13px;'>Chính sách đổi trả</a>
                                <span style='color: #666666;'>|</span>
                                <a href='#' style='color: #4CAF50; text-decoration: none; margin: 0 10px; font-size: 13px;'>Liên hệ hỗ trợ</a>
                                <span style='color: #666666;'>|</span>
                                <a href='#' style='color: #4CAF50; text-decoration: none; margin: 0 10px; font-size: 13px;'>Theo dõi đơn hàng</a>
                            </div>
                            <p style='margin: 15px 0 0 0; color: #666666; font-size: 12px;'>
                                © 2026 Resip. All rights reserved.
                            </p>
                        </td>
                    </tr>
                    
                </table>
            </td>
        </tr>
    </table>
</body>
</html>
");
            return sb.ToString();
        }

        // =====================================================
        // Email thông báo đơn hàng mới cho Admin
        // =====================================================
        public static string BuildAdminNotifyEmailHtml(
            DonHang donHang,
            List<GioHang> cartItems,
            decimal tongThanhToan,
            decimal phiShip,
            string phuongThucThanhToan = "COD")
        {
            var tongSanPham = tongThanhToan - phiShip;
            var sb = new StringBuilder();
            sb.Append($@"
<!DOCTYPE html>
<html lang='vi'>
<head>
    <meta charset='UTF-8'>
    <title>Đơn hàng mới - {donHang.MaDonHang}</title>
</head>
<body style='margin: 0; padding: 0; font-family: Arial, sans-serif; background-color: #f4f4f4;'>
    <table width='100%' cellpadding='0' cellspacing='0' style='background-color: #f4f4f4; padding: 20px 0;'>
        <tr>
            <td align='center'>
                <table width='600' cellpadding='0' cellspacing='0' style='background-color: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 2px 8px rgba(0,0,0,0.1);'>

                    <!-- Header -->
                    <tr>
                        <td style='background: linear-gradient(135deg, #FF6F00 0%, #FF8F00 100%); padding: 30px; text-align: center;'>
                            <h1 style='margin: 0; color: #ffffff; font-size: 22px;'>🛒 Đơn hàng mới cần xử lý</h1>
                            <p style='margin: 10px 0 0 0; color: #ffffff; font-size: 15px;'>Mã đơn: <strong>{donHang.MaDonHang}</strong></p>
                        </td>
                    </tr>

                    <!-- Thông tin khách hàng -->
                    <tr>
                        <td style='padding: 25px 30px 10px 30px;'>
                            <h3 style='margin: 0 0 15px 0; color: #333333; font-size: 16px; border-left: 4px solid #FF6F00; padding-left: 10px;'>Thông tin khách hàng</h3>
                            <table width='100%' cellpadding='10' cellspacing='0' style='background-color: #f8f9fa; border-radius: 6px;'>
                                <tr><td style='color:#666;font-size:14px;border-bottom:1px solid #e0e0e0;'>Họ tên:</td><td align='right' style='color:#333;font-weight:bold;font-size:14px;border-bottom:1px solid #e0e0e0;'>{donHang.HoTen}</td></tr>
                                <tr><td style='color:#666;font-size:14px;border-bottom:1px solid #e0e0e0;'>Email:</td><td align='right' style='color:#333;font-weight:bold;font-size:14px;border-bottom:1px solid #e0e0e0;'>{donHang.Email}</td></tr>
                                <tr><td style='color:#666;font-size:14px;border-bottom:1px solid #e0e0e0;'>Điện thoại:</td><td align='right' style='color:#333;font-weight:bold;font-size:14px;border-bottom:1px solid #e0e0e0;'>{donHang.DienThoai}</td></tr>
                                <tr><td style='color:#666;font-size:14px;border-bottom:1px solid #e0e0e0;vertical-align:top;'>Địa chỉ:</td><td align='right' style='color:#333;font-weight:bold;font-size:14px;border-bottom:1px solid #e0e0e0;'>{donHang.DiaChi}</td></tr>
                                <tr><td style='color:#666;font-size:14px;'>Phương thức TT:</td><td align='right' style='color:#FF6F00;font-weight:bold;font-size:14px;'>{GetPhuongThucThanhToanLabel(phuongThucThanhToan)}</td></tr>
                            </table>
                        </td>
                    </tr>

                    <!-- Chi tiết sản phẩm -->
                    <tr>
                        <td style='padding: 20px 30px;'>
                            <h3 style='margin: 0 0 15px 0; color: #333333; font-size: 16px; border-left: 4px solid #FF6F00; padding-left: 10px;'>Chi tiết đơn hàng</h3>
                            <table width='100%' cellpadding='10' cellspacing='0' style='border: 1px solid #e0e0e0; border-radius: 6px;'>
                                <thead>
                                    <tr style='background: linear-gradient(135deg, #FF6F00 0%, #FF8F00 100%);'>
                                        <th style='color:#fff;font-size:13px;text-align:left;padding:12px;'>Sản phẩm</th>
                                        <th style='color:#fff;font-size:13px;text-align:center;padding:12px;'>SL</th>
                                        <th style='color:#fff;font-size:13px;text-align:right;padding:12px;'>Đơn giá</th>
                                        <th style='color:#fff;font-size:13px;text-align:right;padding:12px;'>Thành tiền</th>
                                    </tr>
                                </thead>
                                <tbody>
");
            foreach (var i in cartItems)
            {
                var thanhTien = i.SoLuong * i.SanPham.GiaBan;
                sb.Append($@"
                                    <tr style='border-bottom: 1px solid #e0e0e0;'>
                                        <td style='color:#333;font-size:13px;padding:10px;'>{i.SanPham.TenSanPham}</td>
                                        <td style='color:#666;font-size:13px;text-align:center;padding:10px;'>{i.SoLuong}</td>
                                        <td style='color:#666;font-size:13px;text-align:right;padding:10px;'>{i.SanPham.GiaBan:n0}₫</td>
                                        <td style='color:#333;font-weight:bold;font-size:13px;text-align:right;padding:10px;'>{thanhTien:n0}₫</td>
                                    </tr>
");
            }
            sb.Append($@"
                                </tbody>
                            </table>
                        </td>
                    </tr>

                    <!-- Tổng tiền -->
                    <tr>
                        <td style='padding: 0 30px 25px 30px;'>
                            <table width='100%' cellpadding='10' cellspacing='0' style='background-color: #FFF8E1; border-radius: 6px;'>
                                <tr><td style='color:#666;font-size:14px;'>Tổng tiền hàng:</td><td align='right' style='color:#333;font-size:14px;'>{tongSanPham:n0}₫</td></tr>
                                <tr><td style='color:#666;font-size:14px;border-bottom:1px solid #FFE082;'>Phí vận chuyển:</td><td align='right' style='color:#333;font-size:14px;border-bottom:1px solid #FFE082;'>{phiShip:n0}₫</td></tr>
                                <tr><td style='color:#333;font-size:16px;font-weight:bold;padding-top:15px;'>Tổng thanh toán:</td><td align='right' style='color:#FF6F00;font-size:20px;font-weight:bold;padding-top:15px;'>{tongThanhToan:n0}₫</td></tr>
                            </table>
                        </td>
                    </tr>

                    <!-- Footer -->
                    <tr>
                        <td style='background-color: #333333; padding: 20px 30px; text-align: center;'>
                            <p style='margin: 0; color: #999999; font-size: 12px;'>Email tự động từ hệ thống Resip · Vui lòng không reply lại email này</p>
                        </td>
                    </tr>

                </table>
            </td>
        </tr>
    </table>
</body>
</html>
");
            return sb.ToString();
        }

        private static string GetPhuongThucThanhToanLabel(string pttt)
        {
            return pttt.ToUpper() switch
            {
                "COD"    => "💵 COD (Thanh toán khi nhận hàng)",
                "MOMO"   => "💜 Ví MoMo",
                "VNPAY"  => "🔵 VNPay",
                "SEPAY"  => "🟢 Chuyển khoản SePay",
                "PAYPAL" => "🔷 PayPal",
                _        => pttt
            };
        }
    }
}