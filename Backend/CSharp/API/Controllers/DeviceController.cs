using API.Data; // Juster stien, hvis nødvendigt
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeviceController : ControllerBase
    {
        private readonly DBContext _context;

        public DeviceController(DBContext context)
        {
            _context = context;
        }

        // GET: api/device
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DeviceDto>>> GetDevices()
        {
            var devices = await _context.Devices.ToListAsync();
            return Ok(devices.Select(d => new DeviceDto
            {
                Id = d.Id,
                Name = d.Name,
                Description = d.Description
            }));
        }

        // GET: api/device/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<DeviceDto>> GetDevice(string id)
        {
            var device = await _context.Devices.FindAsync(id);

            if (device == null)
            {
                return NotFound();
            }

            var deviceDto = new DeviceDto
            {
                Id = device.Id,
                Name = device.Name,
                Description = device.Description
            };

            return Ok(deviceDto);
        }

        // POST: api/device
        [HttpPost]
        public async Task<ActionResult<DeviceDto>> CreateDevice(DeviceCreateDto deviceCreateDto)
        {
            var device = new Device
            {
                Name = deviceCreateDto.Name,
                Description = deviceCreateDto.Description
            };

            _context.Devices.Add(device);
            await _context.SaveChangesAsync();

            var deviceDto = new DeviceDto
            {
                Id = Guid.NewGuid().ToString(),
                Name = device.Name,
                Description = device.Description
            };

            return CreatedAtAction(nameof(GetDevice), new { id = device.Id }, deviceDto);
        }

        // PUT: api/device/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDevice(string id, DeviceUpdateDto deviceUpdateDto)
        {
            var device = await _context.Devices.FindAsync(id);
            if (device == null)
            {
                return NotFound();
            }

            device.Name = deviceUpdateDto.Name;
            device.Description = deviceUpdateDto.Description;

            _context.Entry(device).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/device/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDevice(int id)
        {
            var device = await _context.Devices.FindAsync(id);
            if (device == null)
            {
                return NotFound();
            }

            _context.Devices.Remove(device);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}