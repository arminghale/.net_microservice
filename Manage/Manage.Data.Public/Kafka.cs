using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Manage.Data.Public
{
    //services.AddSingleton<IKafkaProducer>(provider => new KafkaProducer(kafkaConfig)); 

    public interface IKafkaProducer
    {
        Task ProduceAsync(string topic, string message);
    }

    public class KafkaProducer : IKafkaProducer
    {
        private readonly IProducer<Null, string> _producer;

        private readonly ILogger<KafkaProducer> _logger;

        public KafkaProducer(string kafkaConfig, ILogger<KafkaProducer> logger)
        {

            var producerconfig = new ProducerConfig
            {
                BootstrapServers = kafkaConfig
            };

            _producer = new ProducerBuilder<Null, string>(producerconfig).Build();
            _logger = logger;
        }

        public async Task ProduceAsync(string topic, string message)
        {
            var kafkamessage = new Message<Null, string> { Value = message, };

            await _producer.ProduceAsync(topic, kafkamessage);
            _logger.LogInformation($"Produce Kafka message: topic: {topic}, message: {message}");
        }
    }

    //builder.Services.AddHostedService<KafkaConsumer>(provider => new KafkaConsumer(kafkaConfig, groupId, provider.GetRequiredService<ILogger<KafkaConsumer>>()));
    public class KafkaConsumer : BackgroundService
    {
        private readonly IConsumer<Ignore, string> _consumer;

        private readonly ILogger<KafkaConsumer> _logger;

        public KafkaConsumer(string kafkaConfig, string groupId, ILogger<KafkaConsumer> logger)
        {
            _logger = logger;

            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = kafkaConfig,
                GroupId = groupId,
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            _consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _consumer.Subscribe("InventoryUpdates");

            while (!stoppingToken.IsCancellationRequested)
            {
                ProcessKafkaMessage(stoppingToken);

                Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }

            _consumer.Close();
        }

        public void ProcessKafkaMessage(CancellationToken stoppingToken)
        {
            try
            {
                var consumeResult = _consumer.Consume(stoppingToken);

                var message = consumeResult.Message.Value;

                _logger.LogInformation($"Received Kafka message: {message}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing Kafka message: {ex.Message}");
            }
        }
    }
}
