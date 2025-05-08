namespace Tutorial8.Services;

using System.Data.SqlClient;

public class TripService : ITripService
{
    private readonly string _connectionString;
    
    public TripService()
    {
        _connectionString = "Data Source=localhost, 1433; User=SA; Password=yourStrong()Password; Integrated Security=False;Connect Timeout=30;Encrypt=False";
    }

    public async Task<IEnumerable<Trip>> GetTripsAsync()
    {
        var trips = new List<Trip>();
        
        using (var connection = new SqlConnection(_connectionString))
        using (var command = new SqlCommand(
                   "SELECT * FROM trip",
                   connection))
        {
            await connection.OpenAsync();
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var id = reader.GetOrdinal("IdTrip");
                    trips.Add(new Trip
                    {
                        IdTrip = reader.GetInt32(id),
                        Name = reader.GetString(1),
                        Description = reader.GetString(2),
                        DateFrom = reader.GetDateTime(3),
                        DateTo = reader.GetDateTime(4),
                        MaxPeople = reader.GetInt32(5),
                    });
                }
            }
        }
        
        return trips;
    }
}