-- Migration: Create Man of the Match List Function
-- Date: 2025-01-31
-- Description: Returns list of Man of the Match awards for a tournament with match details

-- Drop existing function if exists
DROP FUNCTION IF EXISTS fn_get_man_of_the_match_list(INTEGER);

-- Create the function
CREATE OR REPLACE FUNCTION fn_get_man_of_the_match_list(p_tournament_id INTEGER)
RETURNS TABLE (
    match_id INTEGER,
    player_id INTEGER,
    player_name TEXT,
    jersey_number INTEGER,
    team_id INTEGER,
    team_name VARCHAR(100),
    team_code VARCHAR(10),
    match_date DATE,
    opponent VARCHAR(100),
    goals BIGINT,
    assists BIGINT
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        m.match_id,
        m.mom_player_id AS player_id,
        TRIM(p.first_name || ' ' || COALESCE(p.last_name, '')) AS player_name,
        p.jersey_number,
        p.team_id,
        t.team_name,
        t.team_code,
        m.match_date,
        -- Get opponent team name
        CASE 
            WHEN p.team_id = m.home_team_id THEN at.team_name
            ELSE ht.team_name
        END AS opponent,
        -- Count goals scored by MOM in this match
        COALESCE((
            SELECT COUNT(*) 
            FROM match_events me 
            WHERE me.match_id = m.match_id 
                AND me.player_id = m.mom_player_id 
                AND me.event_type = 'Goal'
        ), 0) AS goals,
        -- Count assists by MOM in this match
        COALESCE((
            SELECT COUNT(*) 
            FROM match_events me 
            WHERE me.match_id = m.match_id 
                AND me.player_id = m.mom_player_id 
                AND me.event_type = 'Assist'
        ), 0) AS assists
    FROM matches m
    JOIN players p ON m.mom_player_id = p.player_id
    JOIN teams t ON p.team_id = t.team_id
    LEFT JOIN teams ht ON m.home_team_id = ht.team_id
    LEFT JOIN teams at ON m.away_team_id = at.team_id
    WHERE m.tournament_id = p_tournament_id
        AND m.mom_player_id IS NOT NULL
        AND m.status = 'Completed'
    ORDER BY m.match_date DESC;
END;
$$ LANGUAGE plpgsql;

-- Grant execute permission
GRANT EXECUTE ON FUNCTION fn_get_man_of_the_match_list(INTEGER) TO PUBLIC;

-- Test the function
-- SELECT * FROM fn_get_man_of_the_match_list(1);
