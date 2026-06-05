namespace eCommerce.Frontend.Services
{
    public class UiOverlayState
    {
        public event Action? OnShopToggleRequested;
        public event Action? OnSearchToggleRequested;
        public event Action? OnCartToggleRequested;

        public void RequestShopToggle()   => OnShopToggleRequested?.Invoke();
        public void RequestSearchToggle() => OnSearchToggleRequested?.Invoke();
        public void RequestCartToggle()   => OnCartToggleRequested?.Invoke();
    }
}
