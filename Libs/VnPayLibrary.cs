using System.Security.Cryptography;
using System.Text;

namespace ResipWeb.Libs
{
    public class VnPayLibrary
    {
        private readonly SortedDictionary<string, string> _requestData = new(StringComparer.Ordinal);
        private readonly SortedDictionary<string, string> _responseData = new(StringComparer.Ordinal);


        public string DebugHashData()
        {
            // dùng đúng rule hash: key không encode, value encode
            return BuildHashData(_requestData);
        }

        public string DebugQuery()
        {
            return BuildQueryString(_requestData);
        }


        // ===== REQUEST =====
        public void AddRequestData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
                _requestData[key] = value;
        }

        // ===== RESPONSE =====
        public void AddResponseData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
                _responseData[key] = value;
        }

        public string GetResponseData(string key)
        {
            return _responseData.TryGetValue(key, out var v) ? v : string.Empty;
        }

        // key không encode, value encode theo VNPAY
        private static string BuildHashData(SortedDictionary<string, string> data)
        {
            var sb = new StringBuilder();
            foreach (var kv in data)
            {
                if (sb.Length > 0) sb.Append('&');
                sb.Append(kv.Key);
                sb.Append('=');
                sb.Append(VnpayUrlEncoder.Encode(kv.Value));
            }
            return sb.ToString();
        }

        // query: encode cả key và value
        private static string BuildQueryString(SortedDictionary<string, string> data)
        {
            var sb = new StringBuilder();
            foreach (var kv in data)
            {
                if (sb.Length > 0) sb.Append('&');
                sb.Append(VnpayUrlEncoder.Encode(kv.Key));
                sb.Append('=');
                sb.Append(VnpayUrlEncoder.Encode(kv.Value));
            }
            return sb.ToString();
        }

        public string CreateRequestUrl(string baseUrl, string hashSecret)
        {
            var hashData = BuildHashData(_requestData);
            var secureHash = HmacSHA512(hashSecret, hashData);

            var query = BuildQueryString(_requestData);
            return $"{baseUrl}?{query}&vnp_SecureHash={secureHash}";
        }

        public bool ValidateSignature(string inputSecureHash, string secretKey)
        {
            var filtered = new SortedDictionary<string, string>(StringComparer.Ordinal);

            foreach (var kv in _responseData)
            {
                if (kv.Key == "vnp_SecureHash" || kv.Key == "vnp_SecureHashType") continue;
                filtered[kv.Key] = kv.Value;
            }

            var hashData = BuildHashData(filtered);
            var myHash = HmacSHA512(secretKey, hashData);

            return string.Equals(myHash, inputSecureHash, StringComparison.OrdinalIgnoreCase);
        }

        private static string HmacSHA512(string key, string inputData)
        {
            using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(key));
            var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(inputData));
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }
    }
}