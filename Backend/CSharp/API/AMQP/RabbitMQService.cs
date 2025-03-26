using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using RabbitMQ.Client.Exceptions;
using System.Text.Json;

namespace API.Services;

public class RabbitMQService : IHostedService
{
    private IConnection _connection;
    private IModel _channel;
    private readonly ILogger<RabbitMQService> _logger;

    // Tilføj denne klasse for at repræsentere OLC-dataene
    private class OlcData
    {
        public long Timestamp { get; set; }
        public double Temperature { get; set; }
        public double Humidity { get; set; }
        public double Pressure { get; set; }
    }

    public RabbitMQService(ILogger<RabbitMQService> logger)
    {
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = "10.134.11.100",
                Port = 5672,
                UserName = "admin",
                Password = "admin",
                RequestedHeartbeat = TimeSpan.FromSeconds(600)
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare("amq.topic", ExchangeType.Topic, true);

            _channel.QueueDeclare(queue: "ArduinoData",
                                durable: true,
                                exclusive: false,
                                autoDelete: false,
                                arguments: null);

            _channel.QueueBind(queue: "ArduinoData",
                             exchange: "amq.topic",
                             routingKey: "ArduinoData");

            _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                
                // Udskriv den rå besked først for at hjælpe med fejlfinding
                _logger.LogInformation($" [x] Rå OLC data: {message}");
                
                try 
                {
                    // Brug JsonSerializerOptions for at sikre korrekt case-sensitiv deserialisering
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true // Gør egenskaber case-insensitive ved deserialisering
                    };
                    
                    var olcData = JsonSerializer.Deserialize<OlcData>(message, options);
                    
                    if (olcData != null)
                    {
                        // Log rådata for fejlfinding
                        _logger.LogDebug($"Deserialiseret timestamp værdi: {olcData.Timestamp}");
                        
                        // Prøv at håndtere forskellige timestamp-formater
                        DateTime dateTime;
                        
                        // Check om timestamp er under 20000000000 (sekunder siden 1970) eller over (millisekunder siden 1970)
                        if (olcData.Timestamp < 20000000000)
                        {
                            dateTime = DateTimeOffset.FromUnixTimeSeconds(olcData.Timestamp).DateTime;
                        }
                        else
                        {
                            dateTime = DateTimeOffset.FromUnixTimeMilliseconds(olcData.Timestamp).DateTime;
                        }
                        
                        // Hvis timestamp stadig er 0, prøv at bruge nuværende tid
                        if (olcData.Timestamp == 0)
                        {
                            dateTime = DateTime.Now;
                            _logger.LogWarning("Timestamp var 0, bruger nuværende tid i stedet.");
                        }
                        
                        _logger.LogInformation(
                            " [x] OLC Data modtaget:\n" +
                            $"  Tidspunkt: {dateTime:yyyy-MM-dd HH:mm:ss}\n" +
                            $"  Temperatur: {olcData.Temperature:F2}°C\n" +
                            $"  Luftfugtighed: {olcData.Humidity:F2}%\n" +
                            $"  Lufttryk: {olcData.Pressure:F2} hPa");
                    }
                    else
                    {
                        _logger.LogWarning($" [!] Kunne ikke deserialisere OLC data: {message}");
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogError($" [!] Fejl ved parsing af OLC data: {ex.Message}. Rå data: {message}");
                }

                _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            };

            _channel.BasicConsume(queue: "ArduinoData",
                                autoAck: false,
                                consumer: consumer);

            _logger.LogInformation(" [*] Venter på ArduinoData beskeder...");
            
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Der opstod en fejl: {ex.Message}");
            return Task.CompletedTask;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _channel?.Close();
        _connection?.Close();
        return Task.CompletedTask;
    }
}