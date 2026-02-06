-- Migration: Update Golden Glove Function to use match_events
-- Date: 2025-01-31
-- Description: Reads saves from match_events table instead of goalkeeper_match_stats

-- Drop existing function
DROP FUNCTION IF EXISTS fn_get_golden_glove_stats(INTEGER, INTEGER);

-- Create updated function
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
    rank BIGINT
) AS $$
BEGIN
    RETURN QUERY
    WITH goalkeeper_stats AS (
        SELECT 
            p.player_id,
            TRIM(p.first_name || ' ' || COALESCE(p.last_name, '')) AS player_name,
            p.team_id,
            t.team_name,
            t.team_code,
            p.jersey_number,
            -- Count saves from match_events
            (SELECT COUNT(*) FROM match_events me 
             JOIN matches m ON me.match_id = m.match_id 
             WHERE me.player_id = p.player_id 
             AND me.event_type = 'GoalSaved'
             AND m.tournament_id = p_tournament_id) AS total_saves,
            -- Count matches played (matches where team participated and match is completed)
            (SELECT COUNT(DISTINCT m.match_id) FROM matches m 
             WHERE m.tournament_id = p_tournament_id
             AND m.status = 'Completed'
             AND (m.home_team_id = p.team_id OR m.away_team_id = p.team_id)) AS matches_played,
            -- Calculate goals conceded (goals against the goalkeeper's team)
            (SELECT COALESCE(SUM(
                CASE 
                    WHEN m.home_team_id = p.team_id THEN COALESCE(m.away_score, 0)
                    ELSE COALESCE(m.home_score, 0)
                END
            ), 0)
             FROM matches m 
             WHERE m.tournament_id = p_tournament_id
             AND m.status = 'Completed'
             AND (m.home_team_id = p.team_id OR m.away_team_id = p.team_id)) AS total_goals_conceded,
            -- Clean sheets (matches where team conceded 0)
            (SELECT COUNT(*) FROM matches m 
             WHERE m.tournament_id = p_tournament_id
             AND m.status = 'Completed'
             AND (
                 (m.home_team_id = p.team_id AND COALESCE(m.away_score, 0) = 0)
                 OR (m.away_team_id = p.team_id AND COALESCE(m.home_score, 0) = 0)
             )) AS clean_sheets,
            -- Count penalty saves from match_events (if we track them separately)
            (SELECT COUNT(*) FROM match_events me 
             JOIN matches m ON me.match_id = m.match_id 
             WHERE me.player_id = p.player_id 
             AND me.event_type = 'PenaltySaved'
             AND m.tournament_id = p_tournament_id) AS penalty_saves
        FROM players p
        JOIN teams t ON p.team_id = t.team_id
        WHERE p.position = 'GK'
        AND t.tournament_id = p_tournament_id
        AND p.is_active = TRUE
    )
    SELECT 
        gs.player_id,
        gs.player_name,
        gs.team_id,
        gs.team_name,
        gs.team_code,
        gs.jersey_number,
        gs.total_saves,
        gs.total_goals_conceded,
        gs.penalty_saves AS total_penalties_saved,
        gs.matches_played,
        gs.clean_sheets,
        -- Points calculation: saves + clean_sheets*3 + penalty_saves*2 - goals_conceded
        (gs.total_saves + gs.clean_sheets * 3 + gs.penalty_saves * 2 - gs.total_goals_conceded) AS golden_glove_points,
        ROW_NUMBER() OVER (
            ORDER BY 
                (gs.total_saves + gs.clean_sheets * 3 + gs.penalty_saves * 2 - gs.total_goals_conceded) DESC,
                gs.clean_sheets DESC,
                gs.total_saves DESC
        ) AS rank
    FROM goalkeeper_stats gs
    WHERE gs.matches_played > 0 OR gs.total_saves > 0
    ORDER BY rank
    LIMIT p_limit;
END;
$$ LANGUAGE plpgsql;

-- Grant execute permission
GRANT EXECUTE ON FUNCTION fn_get_golden_glove_stats(INTEGER, INTEGER) TO PUBLIC;

-- Test the function
-- SELECT * FROM fn_get_golden_glove_stats(1, 10);
