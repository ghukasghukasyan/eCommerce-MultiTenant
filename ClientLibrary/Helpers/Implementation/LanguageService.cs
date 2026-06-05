using ClientLibrary.Helpers.Interface;

namespace ClientLibrary.Helpers.Implementation
{
    public class LanguageService(ICookieService cookieStorageService) : ILanguageService
    {
        private const string LanguageCookie = "guani_lang";
        private bool _isInitialized;

        public string CurrentLanguage { get; private set; } = "en";

        public event Action? OnChange;

        public async Task InitializeAsync()
        {
            if (_isInitialized)
                return;

            var storedLanguage = await cookieStorageService.GetAsync(LanguageCookie);
            CurrentLanguage = Normalize(storedLanguage);
            _isInitialized = true;
        }

        public async Task SetLanguageAsync(string languageCode)
        {
            CurrentLanguage = Normalize(languageCode);
            _isInitialized = true;
            await cookieStorageService.SetAsync(LanguageCookie, CurrentLanguage, 365, "/");
            OnChange?.Invoke();
        }

        private static string Normalize(string? languageCode)
        {
            var code = languageCode?.Trim().ToLowerInvariant();
            return code is "hy" or "ru" ? code : "en";
        }
    }
}
