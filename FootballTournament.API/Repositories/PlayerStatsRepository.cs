using Dapper;
using FootballTournament.API.Data;
using FootballTournament.API.Models.Entities;

namespace FootballTournament.API.Repositories;

public interface IPlayerStatsRepository
{
    Task<IEnumerable<TopScorer>> GetTopScorersAsync(int tournamentId, int limit = 10);
    Task<IEnumerable<TopAssister>> GetTopAssistersAsync(int tournamentId, int limit = 10);
    Task<PlayerStatistics?> GetPlayerStatsAsync(int playerId, int tournamentId);
    Task<IEnumerable<TeamStatistics>> GetTeamStatisticsAsync(int tournamentId);
    Task<TournamentSummary?> GetTournamentSummaryAsync(int tournamentId);
    Task<IEnumerable<GoalsByTime>> GetGoalsByTimeAsync(int tournamentId);
    Task<IEnumerable<GoalsByMatchDay>> GetGoalsByMatchDayAsync(int tournamentId);
    Task<IEnumerable<CardedPlayer>> GetMostCardedPlayersAsync(int tournamentId, int limit = 10);
    Task<IEnumerable<ManOfTheMatch>> GetManOfTheMatchListAsync(int tournamentId);
    Task<IEnumerable<PlayerMatchStatsResponse>> GetMatchStatsAsync(int matchId);
    Task RecordPlayerMatchStatsAsync(int matchId, int playerId, int teamId, int goals, int assists,
        int yellowCards, int redCards, int minutesPlayed, bool isManOfTheMatch, int? userId, string? username, string? ipAddress);
    Task SetManOfTheMatchAsync(int matchId, int playerId, int teamId, int? userId, string? username, string? ipAddress);
}

// Extended response model for UI compatibility
public class PlayerMatchStatsResponse
{
    public int StatId { get; set; }
    public int MatchId { get; set; }
    public int PlayerId { get; set; }
    public int TeamId { get; set; }
    public string? PlayerName { get; set; }
    public int? JerseyNumber { get; set; }
    public string? Position { get; set; }
    public string? TeamName { get; set; }
    public string? TeamCode { get; set; }
    public int Goals { get; set; }
    public int Assists { get; set; }
    public int Saves { get; set; }
    public int Defends { get; set; }
    public int YellowCards { get; set; }
    public int RedCards { get; set; }
    public int MinutesPlayed { get; set; }
    public bool IsManOfTheMatch { get; set; }
    public int TotalPoints { get; set; }  // Goals*3 + Assists*2 + Saves*1 + Defends*1 - cards
}

public class PlayerStatsRepository : IPlayerStatsRepository
{
    private readonly IDbContext _context;

    public PlayerStatsRepository(IDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TopScorer>> GetTopScorersAsync(int tournamentId, int limit = 10)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT 
                player_id AS PlayerId,
                player_name AS PlayerName,
                team_id AS TeamId,
                team_name AS TeamName,
                team_code AS TeamCode,
                position AS Position,
                jersey_number AS JerseyNumber,
                goals AS Goals,
                assists AS Assists,
                matches_played AS MatchesPlayed,
                rank AS Rank
            FROM fn_get_top_scorers(@TournamentId, @Limit)";

        return await connection.QueryAsync<TopScorer>(sql, new { TournamentId = tournamentId, Limit = limit });
    }

    public async Task<IEnumerable<TopAssister>> GetTopAssistersAsync(int tournamentId, int limit = 10)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT 
                player_id AS PlayerId,
                player_name AS PlayerName,
                team_id AS TeamId,
                team_name AS TeamName,
                team_code AS TeamCode,
                position AS Position,
                jersey_number AS JerseyNumber,
                assists AS Assists,
                goals AS Goals,
                matches_played AS MatchesPlayed,
                rank AS Rank
            FROM fn_get_top_assisters(@TournamentId, @Limit)";

