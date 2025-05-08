namespace APBDTut8API.Services;

public interface ITripService
{
    Task<IEnumerable<Trip>> GetTripsAsync();
    
    Task<IEnumerable<ClientTripDTO>> GetTripsForClientAsync(int clientId);
}