namespace APBDTut8API.Services;

public interface ITripService
{
    Task<IEnumerable<Trip>> GetTripsAsync();
    
    Task<IEnumerable<ClientTripDTO>> GetTripsForClientAsync(int clientId);

    Task<int> AddClientAsync(ClientDTO client);
    
    Task RegisterClientForTripAsync(int clientId, int tripId);
    
    Task DeleteRegistrationFromTripAsync(int clientId, int tripId);
}