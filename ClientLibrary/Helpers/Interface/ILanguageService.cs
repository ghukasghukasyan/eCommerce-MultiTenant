namespace ClientLibrary.Helpers.Interface
{
    public interface ILanguageService
    {
        string CurrentLanguage { get; }
        event Action? OnChange;
        Task InitializeAsync();
        Task SetLanguageAsync(string languageCode);
    }
}
