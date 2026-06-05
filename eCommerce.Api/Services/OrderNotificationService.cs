using eCommerce.Api.Hubs;
using eCommerce.Application.Services.Interfaces.Notifications;
using Microsoft.AspNetCore.SignalR;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace eCommerce.Api.Services
{
    public class OrderNotificationService : IOrderNotificationService
    {
        private readonly IHubContext<OrderHub> _hubContext;
        private readonly ITelegramBotClient? _telegramBot;
        private readonly long? _adminChatId;
        private readonly ILogger<OrderNotificationService> _logger;
        private readonly string _currencyLabel;

        public OrderNotificationService(
            IHubContext<OrderHub> hubContext,
            ILogger<OrderNotificationService> logger,
            IConfiguration configuration)
        {
            _hubContext = hubContext;
            _logger = logger;

            _currencyLabel = configuration["Store:Currency"] ?? "AMD";

            var token = configuration["Telegram:BotToken"];
            var chatIdStr = configuration["Telegram:AdminChatId"];

            if (!string.IsNullOrWhiteSpace(token) && long.TryParse(chatIdStr, out var chatId))
            {
                _telegramBot = new TelegramBotClient(token);
                _adminChatId = chatId;
            }
        }

        public async Task NotifyNewOrderAsync(NewOrderEvent order)
        {
            await _hubContext.Clients.Group("admins").SendAsync("NewOrder", order);

            if (_telegramBot is not null && _adminChatId.HasValue)
            {
                try
                {
                    var phoneDisplay = string.IsNullOrWhiteSpace(order.CustomerPhone)
                        ? string.Empty
                        : $"\n📞 {Escape(order.CustomerPhone)}";

                    var text = $"🛒 *New Order\\!*\n" +
                               $"👤 {Escape(order.CustomerName)}\n" +
                               $"📧 {Escape(order.CustomerEmail)}" +
                               phoneDisplay + "\n" +
                               $"💰 {Escape(order.TotalAmount.ToString("N0"))} {Escape(_currencyLabel)}\n" +
                               $"🕐 {Escape(order.CreatedAt.ToString("dd MMM yyyy, HH:mm"))} UTC";

                    await _telegramBot.SendMessage(
                        _adminChatId.Value,
                        text,
                        parseMode: ParseMode.MarkdownV2);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Telegram notification failed for order {OrderId}", order.OrderId);
                }
            }
        }

        private static string Escape(string text) =>
            text.Replace("\\", "\\\\")
                .Replace("_", "\\_").Replace("*", "\\*")
                .Replace("[", "\\[").Replace("]", "\\]")
                .Replace("(", "\\(").Replace(")", "\\)")
                .Replace("~", "\\~").Replace("`", "\\`")
                .Replace(">", "\\>").Replace("#", "\\#")
                .Replace("+", "\\+").Replace("-", "\\-")
                .Replace("=", "\\=").Replace("|", "\\|")
                .Replace("{", "\\{").Replace("}", "\\}")
                .Replace(".", "\\.").Replace("!", "\\!");
    }
}
