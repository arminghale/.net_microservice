using Confluent.Kafka;
using Manage.Data.Management.Repository;
using Microsoft.Extensions.Hosting;
using System.Text.Json;

namespace Manage.Data.Management.Kafka
{
    //services.AddHostedService<GetUser>(provider => new GetUser(kafkaConfig,kafkaGroupId,topic,UserEF)); 
    public class GetUser : BackgroundService
    {
        private readonly IConsumer<Ignore, string> _consumer;
        private readonly IProducer<Null, string> _producer;
        private readonly string _topic;
        private readonly IUser _user;

        public GetUser(string kafkaConfig, string kafkaGroupId, string topic, UserEF user)
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
                
                var user = await _user.GetByID(message.id);

                var kafkamessage = new Message<Null, string> { Value = JsonSerializer.Serialize(user), };
                await _producer.ProduceAsync(consumeResult.Topic, kafkamessage);

                return true;
            }
            catch (Exception)
            {
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
