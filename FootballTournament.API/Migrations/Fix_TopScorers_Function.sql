-- =====================================================
-- FIXED: fn_get_top_scorers Function
-- "position" is a reserved keyword in PostgreSQL
-- Changed to "player_position" to avoid the error
-- =====================================================

CREATE OR REPLACE FUNCTION fn_get_top_scorers(
    p_tournament_id INTEGER,
    p_limit INTEGER DEFAULT 10
)
RETURNS TABLE (
    player_id INTEGER,
    player_name VARCHAR,
    team_id INTEGER,
    team_name VARCHAR,
    team_code VARCHAR,
    player_position VARCHAR,  -- FIXED: renamed from "position"
    jersey_number INTEGER,
    goals INTEGER,
    assists INTEGER,
    matches_played INTEGER,
    rank BIGINT
) AS $$
BEGIN
    RETURN QUERY
    WITH player_goals AS (
        SELECT 
            me.player_id,
            COUNT(*) AS goal_count
        FROM match_events me
        JOIN matches m ON me.match_id = m.match_id
        WHERE m.tournament_id = p_tournament_id
        AND UPPER(me.event_type) IN ('GOAL', 'PENALTY_GOAL', 'PENALTYGOAL')
        AND me.player_id IS NOT NULL
        GROUP BY me.player_id
    ),
    player_assists AS (
        SELECT 
            COALESCE(me.assist_player_id, me.player_id) AS player_id,
            COUNT(*) AS assist_count
        FROM match_events me
        JOIN matches m ON me.match_id = m.match_id
        WHERE m.tournament_id = p_tournament_id
        AND (UPPER(me.event_type) = 'ASSIST' OR me.assist_player_id IS NOT NULL)
        AND (me.assist_player_id IS NOT NULL OR UPPER(me.event_type) = 'ASSIST')
        GROUP BY COALESCE(me.assist_player_id, me.player_id)
    ),
    player_matches AS (
        SELECT 
            me.player_id,
            COUNT(DISTINCT me.match_id) AS match_count
        FROM match_events me
        JOIN matches m ON me.match_id = m.match_id
        WHERE m.tournament_id = p_tournament_id
        AND me.player_id IS NOT NULL
        GROUP BY me.player_id
    )
    SELECT 
        p.player_id,
        COALESCE(p.display_name, p.first_name || ' ' || COALESCE(p.last_name, ''))::VARCHAR AS player_name,
        t.team_id,
        t.team_name::VARCHAR,
        t.team_code::VARCHAR,
        COALESCE(p.position::VARCHAR, 'N/A') AS player_position,  -- FIXED: alias matches return column
        p.jersey_number,
        COALESCE(pg.goal_count, 0)::INTEGER AS goals,
        COALESCE(pa.assist_count, 0)::INTEGER AS assists,
        COALESCE(pm.match_count, 0)::INTEGER AS matches_played,
        ROW_NUMBER() OVER (ORDER BY COALESCE(pg.goal_count, 0) DESC, COALESCE(pa.assist_count, 0) DESC) AS rank
    FROM players p
    JOIN teams t ON p.team_id = t.team_id
    LEFT JOIN player_goals pg ON p.player_id = pg.player_id
    LEFT JOIN player_assists pa ON p.player_id = pa.player_id
    LEFT JOIN player_matches pm ON p.player_id = pm.player_id
    WHERE t.tournament_id = p_tournament_id
      AND p.is_active = TRUE
      AND COALESCE(pg.goal_count, 0) > 0
    ORDER BY goals DESC, assists DESC
    LIMIT p_limit;
END;
$$ LANGUAGE plpgsql;

-- =====================================================
-- FIXED: fn_get_top_assisters Function  
-- =====================================================

CREATE OR REPLACE FUNCTION fn_get_top_assisters(
    p_tournament_id INTEGER,
    p_limit INTEGER DEFAULT 10
)
RETURNS TABLE (
    player_id INTEGER,
    player_name VARCHAR,
    team_id INTEGER,
    team_name VARCHAR,
    team_code VARCHAR,
    player_position VARCHAR,  -- FIXED: renamed from "position"
    jersey_number INTEGER,
    assists INTEGER,
    goals INTEGER,
    matches_played INTEGER,
    rank BIGINT
) AS $$
BEGIN
    RETURN QUERY
    WITH player_goals AS (
        SELECT 
            me.player_id,
            COUNT(*) AS goal_count
        FROM match_events me
        JOIN matches m ON me.match_id = m.match_id
        WHERE m.tournament_id = p_tournament_id
        AND UPPER(me.event_type) IN ('GOAL', 'PENALTY_GOAL', 'PENALTYGOAL')
        AND me.player_id IS NOT NULL
        GROUP BY me.player_id
    ),
    player_assists AS (
        SELECT 
            COALESCE(me.assist_player_id, me.player_id) AS player_id,
            COUNT(*) AS assist_count
        FROM match_events me
        JOIN matches m ON me.match_id = m.match_id
        WHERE m.tournament_id = p_tournament_id
        AND (UPPER(me.event_type) = 'ASSIST' OR me.assist_player_id IS NOT NULL)
        AND (me.assist_player_id IS NOT NULL OR UPPER(me.event_type) = 'ASSIST')
        GROUP BY COALESCE(me.assist_player_id, me.player_id)
    ),
    player_matches AS (
        SELECT 
            me.player_id,
            COUNT(DISTINCT me.match_id) AS match_count
        FROM match_events me
        JOIN matches m ON me.match_id = m.match_id
        WHERE m.tournament_id = p_tournament_id
        AND me.player_id IS NOT NULL
        GROUP BY me.player_id
    )
    SELECT 
        p.player_id,
        COALESCE(p.display_name, p.first_name || ' ' || COALESCE(p.last_name, ''))::VARCHAR AS player_name,
        t.team_id,
        t.team_name::VARCHAR,
        t.team_code::VARCHAR,
        COALESCE(p.position::VARCHAR, 'N/A') AS player_position,  -- FIXED
        p.jersey_number,
        COALESCE(pa.assist_count, 0)::INTEGER AS assists,
        COALESCE(pg.goal_count, 0)::INTEGER AS goals,
        COALESCE(pm.match_count, 0)::INTEGER AS matches_played,
        ROW_NUMBER() OVER (ORDER BY COALESCE(pa.assist_count, 0) DESC, COALESCE(pg.goal_count, 0) DESC) AS rank
    FROM players p
    JOIN teams t ON p.team_id = t.team_id
    LEFT JOIN player_goals pg ON p.player_id = pg.player_id
    LEFT JOIN player_assists pa ON p.player_id = pa.player_id
    LEFT JOIN player_matches pm ON p.player_id = pm.player_id
    WHERE t.tournament_id = p_tournament_id
      AND p.is_active = TRUE
      AND COALESCE(pa.assist_count, 0) > 0
    ORDER BY assists DESC, goals DESC
    LIMIT p_limit;
END;
$$ LANGUAGE plpgsql;

-- =====================================================
-- TEST THE FUNCTIONS
-- =====================================================
-- SELECT * FROM fn_get_top_scorers(1, 10);
-- SELECT * FROM fn_get_top_assisters(1, 10);

SELECT 'Functions created successfully!' AS status;
