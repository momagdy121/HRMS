namespace HRSystem.Business.Interfaces.Services;

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string toName, string subject, string htmlContent, CancellationToken cancellationToken = default);
}
