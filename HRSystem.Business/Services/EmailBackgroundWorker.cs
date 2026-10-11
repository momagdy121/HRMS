using HRSystem.Business.DTOs.Email;
using HRSystem.Business.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HRSystem.Business.Services;

public class EmailBackgroundWorker : BackgroundService
{
    private readonly IBackgroundEmailQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EmailBackgroundWorker> _logger;

    public EmailBackgroundWorker(
        IBackgroundEmailQueue queue,
        IServiceScopeFactory scopeFactory,
        ILogger<EmailBackgroundWorker> logger)
    {
        _queue = queue;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Email background worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var firstMessage = await _queue.DequeueAsync(stoppingToken);
                var batch = new List<EmailMessage> { firstMessage };

                // Batch drain up to 25 pending messages if available immediately
                while (batch.Count < 25 && _queue.TryDequeue(out var nextMessage))
                {
                    batch.Add(nextMessage);
                }

                using var scope = _scopeFactory.CreateScope();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                if (batch.Count == 1)
                {
                    var msg = batch[0];
                    await emailService.SendEmailAsync(msg.ToEmail, msg.ToName, msg.Subject, msg.HtmlContent, stoppingToken);
                }
                else
                {
                    await emailService.SendBulkEmailAsync(batch, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred in EmailBackgroundWorker.");
            }
        }

        _logger.LogInformation("Email background worker stopped.");
    }
}
