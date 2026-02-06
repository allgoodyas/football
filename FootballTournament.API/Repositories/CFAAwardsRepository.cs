using Dapper;
using FootballTournament.API.Data;
using FootballTournament.API.Models.Entities;

namespace FootballTournament.API.Repositories;

public interface ICFAAwardsRepository
{
    // Goalkeeper Stats (Golden Glove)
    Task<int> RecordGoalkeeperStatsAsync(int matchId, int playerId, int teamId, int tournamentId,
        int saves, int goalsConceded, int penaltiesSaved, int penaltiesFaced, int minutesPlayed,
        int? userId, string? username, string? ipAddress);
    Task UpdateGoalkeeperStatsAsync(int goalkeeperStatsId, int saves, int goalsConceded,
        int penaltiesSaved, int penaltiesFaced, int minutesPlayed,
        int? userId, string? username, string? ipAddress);
    Task<GoalkeeperMatchStats?> GetGoalkeeperStatsByMatchAsync(int matchId, int playerId);
    Task<IEnumerable<GoalkeeperMatchStats>> GetGoalkeeperStatsByTournamentAsync(int tournamentId, int playerId);
    Task<IEnumerable<GoldenGloveStanding>> GetGoldenGloveStandingsAsync(int tournamentId, int limit = 10, bool eligibleOnly = false);
    
    // Defender Rating (Best Defender)
    Task<int> RecordDefenderRatingAsync(int matchId, int playerId, int teamId, int tournamentId,
        int ratingPosition, int tacklingScore, int interceptionScore, int markingScore, int blockingScore,
        string? notes, int? userId, string? username, string? ipAddress);
    Task<IEnumerable<DefenderMatchRating>> GetDefenderRatingsByMatchAsync(int matchId);
    Task<IEnumerable<DefenderMatchRating>> GetDefenderRatingsByPlayerAsync(int playerId, int tournamentId);
    Task<IEnumerable<BestDefenderStanding>> GetBestDefenderStandingsAsync(int tournamentId, int limit = 10);
    Task DeleteDefenderRatingsByMatchAsync(int matchId, int? userId, string? username, string? ipAddress);
    
    // Golden Boot (Enhanced)
    Task<IEnumerable<GoldenBootStanding>> GetGoldenBootStandingsAsync(int tournamentId, int limit = 10, string? category = null);
    
    // Award Summary (Airline Ticket)
    Task<IEnumerable<AwardSummary>> GetAwardSummaryAsync(int tournamentId, int limit = 10);
    
    // Walkover
    Task DeclareWalkoverAsync(int matchId, int winnerTeamId, int absentTeamId, string? reason,
        int delayMinutes, int? userId, string? username, string? ipAddress);
    Task<WalkoverResult?> GetWalkoverDetailsAsync(int matchId);
}

public class CFAAwardsRepository : ICFAAwardsRepository
{
    private readonly IDbContext _context;

    public CFAAwardsRepository(IDbContext context)
    {
        _context = context;
    }

    #region Goalkeeper Stats (Golden Glove)

    public async Task<int> RecordGoalkeeperStatsAsync(int matchId, int playerId, int teamId, int tournamentId,
        int saves, int goalsConceded, int penaltiesSaved, int penaltiesFaced, int minutesPlayed,
        int? userId, string? username, string? ipAddress)
    {
        using var connection = _context.CreateConnection();
        
        var isCleanSheet = goalsConceded == 0;
        
        const string sql = @"
            INSERT INTO goalkeeper_match_stats (
                match_id, player_id, team_id, tournament_id,
                saves, goals_conceded, penalties_saved, penalties_faced,
                minutes_played, is_clean_sheet, created_on, created_by
            ) VALUES (
                @MatchId, @PlayerId, @TeamId, @TournamentId,
                @Saves, @GoalsConceded, @PenaltiesSaved, @PenaltiesFaced,
                @MinutesPlayed, @IsCleanSheet, NOW(), @UserId
            )
            ON CONFLICT (match_id, player_id) DO UPDATE SET
                saves = EXCLUDED.saves,
                goals_conceded = EXCLUDED.goals_conceded,
                penalties_saved = EXCLUDED.penalties_saved,
                penalties_faced = EXCLUDED.penalties_faced,
                minutes_played = EXCLUDED.minutes_played,
                is_clean_sheet = EXCLUDED.is_clean_sheet,
                edited_on = NOW(),
                edited_by = @UserId
            RETURNING goalkeeper_stats_id";

        return await connection.ExecuteScalarAsync<int>(sql, new
        {
            MatchId = matchId,
            PlayerId = playerId,
            TeamId = teamId,
            TournamentId = tournamentId,
            Saves = saves,
            GoalsConceded = goalsConceded,
            PenaltiesSaved = penaltiesSaved,
            PenaltiesFaced = penaltiesFaced,
            MinutesPlayed = minutesPlayed,
            IsCleanSheet = isCleanSheet,
            UserId = userId
        });
    }

