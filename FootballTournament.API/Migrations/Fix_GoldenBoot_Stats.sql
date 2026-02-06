-- =====================================================
-- Fix Golden Boot and Player Statistics
-- Run this script to fix the top scorers/awards display
-- =====================================================

-- =====================================================
-- 1. CHECK WHAT'S IN MATCH_EVENTS TABLE
-- =====================================================
-- Run this to see if goals are being recorded:
SELECT 
    me.event_id,
    me.match_id,
    me.player_id,
    me.team_id,
    me.event_type,
    me.event_minute,
    p.first_name || ' ' || COALESCE(p.last_name, '') AS player_name,
    t.team_name
FROM match_events me
LEFT JOIN players p ON me.player_id = p.player_id
LEFT JOIN teams t ON me.team_id = t.team_id
WHERE UPPER(me.event_type) IN ('GOAL', 'ASSIST', 'YELLOWCARD', 'REDCARD')
ORDER BY me.event_id DESC
LIMIT 20;

-- =====================================================
-- 2. SYNC PLAYER_STATISTICS FROM MATCH_EVENTS
-- This populates the player_statistics table from match_events
-- =====================================================
INSERT INTO player_statistics (player_id, tournament_id, goals, assists, yellow_cards, red_cards, matches_played, created_on)
SELECT 
    me.player_id,
    m.tournament_id,
    COUNT(CASE WHEN UPPER(me.event_type) IN ('GOAL') THEN 1 END) AS goals,
    COUNT(CASE WHEN UPPER(me.event_type) IN ('ASSIST') THEN 1 END) AS assists,
    COUNT(CASE WHEN UPPER(me.event_type) IN ('YELLOWCARD', 'YELLOW_CARD') THEN 1 END) AS yellow_cards,
    COUNT(CASE WHEN UPPER(me.event_type) IN ('REDCARD', 'RED_CARD') THEN 1 END) AS red_cards,
    COUNT(DISTINCT me.match_id) AS matches_played,
    NOW()
FROM match_events me
JOIN matches m ON me.match_id = m.match_id
WHERE me.player_id IS NOT NULL
GROUP BY me.player_id, m.tournament_id
ON CONFLICT (player_id, tournament_id) DO UPDATE SET
    goals = EXCLUDED.goals,
    assists = EXCLUDED.assists,
    yellow_cards = EXCLUDED.yellow_cards,
    red_cards = EXCLUDED.red_cards,
    matches_played = EXCLUDED.matches_played,
    edited_on = NOW();

-- =====================================================
-- 3. CREATE/REPLACE BETTER TOP SCORERS FUNCTION
-- This function reads directly from match_events
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
    position VARCHAR,
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
        COALESCE(p.position::VARCHAR, 'N/A'),
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
-- 4. CREATE/REPLACE BETTER TOP ASSISTERS FUNCTION
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
    position VARCHAR,
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
        COALESCE(p.position::VARCHAR, 'N/A'),
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
-- 5. VERIFY THE FIX - Check top scorers
-- =====================================================
-- Run this after the above to verify:
-- SELECT * FROM fn_get_top_scorers(1, 10);

-- =====================================================
-- DONE
-- =====================================================
SELECT 'Golden Boot and Statistics fix applied successfully!' AS status;
