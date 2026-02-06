using Npgsql;
using System.Data;

namespace FootballTournament.API.Data;

public interface IDbContext
{
    IDbConnection CreateConnection();
}

public class DapperContext : IDbContext
{
    private readonly string _connectionString;

    public DapperContext(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    public IDbConnection CreateConnection()
    {
        return new NpgsqlConnection(_connectionString);
    }
}
