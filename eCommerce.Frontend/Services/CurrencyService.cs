using System.Net.Http.Json;
using System.Text.Json.Serialization;
using ClientLibrary.Helpers.Interface;
using eCommerce.Frontend.Models;

namespace eCommerce.Frontend.Services
{
    public class CurrencyService : ICurrencyService
    {
        private static readonly Dictionary<string, (string Symbol, bool PrefixSymbol, int Decimals)> Meta = new()
        {
            { "AMD", ("֏",  false, 0) },
            { "USD", ("$",  true,  2) },
            { "RUB", ("₽",  false, 0) },
        };

        // Fallback rates (1 AMD = X) used when the live fetch fails
        private static readonly Dictionary<string, decimal> FallbackRates = new()
        {
            { "USD", 0.00256m },
            { "RUB", 0.232m  },
        };

        private const string RateApiUrl = "https://open.er-api.com/v6/latest/AMD";
        private const string CurrencyCookie  = "store_currency";

        private readonly ICookieService _cookie;
        private readonly HttpClient _http;
        private readonly Dictionary<string, decimal> _liveRates = new();
        private bool _initialized;

        public string CurrentCurrency { get; private set; }
        public IReadOnlyList<string> SupportedCurrencies { get; }
        public event Action? OnChange;

        public CurrencyService(ICookieService cookie, TenantConfig tenant, IHttpClientFactory httpClientFactory)
        {
            _cookie = cookie;
            _http   = httpClientFactory.CreateClient();
            CurrentCurrency = string.IsNullOrWhiteSpace(tenant.Currency) ? "AMD" : tenant.Currency;
            SupportedCurrencies = (tenant.SupportedCurrencies?.Count > 0
                ? tenant.SupportedCurrencies.Where(c => Meta.ContainsKey(c)).ToList()
                : new List<string> { CurrentCurrency })
                .AsReadOnly();
        }

        public async Task InitializeAsync()
        {
            if (_initialized) return;

            var stored = await _cookie.GetAsync(CurrencyCookie);
            if (!string.IsNullOrWhiteSpace(stored) && SupportedCurrencies.Contains(stored))
                CurrentCurrency = stored;

            await FetchLiveRatesAsync();
            _initialized = true;
        }

        public async Task SetCurrencyAsync(string currencyCode)
        {
            if (!Meta.ContainsKey(currencyCode)) return;
            CurrentCurrency = currencyCode;
            await _cookie.SetAsync(CurrencyCookie, currencyCode, 365, "/");
            OnChange?.Invoke();
        }

        public string Format(decimal amdAmount)
        {
            var converted = ToCurrentCurrency(amdAmount);
            var (symbol, prefix, decimals) = Meta[CurrentCurrency];
            var formatted = converted.ToString($"N{decimals}");
            return prefix ? $"{symbol}{formatted}" : $"{formatted} {symbol}";
        }

        private decimal ToCurrentCurrency(decimal amdAmount)
        {
            if (CurrentCurrency == "AMD") return amdAmount;
            if (_liveRates.TryGetValue(CurrentCurrency, out var live)) return amdAmount * live;
            return FallbackRates.TryGetValue(CurrentCurrency, out var fallback) ? amdAmount * fallback : amdAmount;
        }

        private async Task FetchLiveRatesAsync()
        {
            try
            {
                var response = await _http.GetFromJsonAsync<ExchangeRateResponse>(RateApiUrl);
                if (response?.Result == "success" && response.Rates is not null)
                {
                    foreach (var currency in SupportedCurrencies)
                    {
                        if (currency != "AMD" && response.Rates.TryGetValue(currency, out var rate))
                            _liveRates[currency] = rate;
                    }
                }
            }
            catch
            {
                // silent fallback — FallbackRates used instead
            }
        }

        private class ExchangeRateResponse
        {
            [JsonPropertyName("result")]
            public string? Result { get; set; }

            [JsonPropertyName("rates")]
            public Dictionary<string, decimal>? Rates { get; set; }
        }
    }
}
