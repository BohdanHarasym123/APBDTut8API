namespace Tutorial8.Services;

using System.Data.SqlClient;

public class TripService : ITripService
{
    private readonly string _connectionString;
    
    public TripService()
    {
        _connectionString = "Data Source=localhost, 1433; User=SA; Password=yourStrong()Password; Initial Catalog=apbd; Integrated Security=False;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False";
    }
    
}