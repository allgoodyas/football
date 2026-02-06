using Dapper;
using FootballTournament.API.Data;
using FootballTournament.API.Models.Entities;

namespace FootballTournament.API.Repositories;

public interface ITeamRepository
{
    Task<IEnumerable<Team>> GetAllAsync();
    Task<IEnumerable<Team>> GetAllByTournamentAsync(int tournamentId);
    Task<Team?> GetByIdAsync(int teamId);
    Task<int> CreateAsync(int tournamentId, string teamName, string teamCode, string? className, 
        string? captainName, string? captainContact, string? coachName, string? teamColor, 
        string? groupName, int? institutionId, int? userId, string? username, string? ipAddress);
    Task UpdateAsync(int teamId, string teamName, string teamCode, string? className, 
        string? captainName, string? captainContact, string? coachName, string? teamColor, 
        string? groupName, int? institutionId, int? userId, string? username, string? ipAddress);
    Task DeleteAsync(int teamId, int? userId, string? username, string? ipAddress);
    Task<IEnumerable<TeamStanding>> GetStandingsAsync(int tournamentId, string? groupName = null);
}

public class TeamRepository : ITeamRepository
{
    private readonly IDbContext _context;

    public TeamRepository(IDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Team>> GetAllAsync()
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT 
                team_id AS TeamId,
                tournament_id AS TournamentId,
                team_name AS TeamName,
                team_code AS TeamCode,
                class_name AS ClassName,
                captain_name AS CaptainName,
                captain_contact AS CaptainContact,
                coach_name AS CoachName,
                team_color AS TeamColor,
                group_name AS GroupName,
                institution_id AS InstitutionId,
                matches_played AS MatchesPlayed,
                wins AS Wins,
                draws AS Draws,
                losses AS Losses,
                goals_for AS GoalsFor,
                goals_against AS GoalsAgainst,
                goal_difference AS GoalDifference,
                points AS Points,
                is_eliminated AS IsEliminated,
                is_active AS IsActive
            FROM teams WHERE is_active = true
            ORDER BY team_name";

        return await connection.QueryAsync<Team>(sql);
    }

    public async Task<IEnumerable<Team>> GetAllByTournamentAsync(int tournamentId)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT 
                team_id AS TeamId,
                team_name AS TeamName,
                team_code AS TeamCode,
                class_name AS ClassName,
                captain_name AS CaptainName,
                captain_contact AS CaptainContact,
                coach_name AS CoachName,
                team_color AS TeamColor,
                group_name AS GroupName,
                institution_id AS InstitutionId,
                matches_played AS MatchesPlayed,
                wins AS Wins,
                draws AS Draws,
                losses AS Losses,
                goals_for AS GoalsFor,
                goals_against AS GoalsAgainst,
                goal_difference AS GoalDifference,
                points AS Points,
                is_eliminated AS IsEliminated
            FROM fn_get_teams(@TournamentId)";

        return await connection.QueryAsync<Team>(sql, new { TournamentId = tournamentId });
    }

    public async Task<Team?> GetByIdAsync(int teamId)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT 
                team_id AS TeamId,
                tournament_id AS TournamentId,
                team_name AS TeamName,
                team_code AS TeamCode,
                class_name AS ClassName,
                captain_name AS CaptainName,
                captain_contact AS CaptainContact,
                coach_name AS CoachName,
                team_color AS TeamColor,
                group_name AS GroupName,
                institution_id AS InstitutionId,
                matches_played AS MatchesPlayed,
                wins AS Wins,
                draws AS Draws,
                losses AS Losses,
                goals_for AS GoalsFor,
                goals_against AS GoalsAgainst,
                goal_difference AS GoalDifference,
                points AS Points,
                is_eliminated AS IsEliminated
            FROM fn_get_team_by_id(@TeamId)";

        return await connection.QueryFirstOrDefaultAsync<Team>(sql, new { TeamId = teamId });
    }

    public async Task<int> CreateAsync(int tournamentId, string teamName, string teamCode, 
        string? className, string? captainName, string? captainContact, string? coachName, 
        string? teamColor, string? groupName, int? institutionId, int? userId, string? username, string? ipAddress)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT fn_create_team(
                @TournamentId, @TeamName, @TeamCode, @ClassName, @CaptainName, 
                @CaptainContact, @CoachName, @TeamColor, @GroupName, @InstitutionId,
                @UserId, @Username, @IpAddress
            )";

        return await connection.ExecuteScalarAsync<int>(sql, new
        {
            TournamentId = tournamentId,
            TeamName = teamName,
            TeamCode = teamCode,
            ClassName = className,
            CaptainName = captainName,
            CaptainContact = captainContact,
            CoachName = coachName,
            TeamColor = teamColor,
            GroupName = groupName,
            InstitutionId = institutionId,
            UserId = userId,
            Username = username,
            IpAddress = ipAddress
        });
    }

    public async Task UpdateAsync(int teamId, string teamName, string teamCode, 
        string? className, string? captainName, string? captainContact, string? coachName, 
        string? teamColor, string? groupName, int? institutionId, int? userId, string? username, string? ipAddress)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT fn_update_team(
                @TeamId, @TeamName, @TeamCode, @ClassName, @CaptainName, 
                @CaptainContact, @CoachName, @TeamColor, @GroupName, @InstitutionId,
                @UserId, @Username, @IpAddress
            )";

        await connection.ExecuteAsync(sql, new
        {
            TeamId = teamId,
            TeamName = teamName,
            TeamCode = teamCode,
            ClassName = className,
            CaptainName = captainName,
            CaptainContact = captainContact,
            CoachName = coachName,
            TeamColor = teamColor,
            GroupName = groupName,
            InstitutionId = institutionId,
            UserId = userId,
            Username = username,
            IpAddress = ipAddress
        });
    }

    public async Task DeleteAsync(int teamId, int? userId, string? username, string? ipAddress)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = "SELECT fn_delete_team(@TeamId, @UserId, @Username, @IpAddress)";

        await connection.ExecuteAsync(sql, new
        {
            TeamId = teamId,
            UserId = userId,
            Username = username,
            IpAddress = ipAddress
        });
    }

    public async Task<IEnumerable<TeamStanding>> GetStandingsAsync(int tournamentId, string? groupName = null)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT 
                team_id AS TeamId,
                team_name AS TeamName,
                team_code AS TeamCode,
                group_name AS GroupName,
                matches_played AS MatchesPlayed,
                wins AS Wins,
                draws AS Draws,
                losses AS Losses,
                goals_for AS GoalsFor,
                goals_against AS GoalsAgainst,
                goal_difference AS GoalDifference,
                points AS Points,
                standing_position AS StandingPosition
            FROM fn_get_standings(@TournamentId, @GroupName)";

        return await connection.QueryAsync<TeamStanding>(sql, new { TournamentId = tournamentId, GroupName = groupName });
    }
}
