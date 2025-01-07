using Confluent.Kafka;

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

        public KafkaProducer(string kafkaConfig)
        {

            var producerconfig = new ProducerConfig
            {
                BootstrapServers = kafkaConfig
            };

            _producer = new ProducerBuilder<Null, string>(producerconfig).Build();
        }

        public async Task ProduceAsync(string topic, string message)
        {
            var kafkamessage = new Message<Null, string> { Value = message, };

            await _producer.ProduceAsync(topic, kafkamessage);
        }
    }

}
