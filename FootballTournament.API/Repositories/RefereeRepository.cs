using Dapper;
using FootballTournament.API.Data;
using FootballTournament.API.Models.Entities;

namespace FootballTournament.API.Repositories;

public interface IRefereeRepository
{
    Task<IEnumerable<Referee>> GetAllAsync();
    Task<Referee?> GetByIdAsync(int refereeId);
    Task<IEnumerable<Referee>> GetAvailableAsync(DateTime date);
    Task<int> CreateAsync(string fullName, string email, string? phoneNumber, string? role, 
        int experienceYears, string? certification, int? userId, string? username, string? ipAddress);
    Task UpdateAsync(int refereeId, string fullName, string email, string? phoneNumber, string? role,
        int experienceYears, string? certification, int? userId, string? username, string? ipAddress);
    Task DeleteAsync(int refereeId, int? userId, string? username, string? ipAddress);
    Task UpdateAvailabilityAsync(int refereeId, DateTime date, TimeSpan? startTime, TimeSpan? endTime,
        string status, string? notes, int? userId, string? username, string? ipAddress);
    Task AssignToMatchAsync(int matchId, int refereeId, string refereeRole, int? userId, string? username, string? ipAddress);
    Task<IEnumerable<MatchReferee>> GetMatchAssignmentsAsync(int matchId);
    Task<IEnumerable<MatchReferee>> GetRefereeScheduleAsync(int refereeId, DateTime? startDate, DateTime? endDate);
    Task RemoveFromMatchAsync(int matchId, int refereeId, int? userId, string? username, string? ipAddress);
}

public class RefereeRepository : IRefereeRepository
{
    private readonly IDbContext _context;

    public RefereeRepository(IDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Referee>> GetAllAsync()
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT 
                referee_id AS RefereeId,
                user_id AS UserId,
                full_name AS FullName,
                email AS Email,
                phone_number AS PhoneNumber,
                role AS Role,
                experience_years AS ExperienceYears,
                certification AS Certification,
                status::TEXT AS Status,
                matches_officiated AS MatchesOfficiated,
                next_assignment AS NextAssignment,
                next_assignment_date AS NextAssignmentDate,
                is_active AS IsActive
            FROM fn_get_referees()";

        return await connection.QueryAsync<Referee>(sql);
    }

    public async Task<Referee?> GetByIdAsync(int refereeId)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT 
                referee_id AS RefereeId,
                user_id AS UserId,
                full_name AS FullName,
                email AS Email,
                phone_number AS PhoneNumber,
                role AS Role,
                experience_years AS ExperienceYears,
                certification AS Certification,
                status::TEXT AS Status,
                photo_url AS PhotoUrl,
                matches_officiated AS MatchesOfficiated,
                is_active AS IsActive
            FROM fn_get_referee_by_id(@RefereeId)";

