using Dapper;
using FootballTournament.API.Data;
using FootballTournament.API.Models.Entities;

namespace FootballTournament.API.Repositories;

public interface IMatchRepository
{
    Task<IEnumerable<Match>> GetAllByTournamentAsync(int tournamentId);
    Task<Match?> GetByIdAsync(int matchId);
    Task<IEnumerable<Match>> GetUpcomingAsync(int tournamentId, int count = 5);
    Task<IEnumerable<Match>> GetBracketAsync(int tournamentId);
    Task<Match?> GetLiveMatchAsync(int matchId);
    Task<IEnumerable<MatchEvent>> GetEventsAsync(int matchId);
    Task<int> CreateAsync(int tournamentId, DateTime matchDate, string round, int? homeTeamId, int? awayTeamId,
        int? venueId, TimeSpan? matchTime, string? groupName, bool isKnockout, string? refereeName,
        string? notes, int? userId, string? username, string? ipAddress);
    Task UpdateAsync(int matchId, DateTime matchDate, string round, int? homeTeamId, int? awayTeamId,
        int? venueId, TimeSpan? matchTime, string? groupName, bool isKnockout, string? refereeName,
        string? notes, int? userId, string? username, string? ipAddress);
    Task UpdateScoreAsync(int matchId, int homeScore, int awayScore, string status, int? currentMinute,
        int? homeScoreHT, int? awayScoreHT, int? homePenalties, int? awayPenalties, bool isExtraTime, 
        bool isPenaltyShootout, int? userId, string? username, string? ipAddress);
    Task<int> AddEventAsync(int matchId, int teamId, string eventType, int eventMinute, int? playerId,
        bool isExtraTime, int? assistPlayerId, int? substitutedPlayerId, string? description,
        int? userId, string? username, string? ipAddress);
    Task<int> AddEventWithGoalTypeAsync(int matchId, int teamId, string eventType, int eventMinute, int? playerId,
        bool isExtraTime, int? assistPlayerId, int? substitutedPlayerId, string? description,
        string? goalType, int? userId, string? username, string? ipAddress);
    Task DeleteAsync(int matchId, int? userId, string? username, string? ipAddress);
}

public class MatchRepository : IMatchRepository
{
    private readonly IDbContext _context;

    public MatchRepository(IDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Match>> GetAllByTournamentAsync(int tournamentId)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT 
                match_id AS MatchId,
                tournament_id AS TournamentId,
                home_team_id AS HomeTeamId,
                away_team_id AS AwayTeamId,
                venue_id AS VenueId,
                match_date AS MatchDate,
                match_time AS MatchTime,
                match_number AS MatchNumber,
                round AS Round,
                group_name AS GroupName,
                home_score AS HomeScore,
                away_score AS AwayScore,
                home_score_ht AS HomeScoreHT,
                away_score_ht AS AwayScoreHT,
                home_penalties AS HomePenalties,
                away_penalties AS AwayPenalties,
                status::TEXT AS Status,
                winner_team_id AS WinnerTeamId,
                is_knockout AS IsKnockout,
                current_minute AS CurrentMinute,
                is_extra_time AS IsExtraTime,
                is_penalty_shootout AS IsPenaltyShootout,
                referee_name AS RefereeName,
                notes AS Notes,
                home_team_name AS HomeTeamName,
                home_team_code AS HomeTeamCode,
                away_team_name AS AwayTeamName,
                away_team_code AS AwayTeamCode,
                venue_name AS VenueName
            FROM fn_get_matches(@TournamentId)";

        return await connection.QueryAsync<Match>(sql, new { TournamentId = tournamentId });
    }

    public async Task<Match?> GetByIdAsync(int matchId)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT 
                match_id AS MatchId,
                tournament_id AS TournamentId,
                home_team_id AS HomeTeamId,
                away_team_id AS AwayTeamId,
                venue_id AS VenueId,
                match_date AS MatchDate,
                match_time AS MatchTime,
                match_number AS MatchNumber,
                round AS Round,
                group_name AS GroupName,
                home_score AS HomeScore,
                away_score AS AwayScore,
                home_score_ht AS HomeScoreHT,
                away_score_ht AS AwayScoreHT,
                home_penalties AS HomePenalties,
                away_penalties AS AwayPenalties,
                status::TEXT AS Status,
                winner_team_id AS WinnerTeamId,
                is_knockout AS IsKnockout,
                current_minute AS CurrentMinute,
                is_extra_time AS IsExtraTime,
                is_penalty_shootout AS IsPenaltyShootout,
                referee_name AS RefereeName,
                notes AS Notes,
                home_team_name AS HomeTeamName,
                home_team_code AS HomeTeamCode,
                away_team_name AS AwayTeamName,
                away_team_code AS AwayTeamCode,
                venue_name AS VenueName
            FROM fn_get_match_by_id(@MatchId)";

