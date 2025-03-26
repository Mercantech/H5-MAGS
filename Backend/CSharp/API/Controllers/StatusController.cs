using Microsoft.AspNetCore.Mvc;
using API.Data;
using RabbitMQ.Client;
namespace API.Controllers;

[ApiController]
[Route("[controller]")]
public class StatusController : ControllerBase
{
    private readonly DBContext _context;

    public StatusController(DBContext context)
    {
        _context = context;
    }
    [HttpGet]
    public IActionResult GetStatus()
    {
        return Ok("The server is Live");
    }
    [HttpGet("DB")]
    public IActionResult GetStatusDB()
    {
        if (_context.Database.CanConnect())
        {
            return Ok("The database and Server is Live!");
        }
        else return NotFound();
    }
    [HttpGet("RabbitMQ")]
    public IActionResult GetStatusRabbitMQ()
    {
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = "10.134.11.100",
                Port = 5672,
                UserName = "admin",
                Password = "admin"
            };

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                // Tjek om køen eksisterer
                try
                {
                    channel.QueueDeclarePassive("ArduinoData");
                    return Ok(new
                    {
                        Status = "Online",
                        Queue = "ArduinoData",
                        Connection = "Established",
                        Host = factory.HostName,
                        Port = factory.Port
                    });
                }
                catch (Exception)
                {
                    return StatusCode(500, new
                    {
                        Status = "Error",
                        Message = "Queue 'ArduinoData' not found",
                        Connection = "Established but queue missing"
                    });
                }
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                Status = "Offline",
                Error = ex.Message,
                Connection = "Failed"
            });
        }
    }

    [HttpGet("Full")]
    public IActionResult GetFullStatus()
    {
        var status = new
        {
            Server = "Online",
            Database = _context.Database.CanConnect() ? "Online" : "Offline",
            RabbitMQ = GetRabbitMQStatus(),
            Timestamp = DateTime.Now
        };

        return Ok(status);
    }

    private object GetRabbitMQStatus()
    {
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = "10.134.11.100",
                Port = 5672,
                UserName = "admin",
                Password = "admin"
            };

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclarePassive("ArduinoData");
                return new
                {
                    Status = "Online",
                    Queue = "ArduinoData",
                    Connection = "Established"
                };
            }
        }
        catch (Exception ex)
        {
            return new
            {
                Status = "Offline",
                Error = ex.Message,
                Connection = "Failed"
            };
        }
    }
}