        return await connection.QueryAsync<TopAssister>(sql, new { TournamentId = tournamentId, Limit = limit });
    }

    public async Task<PlayerStatistics?> GetPlayerStatsAsync(int playerId, int tournamentId)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT 
                stat_id AS StatId,
                player_id AS PlayerId,
                tournament_id AS TournamentId,
                goals AS Goals,
                assists AS Assists,
                yellow_cards AS YellowCards,
                red_cards AS RedCards,
                matches_played AS MatchesPlayed,
                minutes_played AS MinutesPlayed,
                clean_sheets AS CleanSheets,
                saves AS Saves,
                man_of_the_match AS ManOfTheMatch
            FROM fn_get_player_stats(@PlayerId, @TournamentId)";

        return await connection.QueryFirstOrDefaultAsync<PlayerStatistics>(sql, 
            new { PlayerId = playerId, TournamentId = tournamentId });
    }

    public async Task<IEnumerable<TeamStatistics>> GetTeamStatisticsAsync(int tournamentId)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT 
                team_id AS TeamId,
                team_name AS TeamName,
                team_code AS TeamCode,
                matches_played AS MatchesPlayed,
                wins AS Wins,
                draws AS Draws,
                losses AS Losses,
                goals_for AS GoalsFor,
                goals_against AS GoalsAgainst,
                goal_difference AS GoalDifference,
                points AS Points,
                clean_sheets AS CleanSheets,
                yellow_cards AS YellowCards,
                red_cards AS RedCards,
                win_percentage AS WinPercentage,
                goals_per_match AS GoalsPerMatch
            FROM fn_get_team_statistics(@TournamentId)";

        return await connection.QueryAsync<TeamStatistics>(sql, new { TournamentId = tournamentId });
    }

    public async Task<TournamentSummary?> GetTournamentSummaryAsync(int tournamentId)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT 
                total_matches AS TotalMatches,
                completed_matches AS CompletedMatches,
                total_goals AS TotalGoals,
                average_goals_per_match AS AverageGoalsPerMatch,
                total_yellow_cards AS TotalYellowCards,
                total_red_cards AS TotalRedCards,
                home_wins AS HomeWins,
                away_wins AS AwayWins,
                draws AS Draws,
                total_players AS TotalPlayers,
                total_teams AS TotalTeams
            FROM fn_get_tournament_summary(@TournamentId)";

        return await connection.QueryFirstOrDefaultAsync<TournamentSummary>(sql, new { TournamentId = tournamentId });
    }

    public async Task<IEnumerable<GoalsByTime>> GetGoalsByTimeAsync(int tournamentId)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT 
                time_period AS TimePeriod,
                goals AS Goals,
                percentage AS Percentage
            FROM fn_get_goals_by_time(@TournamentId)";

        return await connection.QueryAsync<GoalsByTime>(sql, new { TournamentId = tournamentId });
    }

    public async Task<IEnumerable<GoalsByMatchDay>> GetGoalsByMatchDayAsync(int tournamentId)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT 
                match_day AS MatchDay,
                date AS Date,
                goals AS Goals,
                matches AS Matches
            FROM fn_get_goals_by_match_day(@TournamentId)";

        return await connection.QueryAsync<GoalsByMatchDay>(sql, new { TournamentId = tournamentId });
    }

    public async Task<IEnumerable<CardedPlayer>> GetMostCardedPlayersAsync(int tournamentId, int limit = 10)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT 
                player_id AS PlayerId,
                player_name AS PlayerName,
                team_id AS TeamId,
                team_name AS TeamName,
                yellow_cards AS YellowCards,
                red_cards AS RedCards,
                total_cards AS TotalCards
            FROM fn_get_most_carded_players(@TournamentId, @Limit)";

        return await connection.QueryAsync<CardedPlayer>(sql, new { TournamentId = tournamentId, Limit = limit });
    }

    public async Task<IEnumerable<ManOfTheMatch>> GetManOfTheMatchListAsync(int tournamentId)
    {
        try
        {

            using var connection = _context.CreateConnection();

            const string sql = @"
            SELECT 
                match_id AS MatchId,
                player_id AS PlayerId,
                player_name AS PlayerName,
                team_id AS TeamId,
                team_name AS TeamName,
                match_date AS MatchDate,
                opponent AS Opponent,
                goals AS Goals,
                assists AS Assists
            FROM fn_get_man_of_the_match_list(@TournamentId)";

            return await connection.QueryAsync<ManOfTheMatch>(sql, new { TournamentId = tournamentId });

        }
        catch (Exception Ex)
        {

            throw;
        }
    }

    public async Task<IEnumerable<PlayerMatchStatsResponse>> GetMatchStatsAsync(int matchId)
    {
        using var connection = _context.CreateConnection();
        
        // Get all player stats for this match from match_events and matches table
        const string sql = @"
            WITH player_match_stats AS (
                SELECT 
                    me.match_id,
                    me.player_id,
                    me.team_id,
                    p.first_name || ' ' || COALESCE(p.last_name, '') AS player_name,
                    p.jersey_number,
                    p.position,
                    t.team_name,
                    t.team_code,
                    SUM(CASE WHEN me.event_type = 'Goal' THEN 1 ELSE 0 END) AS goals,
                    SUM(CASE WHEN me.event_type = 'Assist' THEN 1 ELSE 0 END) AS assists,
                    SUM(CASE WHEN me.event_type = 'GoalSaved' THEN 1 ELSE 0 END) AS saves,
                    SUM(CASE WHEN me.event_type = 'Defend' THEN 1 ELSE 0 END) AS defends,
                    SUM(CASE WHEN me.event_type = 'YellowCard' THEN 1 ELSE 0 END) AS yellow_cards,
                    SUM(CASE WHEN me.event_type = 'RedCard' THEN 1 ELSE 0 END) AS red_cards
                FROM match_events me
                JOIN players p ON me.player_id = p.player_id
                JOIN teams t ON me.team_id = t.team_id
                WHERE me.match_id = @MatchId
                    AND me.player_id IS NOT NULL
                GROUP BY me.match_id, me.player_id, me.team_id, p.first_name, p.last_name, 
                         p.jersey_number, p.position, t.team_name, t.team_code
            )
            SELECT 
                ROW_NUMBER() OVER (ORDER BY (pms.goals * 3 + pms.assists * 2 + pms.saves + pms.defends) DESC) AS StatId,
                pms.match_id AS MatchId,
                pms.player_id AS PlayerId,
                pms.team_id AS TeamId,
                pms.player_name AS PlayerName,
                pms.jersey_number AS JerseyNumber,
                pms.position AS Position,
                pms.team_name AS TeamName,
                pms.team_code AS TeamCode,
                pms.goals AS Goals,
                pms.assists AS Assists,
                pms.saves AS Saves,
                pms.defends AS Defends,
                pms.yellow_cards AS YellowCards,
                pms.red_cards AS RedCards,
                0 AS MinutesPlayed,
                CASE WHEN m.mom_player_id = pms.player_id THEN TRUE ELSE FALSE END AS IsManOfTheMatch,
                (pms.goals * 3 + pms.assists * 2 + pms.saves + pms.defends - pms.yellow_cards - pms.red_cards * 3) AS TotalPoints
            FROM player_match_stats pms
            JOIN matches m ON pms.match_id = m.match_id
            ORDER BY TotalPoints DESC, pms.goals DESC, pms.assists DESC";

        return await connection.QueryAsync<PlayerMatchStatsResponse>(sql, new { MatchId = matchId });
    }

    public async Task RecordPlayerMatchStatsAsync(int matchId, int playerId, int teamId, int goals, int assists,
        int yellowCards, int redCards, int minutesPlayed, bool isManOfTheMatch, int? userId, string? username, string? ipAddress)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT fn_record_player_match_stats(
                @MatchId, @PlayerId, @TeamId, @Goals, @Assists,
                @YellowCards, @RedCards, @MinutesPlayed, @IsManOfTheMatch,
                @UserId, @Username, @IpAddress
            )";

        await connection.ExecuteAsync(sql, new
        {
            MatchId = matchId,
            PlayerId = playerId,
            TeamId = teamId,
            Goals = goals,
            Assists = assists,
            YellowCards = yellowCards,
            RedCards = redCards,
            MinutesPlayed = minutesPlayed,
            IsManOfTheMatch = isManOfTheMatch,
            UserId = userId,
            Username = username,
            IpAddress = ipAddress
        });
    }

    public async Task SetManOfTheMatchAsync(int matchId, int playerId, int teamId, int? userId, string? username, string? ipAddress)
    {
        using var connection = _context.CreateConnection();
        
        // First, clear any existing MOM for this match
        const string clearSql = @"
            UPDATE matches 
            SET mom_player_id = NULL, edited_on = NOW(), edited_by = @UserId 
            WHERE match_id = @MatchId";
        
        await connection.ExecuteAsync(clearSql, new { MatchId = matchId, UserId = userId });
        
        // Set the new MOM
        const string setSql = @"
            UPDATE matches 
            SET mom_player_id = @PlayerId, edited_on = NOW(), edited_by = @UserId 
            WHERE match_id = @MatchId";
        
        await connection.ExecuteAsync(setSql, new { MatchId = matchId, PlayerId = playerId, UserId = userId });
        
        // Also add a match event for tracking
        const string eventSql = @"
            INSERT INTO match_events (match_id, team_id, player_id, event_type, event_minute, description, created_on, created_by)
            VALUES (@MatchId, @TeamId, @PlayerId, 'ManOfTheMatch', 0, 'Man of the Match', NOW(), @UserId)
            ON CONFLICT DO NOTHING";
        
        try
        {
            await connection.ExecuteAsync(eventSql, new { MatchId = matchId, TeamId = teamId, PlayerId = playerId, UserId = userId });
        }
        catch
        {
            // Event may already exist, ignore
        }
    }
}
