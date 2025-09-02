using Azure.Storage.Queues;
using Microsoft.Extensions.Options;
using Sky.Template.Backend.Core.Configs;

namespace Sky.Template.Backend.Infrastructure.Azure;
public class QueueClientProvider
{
    private readonly QueueClient _queueClient;

    public QueueClientProvider(IOptions<AzureOptConfig> config)
    {
        var connectionString = config.Value.ConnectionString;
        var queueName = config.Value.QueueName;

        var queueClientOptions = new QueueClientOptions
        {
            MessageEncoding = QueueMessageEncoding.Base64
        };

        _queueClient = new QueueClient(connectionString, queueName, queueClientOptions);
    }

    public QueueClient GetQueueClient()
    {
        return _queueClient;
    }
}
