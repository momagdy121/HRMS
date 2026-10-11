using HRSystem.Business.DTOs;
using HRSystem.Business.DTOs.Email;
using HRSystem.Business.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using sib_api_v3_sdk.Api;
using sib_api_v3_sdk.Client;
using sib_api_v3_sdk.Model;
using Task = System.Threading.Tasks.Task;

namespace HRSystem.Business.Services;

public class BrevoEmailService : IEmailService
{
    private readonly BravoApiSettings _settings;
    private readonly ILogger<BrevoEmailService> _logger;

    public BrevoEmailService(IOptions<BravoApiSettings> settings, ILogger<BrevoEmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    private bool IsConfigured =>
        !string.IsNullOrWhiteSpace(_settings.ApiKey) &&
        !_settings.ApiKey.Equals("YOUR_BREVO_API_KEY", StringComparison.OrdinalIgnoreCase);

    public async Task SendEmailAsync(string toEmail, string toName, string subject, string htmlContent, CancellationToken cancellationToken = default)
    {
        if (!IsConfigured)
        {
            _logger.LogWarning("Brevo API key is not configured. Email to {Email} ('{Subject}') was skipped in development mode.", toEmail, subject);
            return;
        }

        try
        {
            EnsureApiKey();

            var apiInstance = new TransactionalEmailsApi();

            var sender = new SendSmtpEmailSender(
                name: _settings.SenderName,
                email: _settings.SenderEmail);

            var toList = new List<SendSmtpEmailTo>
            {
                new SendSmtpEmailTo(email: toEmail, name: toName)
            };

            var email = new SendSmtpEmail(
                sender: sender,
                to: toList,
                subject: subject,
                htmlContent: htmlContent);

            var result = await apiInstance.SendTransacEmailAsync(email);

            _logger.LogInformation("Email sent successfully to {Email}. MessageId: {MessageId}", toEmail, result.MessageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email} via Brevo", toEmail);
            throw;
        }
    }

    public async Task SendBulkEmailAsync(IEnumerable<EmailMessage> messages, CancellationToken cancellationToken = default)
    {
        var messageList = messages?.ToList() ?? new List<EmailMessage>();
        if (messageList.Count == 0)
            return;

        if (!IsConfigured)
        {
            _logger.LogWarning("Brevo API key is not configured. Bulk email dispatch for {Count} messages was skipped in development mode.", messageList.Count);
            return;
        }

        EnsureApiKey();

        var successCount = 0;
        var failCount = 0;

        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = 5,
            CancellationToken = cancellationToken
        };

        await Parallel.ForEachAsync(messageList, parallelOptions, async (msg, ct) =>
        {
            try
            {
                var apiInstance = new TransactionalEmailsApi();
                var sender = new SendSmtpEmailSender(name: _settings.SenderName, email: _settings.SenderEmail);
                var toList = new List<SendSmtpEmailTo>
                {
                    new SendSmtpEmailTo(email: msg.ToEmail, name: msg.ToName)
                };

                var email = new SendSmtpEmail(
                    sender: sender,
                    to: toList,
                    subject: msg.Subject,
                    htmlContent: msg.HtmlContent);

                await apiInstance.SendTransacEmailAsync(email);
                Interlocked.Increment(ref successCount);
            }
            catch (Exception ex)
            {
                Interlocked.Increment(ref failCount);
                _logger.LogError(ex, "Failed to send bulk email item to {Email} via Brevo", msg.ToEmail);
            }
        });

        _logger.LogInformation("Bulk email dispatch completed. Sent: {SuccessCount}, Failed: {FailCount}, Total: {Total}",
            successCount, failCount, messageList.Count);
    }

    private void EnsureApiKey()
    {
        Configuration.Default.ApiKey.Clear();
        Configuration.Default.ApiKey.Add("api-key", _settings.ApiKey);
    }
}
