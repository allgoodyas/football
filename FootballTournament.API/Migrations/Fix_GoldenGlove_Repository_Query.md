# Fix Golden Glove Repository Query

**Date:** 2026-01-31  
**Issue:** `GetGoldenGloveStandingsAsync` in `CFAAwardsRepository.cs` returns 0 results  
**Root Cause:** Query was using `goalkeeper_match_stats` table, but saves are stored in `match_events` table with `event_type = 'GoalSaved'`

## Corrected Query for CFAAwardsRepository.cs

Replace the `GetGoldenGloveStandingsAsync` method with:

```csharp
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
```

## Key Changes

| Old Query | New Query |
|-----------|-----------|
| Used `goalkeeper_match_stats` table | Uses `match_events` table |
| `gms.saves` column | `COUNT(*) WHERE event_type = 'GoalSaved'` |
| `gms.goals_conceded` column | Calculated from match scores (opponent's score) |
| Direct JOIN | `FULL OUTER JOIN` to capture all goalkeepers |

## Data Source

- **Saves**: `match_events.event_type = 'GoalSaved'`
- **Goals Conceded**: Calculated from `matches.home_score` / `matches.away_score` based on which team the goalkeeper belongs to
- **Clean Sheets**: Matches where `conceded = 0`

## Testing

```sql
-- Test query to verify saves in match_events
SELECT 
    me.player_id,
    p.first_name || ' ' || p.last_name AS player_name,
    me.event_type,
    COUNT(*) AS save_count
FROM match_events me
JOIN players p ON me.player_id = p.player_id
JOIN matches m ON me.match_id = m.match_id
WHERE m.tournament_id = 2
    AND me.event_type = 'GoalSaved'
GROUP BY me.player_id, p.first_name, p.last_name, me.event_type;
```

## Related Files

- `Repositories/CFAAwardsRepository.cs` - Contains the method to update
- `Migrations/Fix_Golden_Glove_Stats.sql` - Database function version (already created)
- `Models/Entities/Entities.cs` - Contains `GoldenGloveStanding` model
