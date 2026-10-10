using HRSystem.Business.DTOs;
using HRSystem.Business.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using sib_api_v3_sdk.Api;
using sib_api_v3_sdk.Client;
using sib_api_v3_sdk.Model;

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

    public async System.Threading.Tasks.Task SendEmailAsync(string toEmail, string toName, string subject, string htmlContent, CancellationToken cancellationToken = default)
    {
        try
        {
            Configuration.Default.ApiKey.Clear();
            Configuration.Default.ApiKey.Add("api-key", _settings.ApiKey);

            var apiInstance = new TransactionalEmailsApi();

            var sender = new SendSmtpEmailSender(
                name: _settings.SenderName,
                email: _settings.SenderEmail);

            var toList = new System.Collections.Generic.List<SendSmtpEmailTo>
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
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email} via Brevo", toEmail);
            throw;
        }
    }
}