        return await connection.QueryFirstOrDefaultAsync<Referee>(sql, new { RefereeId = refereeId });
    }

    public async Task<IEnumerable<Referee>> GetAvailableAsync(DateTime date)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT 
                referee_id AS RefereeId,
                full_name AS FullName,
                email AS Email,
                role AS Role,
                experience_years AS ExperienceYears,
                status::TEXT AS Status
            FROM fn_get_available_referees(@Date)";

        return await connection.QueryAsync<Referee>(sql, new { Date = date });
    }

    public async Task<int> CreateAsync(string fullName, string email, string? phoneNumber, string? role, 
        int experienceYears, string? certification, int? userId, string? username, string? ipAddress)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT fn_create_referee(
                @FullName, @Email, @PhoneNumber, @Role,
                @ExperienceYears, @Certification,
                @UserId, @Username, @IpAddress
            )";

        return await connection.ExecuteScalarAsync<int>(sql, new
        {
            FullName = fullName,
            Email = email,
            PhoneNumber = phoneNumber,
            Role = role,
            ExperienceYears = experienceYears,
            Certification = certification,
            UserId = userId,
            Username = username,
            IpAddress = ipAddress
        });
    }

    public async Task UpdateAsync(int refereeId, string fullName, string email, string? phoneNumber, string? role,
        int experienceYears, string? certification, int? userId, string? username, string? ipAddress)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT fn_update_referee(
                @RefereeId, @FullName, @Email, @PhoneNumber, @Role,
                @ExperienceYears, @Certification,
                @UserId, @Username, @IpAddress
            )";

        await connection.ExecuteAsync(sql, new
        {
            RefereeId = refereeId,
            FullName = fullName,
            Email = email,
            PhoneNumber = phoneNumber,
            Role = role,
            ExperienceYears = experienceYears,
            Certification = certification,
            UserId = userId,
            Username = username,
            IpAddress = ipAddress
        });
    }

    public async Task DeleteAsync(int refereeId, int? userId, string? username, string? ipAddress)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = "SELECT fn_delete_referee(@RefereeId, @UserId, @Username, @IpAddress)";

        await connection.ExecuteAsync(sql, new
        {
            RefereeId = refereeId,
            UserId = userId,
            Username = username,
            IpAddress = ipAddress
        });
    }

    public async Task UpdateAvailabilityAsync(int refereeId, DateTime date, TimeSpan? startTime, TimeSpan? endTime,
        string status, string? notes, int? userId, string? username, string? ipAddress)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT fn_update_referee_availability(
                @RefereeId, @Date, @StartTime, @EndTime, @Status, @Notes,
                @UserId, @Username, @IpAddress
            )";

        await connection.ExecuteAsync(sql, new
        {
            RefereeId = refereeId,
            Date = date,
            StartTime = startTime,
            EndTime = endTime,
            Status = status,
            Notes = notes,
            UserId = userId,
            Username = username,
            IpAddress = ipAddress
        });
    }

    public async Task AssignToMatchAsync(int matchId, int refereeId, string refereeRole, 
        int? userId, string? username, string? ipAddress)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT fn_assign_referee_to_match(
                @MatchId, @RefereeId, @RefereeRole,
                @UserId, @Username, @IpAddress
            )";

        await connection.ExecuteAsync(sql, new
        {
            MatchId = matchId,
            RefereeId = refereeId,
            RefereeRole = refereeRole,
            UserId = userId,
            Username = username,
            IpAddress = ipAddress
        });
    }

    public async Task<IEnumerable<MatchReferee>> GetMatchAssignmentsAsync(int matchId)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT 
                match_referee_id AS MatchRefereeId,
                match_id AS MatchId,
                referee_id AS RefereeId,
                referee_role::TEXT AS RefereeRole,
                referee_name AS RefereeName
            FROM fn_get_match_referees(@MatchId)";

        return await connection.QueryAsync<MatchReferee>(sql, new { MatchId = matchId });
    }

    public async Task<IEnumerable<MatchReferee>> GetRefereeScheduleAsync(int refereeId, DateTime? startDate, DateTime? endDate)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT 
                match_referee_id AS MatchRefereeId,
                match_id AS MatchId,
                referee_id AS RefereeId,
                referee_role::TEXT AS RefereeRole,
                match_info AS MatchInfo,
                match_date AS MatchDate
            FROM fn_get_referee_schedule(@RefereeId, @StartDate, @EndDate)";

        return await connection.QueryAsync<MatchReferee>(sql, new 
        { 
            RefereeId = refereeId, 
            StartDate = startDate, 
            EndDate = endDate 
        });
    }

    public async Task RemoveFromMatchAsync(int matchId, int refereeId, int? userId, string? username, string? ipAddress)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = "SELECT fn_remove_referee_from_match(@MatchId, @RefereeId, @UserId, @Username, @IpAddress)";

        await connection.ExecuteAsync(sql, new
        {
            MatchId = matchId,
            RefereeId = refereeId,
            UserId = userId,
            Username = username,
            IpAddress = ipAddress
        });
    }
}
