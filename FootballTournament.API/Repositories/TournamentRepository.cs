using Dapper;
using FootballTournament.API.Data;
using FootballTournament.API.Models.Entities;

namespace FootballTournament.API.Repositories;

public interface ITournamentRepository
{
    Task<Tournament?> GetActiveAsync();
    Task<Tournament?> GetByIdAsync(int tournamentId);
    Task<IEnumerable<Tournament>> GetAllAsync();
    Task<DashboardStats?> GetDashboardStatsAsync(int tournamentId);
    Task<IEnumerable<Venue>> GetVenuesAsync();
    Task<int> CreateAsync(string tournamentName, string? description, DateTime startDate, DateTime endDate,
        string tournamentType, int maxTeams, int? institutionId, string? organizerName, string? organizerContact,
        int? userId, string? username, string? ipAddress);
    Task UpdateAsync(int tournamentId, string tournamentName, string? description, DateTime startDate, 
        DateTime endDate, string tournamentType, string status, int maxTeams, int? institutionId, 
        string? organizerName, string? organizerContact, int? userId, string? username, string? ipAddress);
}

public class TournamentRepository : ITournamentRepository
{
    private readonly IDbContext _context;

    public TournamentRepository(IDbContext context)
    {
        _context = context;
    }

    public async Task<Tournament?> GetActiveAsync()
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT 
                tournament_id AS TournamentId,
                tournament_name AS TournamentName,
                description AS Description,
                start_date AS StartDate,
                end_date AS EndDate,
                tournament_type::TEXT AS TournamentType,
                status::TEXT AS Status,
                max_teams AS MaxTeams,
                institution_id AS InstitutionId,
                institution_name AS InstitutionName,
                organizer_name AS OrganizerName,
                organizer_contact AS OrganizerContact
            FROM fn_get_active_tournament()";

        return await connection.QueryFirstOrDefaultAsync<Tournament>(sql);
    }

    public async Task<Tournament?> GetByIdAsync(int tournamentId)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT 
                tournament_id AS TournamentId,
                tournament_name AS TournamentName,
                description AS Description,
                start_date AS StartDate,
                end_date AS EndDate,
                tournament_type::TEXT AS TournamentType,
                status::TEXT AS Status,
                max_teams AS MaxTeams,
                institution_id AS InstitutionId,
                organizer_name AS OrganizerName,
                organizer_contact AS OrganizerContact
            FROM tournaments WHERE tournament_id = @TournamentId";

        return await connection.QueryFirstOrDefaultAsync<Tournament>(sql, new { TournamentId = tournamentId });
    }

    public async Task<IEnumerable<Tournament>> GetAllAsync()
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT 
                tournament_id AS TournamentId,
                tournament_name AS TournamentName,
                description AS Description,
                start_date AS StartDate,
                end_date AS EndDate,
                tournament_type::TEXT AS TournamentType,
                status::TEXT AS Status,
                max_teams AS MaxTeams
            FROM tournaments ORDER BY start_date DESC";

        return await connection.QueryAsync<Tournament>(sql);
    }

    public async Task<DashboardStats?> GetDashboardStatsAsync(int tournamentId)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT 
                total_teams AS TotalTeams,
                total_matches AS TotalMatches,
                completed_matches AS CompletedMatches,
                scheduled_matches AS ScheduledMatches,
                live_matches AS LiveMatches,
                total_goals AS TotalGoals,
                total_players AS TotalPlayers
            FROM fn_get_dashboard_stats(@TournamentId)";

        return await connection.QueryFirstOrDefaultAsync<DashboardStats>(sql, new { TournamentId = tournamentId });
    }

    public async Task<IEnumerable<Venue>> GetVenuesAsync()
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT 
                venue_id AS VenueId,
                venue_name AS VenueName,
                location AS Location,
                capacity AS Capacity,
                has_floodlights AS HasFloodlights,
                field_size AS FieldSize,
                surface_type AS SurfaceType,
                status::TEXT AS Status,
                is_active AS IsActive
            FROM fn_get_venues()";

        return await connection.QueryAsync<Venue>(sql);
    }

    public async Task<int> CreateAsync(string tournamentName, string? description, DateTime startDate, 
        DateTime endDate, string tournamentType, int maxTeams, int? institutionId, string? organizerName, 
        string? organizerContact, int? userId, string? username, string? ipAddress)
    {
        try
        {

            using var connection = _context.CreateConnection();

            const string sql = @"
            SELECT fn_create_tournament(
                @TournamentName, @Description, @StartDate, @EndDate,
                @TournamentType, @MaxTeams, @InstitutionId, @OrganizerName, @OrganizerContact,
                @UserId, @Username, @IpAddress
            )";

            return await connection.ExecuteScalarAsync<int>(sql, new
            {
                TournamentName = tournamentName,
                Description = description,
                StartDate = startDate,
                EndDate = endDate,
                TournamentType = tournamentType,
                MaxTeams = maxTeams,
                InstitutionId = institutionId,
                OrganizerName = organizerName,
                OrganizerContact = organizerContact,
                UserId = userId,
                Username = username,
                IpAddress = ipAddress
            });

        }
        catch (Exception Ex)
        {

            throw;
        }
    }

    public async Task UpdateAsync(int tournamentId, string tournamentName, string? description, 
        DateTime startDate, DateTime endDate, string tournamentType, string status, int maxTeams, 
        int? institutionId, string? organizerName, string? organizerContact, int? userId, 
        string? username, string? ipAddress)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT fn_update_tournament(
                @TournamentId, @TournamentName, @Description, @StartDate, @EndDate,
                @TournamentType, @Status, @MaxTeams, @InstitutionId, @OrganizerName, @OrganizerContact,
                @UserId, @Username, @IpAddress
            )";

        await connection.ExecuteAsync(sql, new
        {
            TournamentId = tournamentId,
            TournamentName = tournamentName,
            Description = description,
            StartDate = startDate,
            EndDate = endDate,
            TournamentType = tournamentType,
            Status = status,
            MaxTeams = maxTeams,
            InstitutionId = institutionId,
            OrganizerName = organizerName,
            OrganizerContact = organizerContact,
            UserId = userId,
            Username = username,
            IpAddress = ipAddress
        });
    }
}
