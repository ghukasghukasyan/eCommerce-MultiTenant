namespace eCommerce.Frontend.Services
{
    public interface ICurrencyService
    {
        string CurrentCurrency { get; }
        IReadOnlyList<string> SupportedCurrencies { get; }
        string Format(decimal amdAmount);
        Task InitializeAsync();
        Task SetCurrencyAsync(string code);
        event Action? OnChange;
    }
}