    public async Task UpdateGoalkeeperStatsAsync(int goalkeeperStatsId, int saves, int goalsConceded,
        int penaltiesSaved, int penaltiesFaced, int minutesPlayed,
        int? userId, string? username, string? ipAddress)
    {
        using var connection = _context.CreateConnection();
        
        var isCleanSheet = goalsConceded == 0;
        
        const string sql = @"
            UPDATE goalkeeper_match_stats SET
                saves = @Saves,
                goals_conceded = @GoalsConceded,
                penalties_saved = @PenaltiesSaved,
                penalties_faced = @PenaltiesFaced,
                minutes_played = @MinutesPlayed,
                is_clean_sheet = @IsCleanSheet,
                edited_on = NOW(),
                edited_by = @UserId
            WHERE goalkeeper_stats_id = @GoalkeeperStatsId";

        await connection.ExecuteAsync(sql, new
        {
            GoalkeeperStatsId = goalkeeperStatsId,
            Saves = saves,
            GoalsConceded = goalsConceded,
            PenaltiesSaved = penaltiesSaved,
            PenaltiesFaced = penaltiesFaced,
            MinutesPlayed = minutesPlayed,
            IsCleanSheet = isCleanSheet,
            UserId = userId
        });
    }

    public async Task<GoalkeeperMatchStats?> GetGoalkeeperStatsByMatchAsync(int matchId, int playerId)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT 
                gms.goalkeeper_stats_id AS GoalkeeperStatsId,
                gms.match_id AS MatchId,
                gms.player_id AS PlayerId,
                gms.team_id AS TeamId,
                gms.tournament_id AS TournamentId,
                gms.saves AS Saves,
                gms.goals_conceded AS GoalsConceded,
                gms.penalties_saved AS PenaltiesSaved,
                gms.penalties_faced AS PenaltiesFaced,
                gms.minutes_played AS MinutesPlayed,
                gms.is_clean_sheet AS IsCleanSheet,
                COALESCE(p.first_name || ' ' || p.last_name, p.first_name) AS PlayerName,
                t.team_name AS TeamName,
                t.team_code AS TeamCode,
                m.match_date AS MatchDate
            FROM goalkeeper_match_stats gms
            JOIN players p ON gms.player_id = p.player_id
            JOIN teams t ON gms.team_id = t.team_id
            JOIN matches m ON gms.match_id = m.match_id
            WHERE gms.match_id = @MatchId AND gms.player_id = @PlayerId";

