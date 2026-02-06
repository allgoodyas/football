-- Migration: Fix Golden Glove to use match_events table
-- Date: 2025-01-31
-- Description: Rewrite Golden Glove query to calculate saves from match_events

-- Drop existing function if exists
DROP FUNCTION IF EXISTS fn_get_golden_glove_stats(INTEGER, INTEGER);

-- Create the function
CREATE OR REPLACE FUNCTION fn_get_golden_glove_stats(p_tournament_id INTEGER, p_limit INTEGER DEFAULT 10)
RETURNS TABLE (
    player_id INTEGER,
    player_name TEXT,
    team_id INTEGER,
    team_name VARCHAR(100),
    team_code VARCHAR(10),
    jersey_number INTEGER,
    total_saves BIGINT,
    total_goals_conceded BIGINT,
    total_penalties_saved BIGINT,
    matches_played BIGINT,
    clean_sheets BIGINT,
    golden_glove_points BIGINT,
    reached_final BOOLEAN,
    final_match_saves BIGINT,
    final_match_conceded BIGINT,
    final_match_points BIGINT,
    rank BIGINT
) AS $$
BEGIN
    RETURN QUERY
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
        WHERE m.tournament_id = p_tournament_id
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
        WHERE m.tournament_id = p_tournament_id
            AND m.status = 'Completed'
            AND p.position = 'GK'
            AND p.is_active = TRUE
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
                WHERE m.tournament_id = p_tournament_id 
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
        WHERE m.tournament_id = p_tournament_id AND m.round = 'Final'
    )
    SELECT 
        gt.player_id,
        gt.player_name,
        gt.team_id,
        gt.team_name,
        gt.team_code,
        gt.jersey_number,
        gt.total_saves,
        gt.total_goals_conceded,
        gt.total_penalties_saved,
        gt.matches_played,
        gt.clean_sheets,
        gt.golden_glove_points,
        gt.reached_final,
        COALESCE(fs.final_match_saves, 0::BIGINT) AS final_match_saves,
        COALESCE(fs.final_match_conceded, 0::BIGINT) AS final_match_conceded,
        COALESCE(fs.final_match_points, 0::BIGINT) AS final_match_points,
        ROW_NUMBER() OVER (
            ORDER BY 
                gt.golden_glove_points DESC,
                COALESCE(fs.final_match_points, 0) DESC,
                gt.clean_sheets DESC,
                gt.total_saves DESC
        ) AS rank
    FROM goalkeeper_totals gt
    LEFT JOIN final_stats fs ON gt.player_id = fs.player_id
    WHERE gt.total_saves > 0 OR gt.matches_played > 0
    ORDER BY rank
    LIMIT p_limit;
END;
$$ LANGUAGE plpgsql;

-- Grant execute permission
GRANT EXECUTE ON FUNCTION fn_get_golden_glove_stats(INTEGER, INTEGER) TO PUBLIC;

-- Test the function
-- SELECT * FROM fn_get_golden_glove_stats(1, 10);
