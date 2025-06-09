using Confluent.Kafka;
using Manage.Data.Identity.Repository;
using Manage.Data.Public;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Manage.Data.Identity.Kafka
{
    //services.AddHostedService<GetUser>(provider => new GetUser(kafkaConfig,kafkaGroupId,topic,IUser,provider.GetRequiredService<ILogger<GetUser>>())); 
    public class GetUser : BackgroundService
    {
        private readonly IConsumer<Ignore, string> _consumer;
        private readonly IProducer<Null, string> _producer;
        private readonly string _topic;
        private readonly IUser _user;
        private readonly ILogger<GetUser> _logger;

        public GetUser(string kafkaConfig, string kafkaGroupId, string topic, IUser user, ILogger<GetUser> logger)
        {

            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = kafkaConfig,
                GroupId = kafkaGroupId,
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            _consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();

            var producerconfig = new ProducerConfig
            {
                BootstrapServers = kafkaConfig
            };

            _producer = new ProducerBuilder<Null, string>(producerconfig).Build();

            _topic = topic;
            _user = user;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _consumer.Subscribe(_topic);

            while (!stoppingToken.IsCancellationRequested)
            {
                await ProcessKafkaMessage(stoppingToken);

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }

            _consumer.Close();
        }

        public async Task<bool> ProcessKafkaMessage(CancellationToken stoppingToken)
        {
            try
            {
                var consumeResult = _consumer.Consume(stoppingToken);

                
                if (consumeResult.Message.Value == null)
                {
                    return false;
                }
                
                var message = JsonSerializer.Deserialize<DTO.Kafka.GetUser>(consumeResult.Message.Value);
                if (message == null)
                {
                    return false;
                }
                _logger.LogInformation($"Received Kafka message to send a User: {message.id}");
                var user = await _user.GetByID(message.id);

                var kafkamessage = new Message<Null, string> { Value = JsonSerializer.Serialize(user), };
                await _producer.ProduceAsync(consumeResult.Topic, kafkamessage);

                _logger.LogInformation($"Produce Kafka message to send User: topic: {_topic}, message: {kafkamessage}");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing Kafka message: {ex.Message}");
                return false;
            }
        }

        public override void Dispose() 
        { 
            _consumer.Dispose(); 
            _producer.Dispose(); 
            base.Dispose(); 
        }
    }
}
