using System.Text;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Extensions.Options;

namespace ReportingService.EventStore
{
    public class EventStoreClient : IEventStore
    {
        private readonly ILogger<EventStoreClient> _logger;
        private readonly EventStoreConfig _config;

        private static IEventStoreConnection _connection;

        public EventStoreClient(ILogger<EventStoreClient> logger, IOptions<EventStoreConfig> config)
        {
            _logger = logger;
            _config = config.Value;
        }

        public async Task Publish<T>(string eventType, T eventData)
        {
            var connection = await CreateConnection();

            var jsonContent = JsonConvert.SerializeObject(eventData);

            var data = new List<EventData>
            {
                new EventData(
                    Guid.NewGuid(),
                    eventType,
                    true,
                    Encoding.UTF8.GetBytes(jsonContent),
                    new byte[]{}
                )
            };

            await connection.AppendToStreamAsync("recipe-events", ExpectedVersion.Any, data);
        }

        private async Task<IEventStoreConnection> CreateConnection()
        {
            if (_connection == null)
            {
                var connString = _config.ConnectionString;
                _connection = EventStoreConnection.Create(GetConnectionString());

                await _connection.ConnectAsync();
            }

            return _connection;
        }

        private Uri GetConnectionString()
        {
            var builder = new UriBuilder
            {
                Scheme = "tcp",
                Host = _config.Host,
                UserName = _config.Username,
                Password = _config.Password,
                Port = 1113
            };

            return builder.Uri;
        }
    }
}
