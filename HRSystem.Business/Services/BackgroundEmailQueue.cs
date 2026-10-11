using System.Diagnostics.CodeAnalysis;
using System.Threading.Channels;
using HRSystem.Business.DTOs.Email;
using HRSystem.Business.Interfaces.Services;

namespace HRSystem.Business.Services;

public class BackgroundEmailQueue : IBackgroundEmailQueue
{
    private readonly Channel<EmailMessage> _channel;

    public BackgroundEmailQueue(int capacity = 2000)
    {
        var options = new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false
        };

        _channel = Channel.CreateBounded<EmailMessage>(options);
    }

    public async ValueTask QueueEmailAsync(EmailMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);
        await _channel.Writer.WriteAsync(message);
    }

    public async ValueTask QueueEmailsAsync(IEnumerable<EmailMessage> messages)
    {
        ArgumentNullException.ThrowIfNull(messages);
        foreach (var message in messages)
        {
            await _channel.Writer.WriteAsync(message);
        }
    }

    public ValueTask<EmailMessage> DequeueAsync(CancellationToken cancellationToken)
    {
        return _channel.Reader.ReadAsync(cancellationToken);
    }

    public bool TryDequeue([MaybeNullWhen(false)] out EmailMessage message)
    {
        return _channel.Reader.TryRead(out message);
    }
}
