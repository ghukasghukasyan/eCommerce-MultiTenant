namespace eCommerce.Frontend.Models
{
    public class TenantConfig
    {
        public string StoreName { get; set; } = "Store";
        public string StoreDisplayName { get; set; } = "Store";
        public string LogoUrl { get; set; } = "";
        public string Tagline { get; set; } = "";
        public string AboutLabel { get; set; } = "About Us";
        public string ContactPhone { get; set; } = "";
        public string ContactPhoneDisplay { get; set; } = "";
        public int CopyrightYear { get; set; } = DateTime.UtcNow.Year;
        public string InstagramUrl { get; set; } = "";
        public string TikTokUrl { get; set; } = "";
        public string FacebookUrl { get; set; } = "";
        public string Currency { get; set; } = "AMD";
        public List<string> SupportedCurrencies { get; set; } = new() { "AMD", "USD", "RUB" };
        public string ApiBaseUrl { get; set; }
        public string HubBaseUrl { get; set; }
        public TenantColors Colors { get; set; } = new();
        public List<HeroSlideConfig> HeroSlides { get; set; } = new();
        public List<BrandHighlightConfig> BrandHighlights { get; set; } = new();
        public List<FooterTrustBadgeConfig> FooterTrustBadges { get; set; } = new();
        public EditorialBannerConfig EditorialBanner { get; set; } = new();
    }

    public class TenantColors
    {
        public string Primary { get; set; } = "#111111";
        public string Accent { get; set; } = "#f26522";
        public string Soft { get; set; } = "#e89cae";
        public string Background { get; set; } = "#fafaf8";
    }

    public class BrandHighlightConfig
    {
        public string Icon { get; set; } = "fa fa-star";
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
    }

    public class FooterTrustBadgeConfig
    {
        public string Icon { get; set; } = "fa fa-check";
        public string Label { get; set; } = "";
    }

    public class EditorialBannerConfig
    {
        public string Label { get; set; } = "Our Philosophy";
        public string Title { get; set; } = "";
        public string Subtitle { get; set; } = "";
        public string CtaLabel { get; set; } = "Discover More";
        public string BackgroundImagePath { get; set; } = "";
    }

    public class HeroSlideConfig
    {
        public string ImagePath { get; set; } = "";
        public string FallbackGradient { get; set; } = "#1a1a1a";
        public string Tag { get; set; } = "";
        public string Title { get; set; } = "";
        public string Subtitle { get; set; } = "";
        public string CtaLabel { get; set; } = "Shop Now";
        public string CtaUrl { get; set; } = "/";
    }
}
