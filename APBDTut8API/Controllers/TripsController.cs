using Microsoft.AspNetCore.Mvc;
using APBDTut8API.Services;

namespace APBDTut8API.Controllers;


[ApiController]
[Route("api")]
public class TripsController : ControllerBase
{
    private readonly ITripService _service;

    public TripsController(ITripService service)
    {
        _service = service;
    }

    [HttpGet("trips")]
    public async Task<IActionResult> GetTrips()
    {
        var trips = await _service.GetTripsAsync();
        return Ok(trips);
    }

    [HttpGet("clients/{id}/trips")]
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
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPost("clients")]
    public async Task<IActionResult> AddClient([FromBody] ClientDTO client)
    {
        try
        {
            var id = await _service.AddClientAsync(client);
            return Created("", new { IdClient = id });
        }
        catch (Exception ex)    
        {
            return StatusCode(500, new {message = ex.Message});
        }
    }

    [HttpPut("clients/{clientId}/trips/{tripId}")]
    public async Task<IActionResult> RegisterClient(int clientId, int tripId)
    {
        try
        {
            await _service.RegisterClientForTripAsync(clientId, tripId);
            return Created("", $"Client {clientId} registered for trip {tripId}");
        }
        catch (Exception ex)
        {
            if(ex.Message.Contains("not found")) return NotFound(ex.Message);
            
            return StatusCode(500, ex.Message);
        }
    }

    [HttpDelete("clients/{clientId}/trips/{tripId}")]
    public async Task<IActionResult> DeleteClientRegistration(int clientId, int tripId)
    {
        try
        {
            await _service.DeleteRegistrationFromTripAsync(clientId, tripId);
            return NoContent();
        }
        catch (Exception ex)
        {
            if(ex.Message.Contains("not found")) return NotFound(ex.Message);
            return StatusCode(500, ex.Message);
        }
    }
}