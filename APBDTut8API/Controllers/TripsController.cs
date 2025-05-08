using Microsoft.AspNetCore.Mvc;
using APBDTut8API.Services;

namespace APBDTut8API.Controllers;


[ApiController]
[Route("api/[controller]")]
public class TripsController : ControllerBase
{
    private readonly ITripService _service;

    public TripsController(ITripService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetTrips()
    {
        var trips = await _service.GetTripsAsync();
        return Ok(trips);
    }

    [HttpGet("/api/clients/{id}/trips")]
    public async Task<IActionResult> GetClientTrips(int id)
    {
        try
        {
            var trips = await _service.GetTripsForClientAsync(id);

            if (trips == null) return NotFound(new { message = "Client with such id doesn't exist" });
            
            if (trips.Any())
            {
                return Ok(trips);
            }
            
            return NotFound(new { message = "Client doesn't have any trips" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex);
        }
    }
}