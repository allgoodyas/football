// =====================================================
// FIXED GetGoldenBootStandingsAsync Method
// Replace the existing method in CFAAwardsRepository.cs
// =====================================================

using FootballTournament.API.Models.Entities;

public async Task<IEnumerable<GoldenBootStanding>> GetGoldenBootStandingsAsync(int tournamentId, int limit = 10, string? category = null)
{
    using var connection = _context.CreateConnection();
    
    try
    {
        // Fixed query that properly reads from match_events
        // Teams are linked to tournament, players are linked to teams
        const string sql = @"
            WITH player_goals AS (
                SELECT 
                    me.player_id,
                    me.team_id,
                    COUNT(*) AS total_goals,
                    COUNT(CASE WHEN COALESCE(me.goal_type, 'Regular') != 'ShootoutPenalty' THEN 1 END) AS eligible_goals,
                    COUNT(CASE WHEN COALESCE(me.goal_type, 'Regular') IN ('Regular', 'Goal') 
                               AND COALESCE(me.goal_type, 'Regular') != 'Penalty' THEN 1 END) AS regular_goals,
                    COUNT(CASE WHEN me.goal_type = 'Penalty' THEN 1 END) AS penalty_goals,
                    COUNT(CASE WHEN me.goal_type = 'ShootoutPenalty' THEN 1 END) AS shootout_goals
                FROM match_events me
                JOIN matches m ON me.match_id = m.match_id
                WHERE m.tournament_id = @TournamentId
                AND UPPER(me.event_type) IN ('GOAL', 'PENALTY_GOAL', 'PENALTYGOAL')
                AND me.player_id IS NOT NULL
                GROUP BY me.player_id, me.team_id
            ),
            player_assists AS (
                SELECT 
                    me.assist_player_id AS player_id,
                    COUNT(*) AS assists
                FROM match_events me
                JOIN matches m ON me.match_id = m.match_id
                WHERE m.tournament_id = @TournamentId
                AND me.assist_player_id IS NOT NULL
                AND UPPER(me.event_type) IN ('GOAL', 'PENALTY_GOAL', 'PENALTYGOAL')
                GROUP BY me.assist_player_id
            ),
            assist_events AS (
                SELECT 
                    me.player_id,
                    COUNT(*) AS assists
                FROM match_events me
                JOIN matches m ON me.match_id = m.match_id
                WHERE m.tournament_id = @TournamentId
                AND UPPER(me.event_type) = 'ASSIST'
                AND me.player_id IS NOT NULL
                GROUP BY me.player_id
            ),
            final_performance AS (
                SELECT 
                    me.player_id,
                    COUNT(CASE WHEN UPPER(me.event_type) IN ('GOAL', 'PENALTY_GOAL', 'PENALTYGOAL') THEN 1 END) AS final_goals,
                    COUNT(CASE WHEN UPPER(me.event_type) = 'ASSIST' OR me.assist_player_id IS NOT NULL THEN 1 END) AS final_assists,
                    true AS played_in_final
                FROM match_events me
                JOIN matches m ON me.match_id = m.match_id
                WHERE m.tournament_id = @TournamentId AND UPPER(m.round) = 'FINAL'
                GROUP BY me.player_id
            ),
            matches_played AS (
                SELECT 
                    me.player_id,
                    COUNT(DISTINCT me.match_id) AS matches
                FROM match_events me
                JOIN matches m ON me.match_id = m.match_id
                WHERE m.tournament_id = @TournamentId
                AND me.player_id IS NOT NULL
                GROUP BY me.player_id
            )
            SELECT 
                p.player_id AS PlayerId,
                COALESCE(p.first_name || ' ' || COALESCE(p.last_name, ''), p.first_name) AS PlayerName,
                p.team_id AS TeamId,
                t.team_name AS TeamName,
                t.team_code AS TeamCode,
                COALESCE(p.jersey_number, 0) AS JerseyNumber,
                COALESCE(p.position::TEXT, 'N/A') AS Position,
                COALESCE(pg.total_goals, 0) AS TotalGoals,
                COALESCE(pg.eligible_goals, 0) AS EligibleGoals,
                COALESCE(pg.regular_goals, 0) AS RegularGoals,
                COALESCE(pg.penalty_goals, 0) AS PenaltyGoals,
                COALESCE(pg.shootout_goals, 0) AS ShootoutGoals,
                COALESCE(pa.assists, 0) + COALESCE(ae.assists, 0) AS Assists,
                COALESCE(mp.matches, 0) AS MatchesPlayed,
                COALESCE(fp.final_goals, 0) AS FinalMatchGoals,
                COALESCE(fp.final_assists, 0) AS FinalMatchAssists,
                COALESCE(fp.played_in_final, false) AS PlayedInFinal,
                ROW_NUMBER() OVER (
                    ORDER BY 
                        COALESCE(pg.eligible_goals, 0) DESC,
                        COALESCE(pa.assists, 0) + COALESCE(ae.assists, 0) DESC,
                        COALESCE(fp.final_goals, 0) DESC,
                        COALESCE(fp.final_assists, 0) DESC
                ) AS Rank
            FROM players p
            JOIN teams t ON p.team_id = t.team_id
            LEFT JOIN player_goals pg ON p.player_id = pg.player_id
            LEFT JOIN player_assists pa ON p.player_id = pa.player_id
            LEFT JOIN assist_events ae ON p.player_id = ae.player_id
            LEFT JOIN final_performance fp ON p.player_id = fp.player_id
            LEFT JOIN matches_played mp ON p.player_id = mp.player_id
            WHERE t.tournament_id = @TournamentId
            AND p.is_active = true
            AND (COALESCE(pg.total_goals, 0) > 0 OR COALESCE(pa.assists, 0) > 0 OR COALESCE(ae.assists, 0) > 0)
            ORDER BY 
                COALESCE(pg.eligible_goals, 0) DESC,
                COALESCE(pa.assists, 0) + COALESCE(ae.assists, 0) DESC
            LIMIT @Limit";

        return await connection.QueryAsync<GoldenBootStanding>(sql, 
            new { TournamentId = tournamentId, Limit = limit });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Golden Boot query error: {ex.Message}");
        return Enumerable.Empty<GoldenBootStanding>();
    }
}
