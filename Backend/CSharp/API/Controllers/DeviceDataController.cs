using API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeviceDataController : ControllerBase
    {
        private readonly DBContext _context;

        public DeviceDataController(DBContext context)
        {
            _context = context;
        }

        // GET: api/devicedata
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DeviceData>>> GetAllDeviceData()
        {
            return await _context.DeviceData
                .OrderByDescending(d => d.CreatedAt)
                .Take(100)  // Begrænser til de seneste 100 målinger
                .ToListAsync();
        }

        // GET: api/devicedata/{deviceId}
        [HttpGet("{deviceId}")]
        public async Task<ActionResult<IEnumerable<DeviceData>>> GetDeviceData(string deviceId)
        {
            var deviceData = await _context.DeviceData
                .Where(d => d.DeviceID == deviceId)
                .OrderByDescending(d => d.CreatedAt)
                .Take(100)  // Begrænser til de seneste 100 målinger
                .ToListAsync();

            if (!deviceData.Any())
            {
                return NotFound();
            }

            return deviceData;
        }

        // GET: api/devicedata/{deviceId}/latest
        [HttpGet("{deviceId}/latest")]
        public async Task<ActionResult<DeviceData>> GetLatestDeviceData(string deviceId)
        {
            var latestData = await _context.DeviceData
                .Where(d => d.DeviceID == deviceId)
                .OrderByDescending(d => d.CreatedAt)
                .FirstOrDefaultAsync();

            if (latestData == null)
            {
                return NotFound();
            }

            return latestData;
        }

        // GET: api/devicedata/{deviceId}/statistics
        [HttpGet("{deviceId}/statistics")]
        public async Task<ActionResult<object>> GetDeviceStatistics(string deviceId)
        {
            var data = await _context.DeviceData
                .Where(d => d.DeviceID == deviceId)
                .ToListAsync();

            if (!data.Any())
            {
                return NotFound();
            }

            var stats = new
            {
                Temperature = new
                {
                    Average = data.Average(d => d.Temperature ?? 0),
                    Min = data.Min(d => d.Temperature ?? 0),
                    Max = data.Max(d => d.Temperature ?? 0)
                },
                Humidity = new
                {
                    Average = data.Average(d => d.Humidity ?? 0),
                    Min = data.Min(d => d.Humidity ?? 0),
                    Max = data.Max(d => d.Humidity ?? 0)
                },
                Pressure = new
                {
                    Average = data.Average(d => d.Pressure ?? 0),
                    Min = data.Min(d => d.Pressure ?? 0),
                    Max = data.Max(d => d.Pressure ?? 0)
                },
                TotalReadings = data.Count,
                FirstReading = data.Min(d => d.CreatedAt),
                LastReading = data.Max(d => d.CreatedAt)
            };

            return Ok(stats);
        }

        // DELETE: api/devicedata/{deviceId}
        [HttpDelete("{deviceId}")]
        public async Task<IActionResult> DeleteDeviceData(string deviceId)
        {
            var deviceData = await _context.DeviceData
                .Where(d => d.DeviceID == deviceId)
                .ToListAsync();

            if (!deviceData.Any())
            {
                return NotFound();
            }

            _context.DeviceData.RemoveRange(deviceData);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
} 