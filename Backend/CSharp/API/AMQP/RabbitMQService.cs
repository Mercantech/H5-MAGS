using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using RabbitMQ.Client.Exceptions;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using API.Data;
using Models;

namespace API.Services;

public class RabbitMQService : IHostedService
{
    private IConnection _connection;
    private IModel _channel;
    private readonly ILogger<RabbitMQService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    // Tilføj denne klasse for at repræsentere OLC-dataene
    private class OlcData
    {
        public long Timestamp { get; set; }
        public double Temperature { get; set; }
        public double Humidity { get; set; }
        public double Pressure { get; set; }
    }

    public RabbitMQService(
        ILogger<RabbitMQService> logger,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = "10.135.61.102",
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
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                
                // Udskriv den rå besked først for at hjælpe med fejlfinding
                _logger.LogInformation($" [x] Rå OLC data: {message}");
                
                try 
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    
                    var olcData = JsonSerializer.Deserialize<OlcData>(message, options);
                    
                    if (olcData != null)
                    {
                        DateTime dateTime;
                        if (olcData.Timestamp < 20000000000)
                        {
                            dateTime = DateTimeOffset.FromUnixTimeSeconds(olcData.Timestamp).DateTime;
                        }
                        else
                        {
                            dateTime = DateTimeOffset.FromUnixTimeMilliseconds(olcData.Timestamp).DateTime;
                        }
                        
                        if (olcData.Timestamp == 0)
                        {
                            dateTime = DateTime.Now;
                            _logger.LogWarning("Timestamp var 0, bruger nuværende tid i stedet.");
                        }

                        // Gem data i database
                        using (var scope = _scopeFactory.CreateScope())
                        {
                            var dbContext = scope.ServiceProvider.GetRequiredService<DBContext>();
                            
                            // Antager at vi har en default device - du kan tilpasse dette efter behov
                            var deviceData = new DeviceData
                            {
                                DeviceID = "ae091353-1fcd-4214-be23-eb1aecd53a98",
                                Temperature = (decimal)olcData.Temperature,
                                Humidity = (decimal)olcData.Humidity,
                                Pressure = (decimal)olcData.Pressure,
                                CreatedAt = dateTime,
                                UpdatedAt = dateTime
                            };

                            await dbContext.DeviceData.AddAsync(deviceData);
                            await dbContext.SaveChangesAsync();
                            
                            _logger.LogInformation($"Data gemt i database med ID: {deviceData.Id}");
                        }
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
                catch (Exception ex)
                {
                    _logger.LogError($"Fejl ved håndtering af besked: {ex.Message}");
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