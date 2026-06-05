using Microsoft.AspNetCore.SignalR.Client;

namespace eCommerce.Frontend.Services
{
    public record NewOrderNotification(
        Guid OrderId,
        string CustomerName,
        string CustomerEmail,
        decimal TotalAmount,
        DateTime CreatedAt);

    public class OrderNotificationState : IAsyncDisposable
    {
        private HubConnection? _hub;

        public event Action<NewOrderNotification>? OnNewOrder;
        public event Action? OnStateChanged;

        public int UnreadCount { get; private set; }
        public bool IsConnected => _hub?.State == HubConnectionState.Connected;

        public async Task StartAsync(string hubBaseUrl, string token)
        {
            if (_hub is not null) return;

            _hub = new HubConnectionBuilder()
                .WithUrl($"{hubBaseUrl.TrimEnd('/')}/hubs/orders", opts =>
                {
                    opts.AccessTokenProvider = () => Task.FromResult<string?>(token);
                })
                .WithAutomaticReconnect()
                .Build();

            _hub.On<NewOrderNotification>("NewOrder", notification =>
            {
                UnreadCount++;
                OnNewOrder?.Invoke(notification);
                OnStateChanged?.Invoke();
            });

            try
            {
                await _hub.StartAsync();
            }
            catch
            {
                // Hub unavailable or user is not admin — ignore silently
            }
        }

        public void ClearUnread()
        {
            UnreadCount = 0;
            OnStateChanged?.Invoke();
        }

        public async ValueTask DisposeAsync()
        {
            if (_hub is not null)
                await _hub.DisposeAsync();
        }
    }
}
