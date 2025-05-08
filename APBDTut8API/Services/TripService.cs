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

        //checking if client with such id exists
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
                        PaymentDate = reader.IsDBNull(7) ? 0 : reader.GetInt32(7)
                    });
                }
            }
        }
        
        return trips;
    }

    public async Task<int> AddClientAsync(ClientDTO client)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = new SqlCommand(
            "INSERT INTO Client (FirstName, LastName, Email, Telephone, Pesel) OUTPUT INSERTED.IdClient VALUES (@FirstName, @LastName, @Email, @Telephone, @Pesel)",
            connection);

        command.Parameters.AddWithValue("@FirstName", client.FirstName);
        command.Parameters.AddWithValue("@LastName", client.LastName);
        command.Parameters.AddWithValue("@Email", client.Email);
        command.Parameters.AddWithValue("@Telephone", client.Telephone);
        command.Parameters.AddWithValue("@Pesel", client.Pesel);

        var id = await command.ExecuteScalarAsync();
        
        return (int)id!;
    }

    public async Task RegisterClientForTripAsync(int clientId, int tripId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        //checking if client with such id exists
        var checkClient = new SqlCommand(
            "SELECT 1 FROM client WHERE IdClient = @IdClient", connection);
        checkClient.Parameters.AddWithValue("@IdClient", clientId);
        var existsClient = await checkClient.ExecuteScalarAsync();
        if (existsClient == null) throw new Exception("Client not found");

        //checking if trip with such id exists
        var checkTrip = new SqlCommand(
            "SELECT 1 FROM trip WHERE IdTrip = @IdTrip", connection);
        checkTrip.Parameters.AddWithValue("@IdTrip", tripId);
        var existsTrip = await checkTrip.ExecuteScalarAsync();
        if (existsTrip == null) throw new Exception("Trip not found");

        //checking if trip hasn't reached max number of participants
        var checkCapacity = new SqlCommand(
            "SELECT COUNT(*) FROM client_trip WHERE IdTrip = @IdTrip", connection);
        checkCapacity.Parameters.AddWithValue("@IdTrip", tripId);
        var currentCount = (int)await checkCapacity.ExecuteScalarAsync();
        if(currentCount == null) currentCount = 0;

        var maxCapacity = new SqlCommand(
            "SELECT MaxPeople FROM trip WHERE IdTrip = @IdTrip", connection);
        maxCapacity.Parameters.AddWithValue("@IdTrip", tripId);
        
        var maxPeople = (int)await maxCapacity.ExecuteScalarAsync();
        if(maxPeople == null) maxPeople = 0;
        
        if(currentCount >= maxPeople) throw new Exception("Trip is full");

        //checking if client is not already registered for this trip
        var checkRegistration = new SqlCommand(
            "SELECT 1 FROM client_trip WHERE IdTrip = @IdTrip AND IdClient = @IdClient", connection);
        checkRegistration.Parameters.AddWithValue("@IdTrip", tripId);
        checkRegistration.Parameters.AddWithValue("@IdClient", clientId);
        var registration = await checkRegistration.ExecuteScalarAsync();
        if(registration != null) throw new Exception("Registration already exists");
        
        //registration
        var insert = new SqlCommand(
            "INSERT INTO client_trip (IdClient, IdTrip, RegisteredAt) VALUES (@IdClient, @IdTrip, @RegisteredAt)",
            connection);
        insert.Parameters.AddWithValue("@IdClient", clientId);
        insert.Parameters.AddWithValue("@IdTrip", tripId);
        insert.Parameters.AddWithValue("@RegisteredAt", Int32.Parse(DateTime.Now.ToString("yyyyMMdd")));
        
        await insert.ExecuteNonQueryAsync();
    }

    public async Task DeleteRegistrationFromTripAsync(int clientId, int tripId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        //checking if client with such id exists
        var checkClient = new SqlCommand(
            "SELECT 1 FROM client WHERE IdClient = @IdClient", connection);
        checkClient.Parameters.AddWithValue("@IdClient", clientId);
        var existsClient = await checkClient.ExecuteScalarAsync();
        if (existsClient == null) throw new Exception("Client not found");

        //checking if trip with such id exists
        var checkTrip = new SqlCommand(
            "SELECT 1 FROM trip WHERE IdTrip = @IdTrip", connection);
        checkTrip.Parameters.AddWithValue("@IdTrip", tripId);
        var existsTrip = await checkTrip.ExecuteScalarAsync();
        if (existsTrip == null) throw new Exception("Trip not found");

        //checking if client is registered for this trip
        var checkRegistration = new SqlCommand(
            "SELECT 1 FROM client_trip WHERE IdTrip = @IdTrip AND IdClient = @IdClient", connection);
        checkRegistration.Parameters.AddWithValue("@IdTrip", tripId);
        checkRegistration.Parameters.AddWithValue("@IdClient", clientId);
        var registration = await checkRegistration.ExecuteScalarAsync();
        if(registration == null) throw new Exception("Client is not registered for this trip");
        
        //deleting registration
        var delete = new SqlCommand(
            "DELETE FROM client_trip WHERE IdClient = @IdClient AND IdTrip = @IdTrip", connection);
        delete.Parameters.AddWithValue("@IdClient", clientId);
        delete.Parameters.AddWithValue("@IdTrip", tripId);
        
        await delete.ExecuteNonQueryAsync();
    }
}