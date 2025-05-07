namespace Tutorial8.Services;

public interface ITripService
{
    Task<IEnumerable<Trip>> GetTripsAsync();
}