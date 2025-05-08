namespace APBDTut8API.Services;

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
                   "SELECT t.IdTrip, t.Name, t.Description, t.DateFrom, t.DateTo, t.MaxPeople, c.Name as Country FROM trip t JOIN country_trip ct ON t.IdTrip = ct.IdTrip JOIN country c ON ct.IdCountry = c.IdCountry",
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
                        Country = reader.GetString(6),
                    });
                }
            }
        }
        
        return trips;
    }

    public async Task<IEnumerable<ClientTripDTO>> GetTripsForClientAsync(int clientId)
    {
        var trips = new List<ClientTripDTO>();
        
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var checkClient = new SqlCommand(
            "SELECT 1 FROM client WHERE IdClient = @IdClient", connection
        );
        checkClient.Parameters.AddWithValue("@IdClient", clientId);
        
        var exists = await checkClient.ExecuteScalarAsync();
        if (exists == null) return null;

        using (var command = new SqlCommand(
            "SELECT t.IdTrip, t.Name, t.Description, t.DateFrom, t.DateTo, t.MaxPeople, ct.RegisteredAt, ct.PaymentDate FROM trip t JOIN client_trip ct ON t.IdTrip = ct.IdTrip WHERE ct.IdClient = @IdClient",
            connection
        ))
        {
            command.Parameters.AddWithValue("@IdClient", clientId);
            
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    trips.Add(new ClientTripDTO
                    {
                        IdTrip = reader.GetInt32(0),
                        TripName = reader.GetString(1),
                        Description = reader.GetString(2),
                        DateFrom = reader.GetDateTime(3),
                        DateTo = reader.GetDateTime(4),
                        MaxPeople = reader.GetInt32(5),
                        RegisteredAt = reader.GetInt32(6),
                        PaymentDate = reader.GetInt32(7),
                    });
                }
            }
        }
        
        return trips;
    }
}