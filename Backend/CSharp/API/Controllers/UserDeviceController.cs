using API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserDeviceController : ControllerBase
    {
        private readonly DBContext _context;

        public UserDeviceController(DBContext context)
        {
            _context = context;
        }

        // GET: api/userdevice/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<Device>>> GetUserDevices(string userId)
        {
            var devices = await _context.UserDevices
                .Where(ud => ud.UserID == userId)
                .Include(ud => ud.Device)
                .Select(ud => ud.Device)
                .ToListAsync();

            if (!devices.Any())
            {
                return NotFound($"Ingen enheder fundet for bruger med ID: {userId}");
            }

            return Ok(devices);
        }

        // GET: api/userdevice/device/{deviceId}
        [HttpGet("device/{deviceId}")]
        public async Task<ActionResult<IEnumerable<User>>> GetDeviceUsers(string deviceId)
        {
            var users = await _context.UserDevices
                .Where(ud => ud.DeviceID == deviceId)
                .Include(ud => ud.User)
                .Select(ud => new UserDto
                {
                    Id = ud.User.Id,
                    Name = ud.User.Name,
                    Email = ud.User.Email,
                    Role = ud.User.Role,
                    Status = ud.User.Status
                })
                .ToListAsync();

            if (!users.Any())
            {
                return NotFound($"Ingen brugere fundet for enhed med ID: {deviceId}");
            }

            return Ok(users);
        }

        // POST: api/userdevice
        [HttpPost]
        public async Task<ActionResult> AssignDeviceToUser(UserDeviceAssignDto assignDto)
        {
            // Tjek om bruger eksisterer
            var user = await _context.Users.FindAsync(assignDto.UserId);
            if (user == null)
            {
                return NotFound($"Bruger med ID {assignDto.UserId} blev ikke fundet");
            }

            // Tjek om device eksisterer
            var device = await _context.Devices.FindAsync(assignDto.DeviceId);
            if (device == null)
            {
                return NotFound($"Enhed med ID {assignDto.DeviceId} blev ikke fundet");
            }

            // Tjek om forbindelsen allerede eksisterer
            var existingConnection = await _context.UserDevices
                .AnyAsync(ud => ud.UserID == assignDto.UserId && ud.DeviceID == assignDto.DeviceId);

            if (existingConnection)
            {
                return BadRequest("Denne bruger er allerede tilknyttet denne enhed");
            }

            // Opret ny forbindelse
            var userDevice = new UserDevice
            {
                UserID = assignDto.UserId,
                DeviceID = assignDto.DeviceId
            };

            _context.UserDevices.Add(userDevice);
            await _context.SaveChangesAsync();

            return Ok("Enhed blev succesfuldt tilknyttet brugeren");
        }

        // DELETE: api/userdevice
        [HttpDelete]
        public async Task<ActionResult> RemoveDeviceFromUser(UserDeviceAssignDto assignDto)
        {
            var userDevice = await _context.UserDevices
                .FirstOrDefaultAsync(ud => 
                    ud.UserID == assignDto.UserId && 
                    ud.DeviceID == assignDto.DeviceId);

            if (userDevice == null)
            {
                return NotFound("Forbindelsen mellem bruger og enhed blev ikke fundet");
            }

            _context.UserDevices.Remove(userDevice);
            await _context.SaveChangesAsync();

            return Ok("Enheden blev succesfuldt fjernet fra brugeren");
        }
    }
} 