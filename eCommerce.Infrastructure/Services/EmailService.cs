using eCommerce.Application.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace eCommerce.Infrastructure.Services
{
    public class EmailService(IConfiguration config, IHttpClientFactory httpClientFactory, ILogger<EmailService> logger) : IEmailService
    {
        private string ApiKey     => config["Email:ApiKey"]      ?? config["EMAIL_API_KEY"]      ?? "";
        private string FromAddress => config["Email:FromAddress"] ?? config["EMAIL_FROM_ADDRESS"] ?? "";
        private string FromName    => config["Email:FromName"]    ?? config["EMAIL_FROM_NAME"]    ?? "Store";

        public async Task SendEmailConfirmationAsync(string toEmail, string toName, string confirmationLink)
        {
            var html = $@"
                <div style='font-family:sans-serif;max-width:480px;margin:0 auto'>
                    <h2>Confirm your email</h2>
                    <p>Hi {toName},</p>
                    <p>Thanks for registering. Click the button below to confirm your email address.</p>
                    <a href='{confirmationLink}'
                       style='display:inline-block;padding:12px 24px;background:#111;color:#fff;
                              text-decoration:none;border-radius:6px;margin:16px 0'>
                        Confirm Email
                    </a>
                    <p style='color:#888;font-size:13px'>If you didn't create an account, you can ignore this email.</p>
                </div>";

            await SendAsync(toEmail, toName, "Confirm your email address", html);
        }

        public async Task SendPasswordResetAsync(string toEmail, string toName, string resetLink)
        {
            var html = $@"
                <div style='font-family:sans-serif;max-width:480px;margin:0 auto'>
                    <h2>Reset your password</h2>
                    <p>Hi {toName},</p>
                    <p>We received a request to reset your password. Click below to choose a new one.</p>
                    <a href='{resetLink}'
                       style='display:inline-block;padding:12px 24px;background:#111;color:#fff;
                              text-decoration:none;border-radius:6px;margin:16px 0'>
                        Reset Password
                    </a>
                    <p style='color:#888;font-size:13px'>This link expires in 1 hour. If you didn't request a reset, you can ignore this email.</p>
                </div>";

            await SendAsync(toEmail, toName, "Reset your password", html);
        }

        private async Task SendAsync(string toEmail, string toName, string subject, string htmlBody)
        {
            logger.LogInformation("Sending email via Brevo API — From: {FromAddress}, To: {ToEmail}, ApiKey configured: {HasKey}",
                FromAddress, toEmail, !string.IsNullOrWhiteSpace(ApiKey));

            if (string.IsNullOrWhiteSpace(ApiKey))
                throw new InvalidOperationException("Email:ApiKey is not configured.");

            var payload = new
            {
                sender = new { name = FromName, email = FromAddress },
                to = new[] { new { email = toEmail, name = toName } },
                subject,
                htmlContent = htmlBody
            };

            var client = httpClientFactory.CreateClient("Brevo");
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("https://api.brevo.com/v3/smtp/email", content);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"Brevo API error {(int)response.StatusCode}: {body}");
        }
    }
}