        return await connection.QueryFirstOrDefaultAsync<GoalkeeperMatchStats>(sql, 
            new { MatchId = matchId, PlayerId = playerId });
    }

    public async Task<IEnumerable<GoalkeeperMatchStats>> GetGoalkeeperStatsByTournamentAsync(int tournamentId, int playerId)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT 
                gms.goalkeeper_stats_id AS GoalkeeperStatsId,
                gms.match_id AS MatchId,
                gms.player_id AS PlayerId,
                gms.team_id AS TeamId,
                gms.tournament_id AS TournamentId,
                gms.saves AS Saves,
                gms.goals_conceded AS GoalsConceded,
                gms.penalties_saved AS PenaltiesSaved,
                gms.penalties_faced AS PenaltiesFaced,
                gms.minutes_played AS MinutesPlayed,
                gms.is_clean_sheet AS IsCleanSheet,
                COALESCE(p.first_name || ' ' || p.last_name, p.first_name) AS PlayerName,
                t.team_name AS TeamName,
                t.team_code AS TeamCode,
                m.match_date AS MatchDate
            FROM goalkeeper_match_stats gms
            JOIN players p ON gms.player_id = p.player_id
            JOIN teams t ON gms.team_id = t.team_id
            JOIN matches m ON gms.match_id = m.match_id
            WHERE gms.tournament_id = @TournamentId AND gms.player_id = @PlayerId
            ORDER BY m.match_date DESC";

        return await connection.QueryAsync<GoalkeeperMatchStats>(sql, 
            new { TournamentId = tournamentId, PlayerId = playerId });
    }

    public async Task<IEnumerable<GoldenGloveStanding>> GetGoldenGloveStandingsAsync(int tournamentId, int limit = 10, bool eligibleOnly = false)
    {
        using var connection = _context.CreateConnection();

        try
        {
            var eligibleFilter = eligibleOnly ? "AND t.is_eliminated = false" : "";

            var sql = $@"
            WITH goalkeeper_saves AS (
                -- Get saves from match_events for each goalkeeper per match
                SELECT 
                    me.player_id,
                    me.team_id,
                    me.match_id,
                    COUNT(*) AS saves_in_match
                FROM match_events me
                JOIN players p ON me.player_id = p.player_id
                JOIN matches m ON me.match_id = m.match_id
                WHERE m.tournament_id = @TournamentId
                    AND me.event_type = 'GoalSaved'
                    AND p.position = 'GK'
                GROUP BY me.player_id, me.team_id, me.match_id
            ),
            goals_conceded AS (
                -- Goals conceded = goals scored by opponent team against this goalkeeper's team
                SELECT 
                    p.player_id,
                    p.team_id,
                    m.match_id,
                    CASE 
                        WHEN p.team_id = m.home_team_id THEN COALESCE(m.away_score, 0)
                        ELSE COALESCE(m.home_score, 0)
                    END AS conceded_in_match
                FROM players p
                JOIN matches m ON (m.home_team_id = p.team_id OR m.away_team_id = p.team_id)
                JOIN teams t ON p.team_id = t.team_id
                WHERE m.tournament_id = @TournamentId
                    AND m.status = 'Completed'
                    AND p.position = 'GK'
                    AND p.is_active = TRUE
                    {eligibleFilter}
            ),
            goalkeeper_match_data AS (
                -- Combine saves and goals conceded per match
                SELECT 
                    COALESCE(gs.player_id, gc.player_id) AS player_id,
                    COALESCE(gs.team_id, gc.team_id) AS team_id,
                    COALESCE(gs.match_id, gc.match_id) AS match_id,
                    COALESCE(gs.saves_in_match, 0) AS saves,
                    COALESCE(gc.conceded_in_match, 0) AS conceded
                FROM goalkeeper_saves gs
                FULL OUTER JOIN goals_conceded gc 
                    ON gs.player_id = gc.player_id 
                    AND gs.match_id = gc.match_id
            ),
            goalkeeper_totals AS (
                SELECT 
                    gmd.player_id,
                    TRIM(p.first_name || ' ' || COALESCE(p.last_name, '')) AS player_name,
                    gmd.team_id,
                    t.team_name,
                    t.team_code,
                    p.jersey_number,
                    SUM(gmd.saves) AS total_saves,
                    SUM(gmd.conceded) AS total_goals_conceded,
                    0::BIGINT AS total_penalties_saved,
                    COUNT(DISTINCT gmd.match_id) AS matches_played,
                    SUM(CASE WHEN gmd.conceded = 0 THEN 1 ELSE 0 END) AS clean_sheets,
                    SUM(gmd.saves) - SUM(gmd.conceded) AS golden_glove_points,
                    EXISTS (
                        SELECT 1 FROM matches m 
                        WHERE m.tournament_id = @TournamentId 
                        AND m.round = 'Final'
                        AND (m.home_team_id = gmd.team_id OR m.away_team_id = gmd.team_id)
                    ) AS reached_final
                FROM goalkeeper_match_data gmd
                JOIN players p ON gmd.player_id = p.player_id
                JOIN teams t ON gmd.team_id = t.team_id
                WHERE gmd.player_id IS NOT NULL
                GROUP BY gmd.player_id, p.first_name, p.last_name, gmd.team_id, t.team_name, t.team_code, p.jersey_number
            ),
            final_stats AS (
                SELECT 
                    gmd.player_id,
                    gmd.saves AS final_match_saves,
                    gmd.conceded AS final_match_conceded,
                    (gmd.saves - gmd.conceded) AS final_match_points
                FROM goalkeeper_match_data gmd
                JOIN matches m ON gmd.match_id = m.match_id
                WHERE m.tournament_id = @TournamentId AND m.round = 'Final'
            )
            SELECT 
                gt.player_id AS PlayerId,
                gt.player_name AS PlayerName,
                gt.team_id AS TeamId,
                gt.team_name AS TeamName,
                gt.team_code AS TeamCode,
                gt.jersey_number AS JerseyNumber,
                gt.total_saves AS TotalSaves,
                gt.total_goals_conceded AS TotalGoalsConceded,
                gt.total_penalties_saved AS TotalPenaltiesSaved,
                gt.matches_played AS MatchesPlayed,
                gt.clean_sheets AS CleanSheets,
                gt.golden_glove_points AS GoldenGlovePoints,
                gt.reached_final AS ReachedFinal,
                COALESCE(fs.final_match_saves, 0) AS FinalMatchSaves,
                COALESCE(fs.final_match_conceded, 0) AS FinalMatchConceded,
                COALESCE(fs.final_match_points, 0) AS FinalMatchPoints,
                ROW_NUMBER() OVER (
                    ORDER BY 
                        gt.golden_glove_points DESC,
                        COALESCE(fs.final_match_points, 0) DESC,
                        gt.clean_sheets DESC,
                        gt.total_saves DESC
                ) AS Rank
            FROM goalkeeper_totals gt
            LEFT JOIN final_stats fs ON gt.player_id = fs.player_id
            WHERE gt.total_saves > 0 OR gt.matches_played > 0
            ORDER BY Rank
            LIMIT @Limit";

            return await connection.QueryAsync<GoldenGloveStanding>(sql, new { TournamentId = tournamentId, Limit = limit });
        }
        catch
        {
            return Enumerable.Empty<GoldenGloveStanding>();
        }
    }
    #endregion

    #region Defender Rating (Best Defender)

    public async Task<int> RecordDefenderRatingAsync(int matchId, int playerId, int teamId, int tournamentId,
        int ratingPosition, int tacklingScore, int interceptionScore, int markingScore, int blockingScore,
        string? notes, int? userId, string? username, string? ipAddress)
    {
        using var connection = _context.CreateConnection();
        
        var pointsAwarded = ratingPosition == 1 ? 2 : 1;
        
        const string sql = @"
            INSERT INTO defender_match_ratings (
                match_id, player_id, team_id, tournament_id,
                rating_position, points_awarded,
                tackling_score, interception_score, marking_score, blocking_score,
                notes, created_on, created_by
            ) VALUES (
                @MatchId, @PlayerId, @TeamId, @TournamentId,
                @RatingPosition, @PointsAwarded,
                @TacklingScore, @InterceptionScore, @MarkingScore, @BlockingScore,
                @Notes, NOW(), @UserId
            )
            RETURNING rating_id";

        return await connection.ExecuteScalarAsync<int>(sql, new
        {
            MatchId = matchId,
            PlayerId = playerId,
            TeamId = teamId,
            TournamentId = tournamentId,
            RatingPosition = ratingPosition,
            PointsAwarded = pointsAwarded,
            TacklingScore = tacklingScore,
            InterceptionScore = interceptionScore,
            MarkingScore = markingScore,
            BlockingScore = blockingScore,
            Notes = notes,
            UserId = userId
        });
    }

    public async Task<IEnumerable<DefenderMatchRating>> GetDefenderRatingsByMatchAsync(int matchId)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT 
                dmr.rating_id AS RatingId,
                dmr.match_id AS MatchId,
                dmr.player_id AS PlayerId,
                dmr.team_id AS TeamId,
                dmr.tournament_id AS TournamentId,
                dmr.rating_position AS RatingPosition,
                dmr.points_awarded AS PointsAwarded,
                dmr.tackling_score AS TacklingScore,
                dmr.interception_score AS InterceptionScore,
                dmr.marking_score AS MarkingScore,
                dmr.blocking_score AS BlockingScore,
                dmr.notes AS Notes,
                COALESCE(p.first_name || ' ' || p.last_name, p.first_name) AS PlayerName,
                p.jersey_number AS JerseyNumber,
                t.team_name AS TeamName,
                t.team_code AS TeamCode,
                m.match_date AS MatchDate
            FROM defender_match_ratings dmr
            JOIN players p ON dmr.player_id = p.player_id
            JOIN teams t ON dmr.team_id = t.team_id
            JOIN matches m ON dmr.match_id = m.match_id
            WHERE dmr.match_id = @MatchId
            ORDER BY dmr.rating_position";

        return await connection.QueryAsync<DefenderMatchRating>(sql, new { MatchId = matchId });
    }

    public async Task<IEnumerable<DefenderMatchRating>> GetDefenderRatingsByPlayerAsync(int playerId, int tournamentId)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT 
                dmr.rating_id AS RatingId,
                dmr.match_id AS MatchId,
                dmr.player_id AS PlayerId,
                dmr.team_id AS TeamId,
                dmr.tournament_id AS TournamentId,
                dmr.rating_position AS RatingPosition,
                dmr.points_awarded AS PointsAwarded,
                dmr.tackling_score AS TacklingScore,
                dmr.interception_score AS InterceptionScore,
                dmr.marking_score AS MarkingScore,
                dmr.blocking_score AS BlockingScore,
                dmr.notes AS Notes,
                COALESCE(p.first_name || ' ' || p.last_name, p.first_name) AS PlayerName,
                p.jersey_number AS JerseyNumber,
                t.team_name AS TeamName,
                t.team_code AS TeamCode,
                m.match_date AS MatchDate
            FROM defender_match_ratings dmr
            JOIN players p ON dmr.player_id = p.player_id
            JOIN teams t ON dmr.team_id = t.team_id
            JOIN matches m ON dmr.match_id = m.match_id
            WHERE dmr.player_id = @PlayerId AND dmr.tournament_id = @TournamentId
            ORDER BY m.match_date DESC";

        return await connection.QueryAsync<DefenderMatchRating>(sql, 
            new { PlayerId = playerId, TournamentId = tournamentId });
    }

    public async Task<IEnumerable<BestDefenderStanding>> GetBestDefenderStandingsAsync(int tournamentId, int limit = 10)
    {
        using var connection = _context.CreateConnection();
        
        try
        {
            const string sql = @"
                SELECT 
                    dmr.player_id AS PlayerId,
                    COALESCE(p.first_name || ' ' || p.last_name, p.first_name) AS PlayerName,
                    dmr.team_id AS TeamId,
                    t.team_name AS TeamName,
                    t.team_code AS TeamCode,
                    p.jersey_number AS JerseyNumber,
                    p.position::TEXT AS Position,
                    SUM(dmr.points_awarded) AS TotalPoints,
                    COUNT(*) AS MatchesRated,
                    SUM(CASE WHEN dmr.rating_position = 1 THEN 1 ELSE 0 END) AS FirstPlaceCount,
                    SUM(CASE WHEN dmr.rating_position = 2 THEN 1 ELSE 0 END) AS SecondPlaceCount,
                    ROUND(AVG((dmr.tackling_score + dmr.interception_score + dmr.marking_score + dmr.blocking_score) / 4.0), 2) AS AverageScore,
                    ROW_NUMBER() OVER (ORDER BY SUM(dmr.points_awarded) DESC, COUNT(*) DESC) AS Rank
                FROM defender_match_ratings dmr
                JOIN players p ON dmr.player_id = p.player_id
                JOIN teams t ON dmr.team_id = t.team_id
                WHERE dmr.tournament_id = @TournamentId
                GROUP BY dmr.player_id, p.first_name, p.last_name, dmr.team_id, t.team_name, t.team_code, p.jersey_number, p.position
                ORDER BY TotalPoints DESC, MatchesRated DESC
                LIMIT @Limit";

            return await connection.QueryAsync<BestDefenderStanding>(sql, 
                new { TournamentId = tournamentId, Limit = limit });
        }
        catch
        {
            return Enumerable.Empty<BestDefenderStanding>();
        }
    }

    public async Task DeleteDefenderRatingsByMatchAsync(int matchId, int? userId, string? username, string? ipAddress)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = "DELETE FROM defender_match_ratings WHERE match_id = @MatchId";
        await connection.ExecuteAsync(sql, new { MatchId = matchId });
    }

    #endregion

    #region Golden Boot (Enhanced)    

    public async Task<IEnumerable<GoldenBootStanding>> GetGoldenBootStandingsAsync(int tournamentId, int limit = 10, string? category = null)
    {
        using var connection = _context.CreateConnection();
        
        try
        {
            // Note: This query assumes match_events has a goal_type column
            // If not, we treat all goals as Regular except during penalty shootout phase
            const string sql = @"
                WITH player_goals AS (
                    SELECT 
                        me.player_id,
                        COUNT(*) AS total_goals,
                        -- Count eligible goals (excluding shootout penalties)
                        COUNT(CASE WHEN COALESCE(me.goal_type, 
                            CASE WHEN m.is_penalty_shootout AND me.event_minute > 120 THEN 'ShootoutPenalty' ELSE 'Regular' END
                        ) != 'ShootoutPenalty' THEN 1 END) AS eligible_goals,
                        -- Goal breakdown
                        COUNT(CASE WHEN COALESCE(me.goal_type, 'Regular') = 'Regular' THEN 1 END) AS regular_goals,
                        COUNT(CASE WHEN me.goal_type = 'Penalty' THEN 1 END) AS penalty_goals,
                        COUNT(CASE WHEN COALESCE(me.goal_type, 
                            CASE WHEN m.is_penalty_shootout AND me.event_minute > 120 THEN 'ShootoutPenalty' ELSE 'Regular' END
                        ) = 'ShootoutPenalty' THEN 1 END) AS shootout_goals
                    FROM match_events me
                    JOIN matches m ON me.match_id = m.match_id
                    WHERE m.tournament_id = @TournamentId
                    AND me.event_type IN ('GOAL', 'Goal', 'PENALTY_GOAL', 'PenaltyGoal')
                    AND me.player_id IS NOT NULL
                    GROUP BY me.player_id
                ),
                player_assists AS (
                    SELECT 
                        COALESCE(me.assist_player_id, me.player_id) AS player_id,
                        COUNT(*) AS assists
                    FROM match_events me
                    JOIN matches m ON me.match_id = m.match_id
                    WHERE m.tournament_id = @TournamentId
                    AND (me.event_type IN ('ASSIST', 'Assist') OR me.assist_player_id IS NOT NULL)
                    GROUP BY COALESCE(me.assist_player_id, me.player_id)
                ),
                final_performance AS (
                    SELECT 
                        me.player_id,
                        COUNT(CASE WHEN me.event_type IN ('GOAL', 'Goal') THEN 1 END) AS final_goals,
                        COUNT(CASE WHEN me.event_type IN ('ASSIST', 'Assist') OR me.assist_player_id IS NOT NULL THEN 1 END) AS final_assists,
                        true AS played_in_final
                    FROM match_events me
                    JOIN matches m ON me.match_id = m.match_id
                    WHERE m.tournament_id = @TournamentId AND m.round = 'Final'
                    GROUP BY me.player_id
                ),
                matches_played AS (
                    SELECT 
                        me.player_id,
                        COUNT(DISTINCT me.match_id) AS matches
                    FROM match_events me
                    JOIN matches m ON me.match_id = m.match_id
                    WHERE m.tournament_id = @TournamentId
                    GROUP BY me.player_id
                )
                SELECT 
                    p.player_id AS PlayerId,
                    COALESCE(p.first_name || ' ' || p.last_name, p.first_name) AS PlayerName,
                    p.team_id AS TeamId,
                    t.team_name AS TeamName,
                    t.team_code AS TeamCode,
                    p.jersey_number AS JerseyNumber,
                    p.position::TEXT AS Position,
                    COALESCE(pg.total_goals, 0) AS TotalGoals,
                    COALESCE(pg.eligible_goals, 0) AS EligibleGoals,
                    COALESCE(pg.regular_goals, 0) AS RegularGoals,
                    COALESCE(pg.penalty_goals, 0) AS PenaltyGoals,
                    COALESCE(pg.shootout_goals, 0) AS ShootoutGoals,
                    COALESCE(pa.assists, 0) AS Assists,
                    COALESCE(mp.matches, 0) AS MatchesPlayed,
                    COALESCE(fp.final_goals, 0) AS FinalMatchGoals,
                    COALESCE(fp.final_assists, 0) AS FinalMatchAssists,
                    COALESCE(fp.played_in_final, false) AS PlayedInFinal,
                    ROW_NUMBER() OVER (
                        ORDER BY 
                            COALESCE(pg.eligible_goals, 0) DESC,
                            COALESCE(pa.assists, 0) DESC,
                            COALESCE(fp.final_goals, 0) DESC,
                            COALESCE(fp.final_assists, 0) DESC
                    ) AS Rank
                FROM players p
                JOIN teams t ON p.team_id = t.team_id
                LEFT JOIN player_goals pg ON p.player_id = pg.player_id
                LEFT JOIN player_assists pa ON p.player_id = pa.player_id
                LEFT JOIN final_performance fp ON p.player_id = fp.player_id
                LEFT JOIN matches_played mp ON p.player_id = mp.player_id
                WHERE t.tournament_id = @TournamentId
                AND p.is_active = true
                AND (COALESCE(pg.total_goals, 0) > 0 OR COALESCE(pa.assists, 0) > 0)
                ORDER BY Rank
                LIMIT @Limit";

            return await connection.QueryAsync<GoldenBootStanding>(sql, 
                new { TournamentId = tournamentId, Limit = limit });
        }
        catch
        {
            return Enumerable.Empty<GoldenBootStanding>();
        }
    }

    #endregion

    #region Award Summary (Airline Ticket)

    public async Task<IEnumerable<AwardSummary>> GetAwardSummaryAsync(int tournamentId, int limit = 10)
    {
        using var connection = _context.CreateConnection();
        
        try
        {
            const string sql = @"
                WITH golden_boot AS (
                    SELECT 
                        me.player_id,
                        COUNT(CASE WHEN me.event_type IN ('GOAL', 'Goal') THEN 1 END) AS goals,
                        COUNT(CASE WHEN me.event_type IN ('ASSIST', 'Assist') OR me.assist_player_id IS NOT NULL THEN 1 END) AS assists,
                        (COUNT(CASE WHEN me.event_type IN ('GOAL', 'Goal') THEN 1 END) * 2) +
                        COUNT(CASE WHEN me.event_type IN ('ASSIST', 'Assist') OR me.assist_player_id IS NOT NULL THEN 1 END) AS points
                    FROM match_events me
                    JOIN matches m ON me.match_id = m.match_id
                    WHERE m.tournament_id = @TournamentId
                    GROUP BY me.player_id
                ),
                golden_glove AS (
                    SELECT 
                        player_id,
                        SUM(saves - goals_conceded) AS points
                    FROM goalkeeper_match_stats
                    WHERE tournament_id = @TournamentId
                    GROUP BY player_id
                ),
                best_defender AS (
                    SELECT 
                        player_id,
                        SUM(points_awarded) AS points
                    FROM defender_match_ratings
                    WHERE tournament_id = @TournamentId
                    GROUP BY player_id
                )
                SELECT 
                    p.player_id AS PlayerId,
                    COALESCE(p.first_name || ' ' || p.last_name, p.first_name) AS PlayerName,
                    p.team_id AS TeamId,
                    t.team_name AS TeamName,
                    t.team_code AS TeamCode,
                    p.jersey_number AS JerseyNumber,
                    p.position::TEXT AS Position,
                    trn.category AS Category,
                    COALESCE(gb.goals, 0) AS GoldenBootGoals,
                    COALESCE(gb.assists, 0) AS GoldenBootAssists,
                    COALESCE(gb.points, 0) AS GoldenBootPoints,
                    COALESCE(gg.points, 0) AS GoldenGlovePoints,
                    COALESCE(bd.points, 0) AS BestDefenderPoints,
                    COALESCE(gb.points, 0) + COALESCE(gg.points, 0) + COALESCE(bd.points, 0) AS TotalAirlineTicketPoints,
                    ROW_NUMBER() OVER (
                        ORDER BY COALESCE(gb.points, 0) + COALESCE(gg.points, 0) + COALESCE(bd.points, 0) DESC
                    ) AS Rank
                FROM players p
                JOIN teams t ON p.team_id = t.team_id
                JOIN tournaments trn ON t.tournament_id = trn.tournament_id
                LEFT JOIN golden_boot gb ON p.player_id = gb.player_id
                LEFT JOIN golden_glove gg ON p.player_id = gg.player_id
                LEFT JOIN best_defender bd ON p.player_id = bd.player_id
                WHERE t.tournament_id = @TournamentId
                AND p.is_active = true
                AND trn.category = 'Senior'
                AND (COALESCE(gb.points, 0) > 0 OR COALESCE(gg.points, 0) > 0 OR COALESCE(bd.points, 0) > 0)
                ORDER BY TotalAirlineTicketPoints DESC
                LIMIT @Limit";

            return await connection.QueryAsync<AwardSummary>(sql, 
                new { TournamentId = tournamentId, Limit = limit });
        }
        catch
        {
            return Enumerable.Empty<AwardSummary>();
        }
    }

    #endregion

    #region Walkover

    public async Task DeclareWalkoverAsync(int matchId, int winnerTeamId, int absentTeamId, string? reason,
        int delayMinutes, int? userId, string? username, string? ipAddress)
    {
        using var connection = _context.CreateConnection();
        
        // Determine home/away based on winner
        const string getMatchSql = "SELECT home_team_id, away_team_id FROM matches WHERE match_id = @MatchId";
        var match = await connection.QueryFirstOrDefaultAsync<dynamic>(getMatchSql, new { MatchId = matchId });
        
        if (match == null) throw new Exception("Match not found");
        
        int homeScore = 0, awayScore = 0;
        if (match.home_team_id == winnerTeamId)
        {
            homeScore = 3;
            awayScore = 0;
        }
        else
        {
            homeScore = 0;
            awayScore = 3;
        }
        
        // Update match with walkover result
        const string updateMatchSql = @"
            UPDATE matches SET
                home_score = @HomeScore,
                away_score = @AwayScore,
                status = 'Walkover',
                winner_team_id = @WinnerTeamId,
                notes = COALESCE(notes || ' | ', '') || 'WALKOVER: ' || @Reason,
                edited_on = NOW(),
                edited_by = @UserId
            WHERE match_id = @MatchId";

        await connection.ExecuteAsync(updateMatchSql, new
        {
            MatchId = matchId,
            HomeScore = homeScore,
            AwayScore = awayScore,
            WinnerTeamId = winnerTeamId,
            Reason = reason ?? $"Opponent absent. Delay: {delayMinutes} minutes.",
            UserId = userId
        });

        // Try to insert walkover details if table exists
        try
        {
            const string insertWalkoverSql = @"
                INSERT INTO match_walkovers (
                    match_id, winner_team_id, absent_team_id, reason,
                    delay_minutes, declared_at, declared_by, winner_score, loser_score
                ) VALUES (
                    @MatchId, @WinnerTeamId, @AbsentTeamId, @Reason,
                    @DelayMinutes, NOW(), @DeclaredBy, 3, 0
                )
                ON CONFLICT (match_id) DO UPDATE SET
                    winner_team_id = EXCLUDED.winner_team_id,
                    absent_team_id = EXCLUDED.absent_team_id,
                    reason = EXCLUDED.reason,
                    delay_minutes = EXCLUDED.delay_minutes,
                    declared_at = NOW()";

            await connection.ExecuteAsync(insertWalkoverSql, new
            {
                MatchId = matchId,
                WinnerTeamId = winnerTeamId,
                AbsentTeamId = absentTeamId,
                Reason = reason ?? "Team absent",
                DelayMinutes = delayMinutes,
                DeclaredBy = username ?? "System"
            });
        }
        catch
        {
            // Table doesn't exist yet, ignore
        }
    }

    public async Task<WalkoverResult?> GetWalkoverDetailsAsync(int matchId)
    {
        using var connection = _context.CreateConnection();
        
        try
        {
            const string sql = @"
                SELECT 
                    mw.match_id AS MatchId,
                    mw.winner_team_id AS WinnerTeamId,
                    mw.absent_team_id AS AbsentTeamId,
                    mw.reason AS Reason,
                    mw.delay_minutes AS DelayMinutes,
                    mw.declared_at AS DeclaredAt,
                    mw.declared_by AS DeclaredBy,
                    mw.winner_score AS WinnerScore,
                    mw.loser_score AS LoserScore,
                    wt.team_name AS WinnerTeamName,
                    at.team_name AS AbsentTeamName
                FROM match_walkovers mw
                JOIN teams wt ON mw.winner_team_id = wt.team_id
                LEFT JOIN teams at ON mw.absent_team_id = at.team_id
                WHERE mw.match_id = @MatchId";

            return await connection.QueryFirstOrDefaultAsync<WalkoverResult>(sql, new { MatchId = matchId });
        }
        catch
        {
            return null;
        }
    }

    #endregion
}
