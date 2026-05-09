using Microsoft.Extensions.Caching.Memory;

namespace ResipWeb.Services
{
    public class ExchangeRateService
    {
        private readonly HttpClient _http;
        private readonly IMemoryCache _cache;
        private const string CacheKey = "USD_VND_RATE";

        public ExchangeRateService(HttpClient http, IMemoryCache cache)
        {
            _http = http;
            _cache = cache;
        }

        public async Task<decimal> GetUsdToVndAsync()
        {
            if (_cache.TryGetValue(CacheKey, out decimal cached))
                return cached;

            try
            {
                var res = await _http.GetFromJsonAsync<ExchangeRateResponse>(
                    "https://open.er-api.com/v6/latest/USD");

                var rate = res?.rates?["VND"] ?? 25000m;

                if (rate > 0)
                {
                    _cache.Set(CacheKey, rate, TimeSpan.FromMinutes(60));
                    return rate;
                }
            }
            catch (Exception)
            {
                // API lỗi, dùng tỷ giá mặc định 25000 VND/USD
            }

            return 25000m;
        }
    }

    public record ExchangeRateResponse(string result, Dictionary<string, decimal> rates);
}