        return await connection.QueryFirstOrDefaultAsync<Match>(sql, new { MatchId = matchId });
    }

    public async Task<IEnumerable<Match>> GetUpcomingAsync(int tournamentId, int count = 5)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT 
                match_id AS MatchId,
                tournament_id AS TournamentId,
                home_team_id AS HomeTeamId,
                away_team_id AS AwayTeamId,
                venue_id AS VenueId,
                match_date AS MatchDate,
                match_time AS MatchTime,
                round AS Round,
                group_name AS GroupName,
                status::TEXT AS Status,
                is_knockout AS IsKnockout,
                home_team_name AS HomeTeamName,
                home_team_code AS HomeTeamCode,
                away_team_name AS AwayTeamName,
                away_team_code AS AwayTeamCode,
                venue_name AS VenueName
            FROM fn_get_upcoming_matches(@TournamentId, @Count)";

        return await connection.QueryAsync<Match>(sql, new { TournamentId = tournamentId, Count = count });
    }

    public async Task<IEnumerable<Match>> GetBracketAsync(int tournamentId)
    {
        using var connection = _context.CreateConnection();
        
        // First, try to get knockout matches only
        const string knockoutSql = @"
            SELECT 
                m.match_id AS MatchId,
                m.tournament_id AS TournamentId,
                m.home_team_id AS HomeTeamId,
                m.away_team_id AS AwayTeamId,
                m.match_date AS MatchDate,
                m.match_time AS MatchTime,
                m.round AS Round,
                m.group_name AS GroupName,
                m.home_score AS HomeScore,
                m.away_score AS AwayScore,
                m.status::TEXT AS Status,
                m.winner_team_id AS WinnerTeamId,
                m.is_knockout AS IsKnockout,
                ht.team_name AS HomeTeamName,
                ht.team_code AS HomeTeamCode,
                at.team_name AS AwayTeamName,
                at.team_code AS AwayTeamCode
            FROM matches m
            LEFT JOIN teams ht ON m.home_team_id = ht.team_id
            LEFT JOIN teams at ON m.away_team_id = at.team_id
            WHERE m.tournament_id = @TournamentId
              AND m.is_knockout = true
            ORDER BY 
                CASE m.round
                    WHEN 'Final' THEN 1
                    WHEN 'Semi-Final' THEN 2
                    WHEN 'Quarter-Final' THEN 3
                    WHEN 'Round of 16' THEN 4
                    WHEN 'Round of 32' THEN 5
                    ELSE 6
                END,
                m.match_date, m.match_number";

        var knockoutMatches = await connection.QueryAsync<Match>(knockoutSql, new { TournamentId = tournamentId });
        
        // If no knockout matches, return all matches for the tournament (for bracket display)
        if (!knockoutMatches.Any())
        {
            const string allMatchesSql = @"
                SELECT 
                    m.match_id AS MatchId,
                    m.tournament_id AS TournamentId,
                    m.home_team_id AS HomeTeamId,
                    m.away_team_id AS AwayTeamId,
                    m.match_date AS MatchDate,
                    m.match_time AS MatchTime,
                    m.round AS Round,
                    m.group_name AS GroupName,
                    m.home_score AS HomeScore,
                    m.away_score AS AwayScore,
                    m.status::TEXT AS Status,
                    m.winner_team_id AS WinnerTeamId,
                    m.is_knockout AS IsKnockout,
                    ht.team_name AS HomeTeamName,
                    ht.team_code AS HomeTeamCode,
                    at.team_name AS AwayTeamName,
                    at.team_code AS AwayTeamCode
                FROM matches m
                LEFT JOIN teams ht ON m.home_team_id = ht.team_id
                LEFT JOIN teams at ON m.away_team_id = at.team_id
                WHERE m.tournament_id = @TournamentId
                ORDER BY 
                    CASE m.round
                        WHEN 'Final' THEN 1
                        WHEN 'Semi-Final' THEN 2
                        WHEN 'Semi Final' THEN 2
                        WHEN 'Quarter-Final' THEN 3
                        WHEN 'Quarter Final' THEN 3
                        WHEN 'Round of 16' THEN 4
                        WHEN 'Round of 32' THEN 5
                        WHEN 'Group Stage' THEN 10
                        ELSE 6
                    END,
                    m.match_date, m.match_number";

            return await connection.QueryAsync<Match>(allMatchesSql, new { TournamentId = tournamentId });
        }
        
        return knockoutMatches;
    }

    public async Task<Match?> GetLiveMatchAsync(int matchId)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT 
                match_id AS MatchId,
                tournament_id AS TournamentId,
                home_team_id AS HomeTeamId,
                away_team_id AS AwayTeamId,
                venue_id AS VenueId,
                match_date AS MatchDate,
                match_time AS MatchTime,
                round AS Round,
                home_score AS HomeScore,
                away_score AS AwayScore,
                home_score_ht AS HomeScoreHT,
                away_score_ht AS AwayScoreHT,
                home_penalties AS HomePenalties,
                away_penalties AS AwayPenalties,
                status::TEXT AS Status,
                current_minute AS CurrentMinute,
                is_extra_time AS IsExtraTime,
                is_penalty_shootout AS IsPenaltyShootout,
                home_team_name AS HomeTeamName,
                home_team_code AS HomeTeamCode,
                away_team_name AS AwayTeamName,
                away_team_code AS AwayTeamCode,
                venue_name AS VenueName
            FROM fn_get_live_match(@MatchId)";

        return await connection.QueryFirstOrDefaultAsync<Match>(sql, new { MatchId = matchId });
    }

    public async Task<IEnumerable<MatchEvent>> GetEventsAsync(int matchId)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT 
                event_id AS EventId,
                match_id AS MatchId,
                team_id AS TeamId,
                player_id AS PlayerId,
                event_type AS EventType,
                event_minute AS EventMinute,
                is_extra_time AS IsExtraTime,
                assist_player_id AS AssistPlayerId,
                substituted_player_id AS SubstitutedPlayerId,
                description AS Description,
                team_name AS TeamName,
                team_code AS TeamCode,
                player_name AS PlayerName,
                jersey_number AS JerseyNumber,
                assist_player_name AS AssistPlayerName
            FROM fn_get_match_events(@MatchId)";

        return await connection.QueryAsync<MatchEvent>(sql, new { MatchId = matchId });
    }

    public async Task<int> CreateAsync(int tournamentId, DateTime matchDate, string round, int? homeTeamId, 
        int? awayTeamId, int? venueId, TimeSpan? matchTime, string? groupName, bool isKnockout, 
        string? refereeName, string? notes, int? userId, string? username, string? ipAddress)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT fn_create_match(
                @TournamentId, @MatchDate, @Round, @HomeTeamId, @AwayTeamId,
                @VenueId, @MatchTime, @GroupName, @IsKnockout, @RefereeName,
                @Notes, @UserId, @Username, @IpAddress
            )";

        return await connection.ExecuteScalarAsync<int>(sql, new
        {
            TournamentId = tournamentId,
            MatchDate = matchDate,
            Round = round,
            HomeTeamId = homeTeamId,
            AwayTeamId = awayTeamId,
            VenueId = venueId,
            MatchTime = matchTime,
            GroupName = groupName,
            IsKnockout = isKnockout,
            RefereeName = refereeName,
            Notes = notes,
            UserId = userId,
            Username = username,
            IpAddress = ipAddress
        });
    }

    public async Task UpdateAsync(int matchId, DateTime matchDate, string round, int? homeTeamId, 
        int? awayTeamId, int? venueId, TimeSpan? matchTime, string? groupName, bool isKnockout, 
        string? refereeName, string? notes, int? userId, string? username, string? ipAddress)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT fn_update_match(
                @MatchId, @MatchDate, @Round, @HomeTeamId, @AwayTeamId,
                @VenueId, @MatchTime, @GroupName, @IsKnockout, @RefereeName,
                @Notes, @UserId, @Username, @IpAddress
            )";

        await connection.ExecuteAsync(sql, new
        {
            MatchId = matchId,
            MatchDate = matchDate,
            Round = round,
            HomeTeamId = homeTeamId,
            AwayTeamId = awayTeamId,
            VenueId = venueId,
            MatchTime = matchTime,
            GroupName = groupName,
            IsKnockout = isKnockout,
            RefereeName = refereeName,
            Notes = notes,
            UserId = userId,
            Username = username,
            IpAddress = ipAddress
        });
    }

    public async Task UpdateScoreAsync(int matchId, int homeScore, int awayScore, string status, 
        int? currentMinute, int? homeScoreHT, int? awayScoreHT, int? homePenalties, int? awayPenalties,
        bool isExtraTime, bool isPenaltyShootout, int? userId, string? username, string? ipAddress)
    {
        try
        {

            using var connection = _context.CreateConnection();

            const string sql = @"
            SELECT fn_update_match_score(
                @MatchId, @HomeScore, @AwayScore, @Status, @CurrentMinute,
                @HomeScoreHT, @AwayScoreHT, @HomePenalties, @AwayPenalties,
                @IsExtraTime, @IsPenaltyShootout, @UserId, @Username, @IpAddress
            )";

            await connection.ExecuteAsync(sql, new
            {
                MatchId = matchId,
                HomeScore = homeScore,
                AwayScore = awayScore,
                Status = status,
                CurrentMinute = currentMinute,
                HomeScoreHT = homeScoreHT,
                AwayScoreHT = awayScoreHT,
                HomePenalties = homePenalties,
                AwayPenalties = awayPenalties,
                IsExtraTime = isExtraTime,
                IsPenaltyShootout = isPenaltyShootout,
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

    public async Task<int> AddEventAsync(int matchId, int teamId, string eventType, int eventMinute, 
        int? playerId, bool isExtraTime, int? assistPlayerId, int? substitutedPlayerId, 
        string? description, int? userId, string? username, string? ipAddress)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT fn_add_match_event(
                @MatchId, @TeamId, @EventType, @EventMinute, @PlayerId,
                @IsExtraTime, @AssistPlayerId, @SubstitutedPlayerId, @Description,
                @UserId, @Username, @IpAddress
            )";

        return await connection.ExecuteScalarAsync<int>(sql, new
        {
            MatchId = matchId,
            TeamId = teamId,
            EventType = eventType,
            EventMinute = eventMinute,
            PlayerId = playerId,
            IsExtraTime = isExtraTime,
            AssistPlayerId = assistPlayerId,
            SubstitutedPlayerId = substitutedPlayerId,
            Description = description,
            UserId = userId,
            Username = username,
            IpAddress = ipAddress
        });
    }

    /// <summary>
    /// Add match event with goal type (for CFA Tournament 2026)
    /// </summary>
    public async Task<int> AddEventWithGoalTypeAsync(int matchId, int teamId, string eventType, int eventMinute, 
        int? playerId, bool isExtraTime, int? assistPlayerId, int? substitutedPlayerId, 
        string? description, string? goalType, int? userId, string? username, string? ipAddress)
    {
        using var connection = _context.CreateConnection();
        
        // Try to use the new function with goal_type, fallback to direct insert if not available
        try
        {
            const string sql = @"
                INSERT INTO match_events (
                    match_id, team_id, event_type, event_minute, player_id,
                    is_extra_time, assist_player_id, substituted_player_id, description,
                    goal_type, created_on, created_by
                ) VALUES (
                    @MatchId, @TeamId, @EventType, @EventMinute, @PlayerId,
                    @IsExtraTime, @AssistPlayerId, @SubstitutedPlayerId, @Description,
                    @GoalType, NOW(), @UserId
                )
                RETURNING event_id";

            return await connection.ExecuteScalarAsync<int>(sql, new
            {
                MatchId = matchId,
                TeamId = teamId,
                EventType = eventType,
                EventMinute = eventMinute,
                PlayerId = playerId,
                IsExtraTime = isExtraTime,
                AssistPlayerId = assistPlayerId,
                SubstitutedPlayerId = substitutedPlayerId,
                Description = description,
                GoalType = goalType ?? "Regular",
                UserId = userId
            });
        }
        catch
        {
            // Fallback to existing method if goal_type column doesn't exist yet
            return await AddEventAsync(matchId, teamId, eventType, eventMinute, playerId,
                isExtraTime, assistPlayerId, substitutedPlayerId, description,
                userId, username, ipAddress);
        }
    }

    public async Task DeleteAsync(int matchId, int? userId, string? username, string? ipAddress)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = "SELECT fn_delete_match(@MatchId, @UserId, @Username, @IpAddress)";

        await connection.ExecuteAsync(sql, new
        {
            MatchId = matchId,
            UserId = userId,
            Username = username,
            IpAddress = ipAddress
        });
    }
}
