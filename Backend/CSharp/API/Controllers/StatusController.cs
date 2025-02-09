using Microsoft.AspNetCore.Mvc;
using API.Data;
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
}
