using System;

public static class VnpayUrlEncoder
{
    public static string Encode(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;

        // RFC3986 – VNPAY thường khớp kiểu này hơn UrlEncode (+)
        return Uri.EscapeDataString(input)
            .Replace("%20", "+"); 
                                  
    }
}