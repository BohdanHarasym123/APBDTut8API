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

    //returns list of trips with basic info
    [HttpGet("trips")]
    public async Task<IActionResult> GetTrips()
    {
        var trips = await _service.GetTripsAsync();
        return Ok(trips);
    }

    //returns all trips of a specific client 
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

    //creates a new client record 
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

    //registers a client with specified id for a trip with specified id 
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

    //deletes clients registration from a specified trip
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