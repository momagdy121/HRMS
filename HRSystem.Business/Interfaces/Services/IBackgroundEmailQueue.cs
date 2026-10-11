using System.Diagnostics.CodeAnalysis;
using HRSystem.Business.DTOs.Email;

namespace HRSystem.Business.Interfaces.Services;

public interface IBackgroundEmailQueue
{
    ValueTask QueueEmailAsync(EmailMessage message);

    ValueTask QueueEmailsAsync(IEnumerable<EmailMessage> messages);

    ValueTask<EmailMessage> DequeueAsync(CancellationToken cancellationToken);

    bool TryDequeue([MaybeNullWhen(false)] out EmailMessage message);
}
