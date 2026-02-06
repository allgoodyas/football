-- =====================================================
-- DEBUG: Golden Boot Data Flow Diagnostic
-- Run each section step by step to find where data breaks
-- =====================================================

-- =====================================================
-- STEP 1: Check match_events table (WHERE GOALS ARE STORED)
-- =====================================================
SELECT '=== STEP 1: Raw Goals in match_events ===' AS step;

SELECT 
    me.event_id,
    me.match_id,
    me.player_id,
    me.team_id,
    me.event_type,
    me.event_minute,
    me.created_on
FROM match_events me
WHERE UPPER(me.event_type) IN ('GOAL', 'PENALTY_GOAL', 'PENALTYGOAL')
ORDER BY me.created_on DESC
LIMIT 20;

-- Count total goals
SELECT 'Total GOAL events in match_events: ' || COUNT(*) AS info
FROM match_events
WHERE UPPER(event_type) IN ('GOAL', 'PENALTY_GOAL', 'PENALTYGOAL');

-- =====================================================
-- STEP 2: Check if matches are linked to tournaments
-- =====================================================
SELECT '=== STEP 2: Matches with Goals linked to Tournaments ===' AS step;

SELECT 
    m.match_id,
    m.tournament_id,
    t.tournament_name,
    COUNT(me.event_id) AS goal_count
FROM matches m
LEFT JOIN tournaments t ON m.tournament_id = t.tournament_id
LEFT JOIN match_events me ON m.match_id = me.match_id 
    AND UPPER(me.event_type) IN ('GOAL', 'PENALTY_GOAL', 'PENALTYGOAL')
GROUP BY m.match_id, m.tournament_id, t.tournament_name
HAVING COUNT(me.event_id) > 0
ORDER BY m.tournament_id;

-- =====================================================
-- STEP 3: Check player → team → tournament chain
-- =====================================================
SELECT '=== STEP 3: Players with Goals - Full Chain ===' AS step;

SELECT 
    me.player_id,
    p.first_name || ' ' || COALESCE(p.last_name, '') AS player_name,
    p.team_id AS player_team_id,
    t.team_id AS team_id,
    t.team_name,
    t.tournament_id AS team_tournament_id,
    m.tournament_id AS match_tournament_id,
    COUNT(*) AS goals
FROM match_events me
JOIN matches m ON me.match_id = m.match_id
LEFT JOIN players p ON me.player_id = p.player_id
LEFT JOIN teams t ON p.team_id = t.team_id
WHERE UPPER(me.event_type) IN ('GOAL', 'PENALTY_GOAL', 'PENALTYGOAL')
AND me.player_id IS NOT NULL
GROUP BY me.player_id, p.first_name, p.last_name, p.team_id, t.team_id, t.team_name, t.tournament_id, m.tournament_id
ORDER BY goals DESC;

-- =====================================================
-- STEP 4: Check which tournaments have data
-- =====================================================
SELECT '=== STEP 4: Tournaments with Goals ===' AS step;

SELECT 
    trn.tournament_id,
    trn.tournament_name,
    COUNT(DISTINCT me.player_id) AS players_with_goals,
    COUNT(me.event_id) AS total_goals
FROM tournaments trn
LEFT JOIN matches m ON trn.tournament_id = m.tournament_id
LEFT JOIN match_events me ON m.match_id = me.match_id 
    AND UPPER(me.event_type) IN ('GOAL', 'PENALTY_GOAL', 'PENALTYGOAL')
GROUP BY trn.tournament_id, trn.tournament_name
ORDER BY trn.tournament_id;

-- =====================================================
-- STEP 5: Test the EXACT Golden Boot Query
-- Replace @TournamentId with your actual tournament ID (e.g., 1)
-- =====================================================
SELECT '=== STEP 5: Golden Boot Query Test (Tournament ID = 1) ===' AS step;

WITH player_goals AS (
    SELECT 
        me.player_id,
        COUNT(*) AS total_goals
    FROM match_events me
    JOIN matches m ON me.match_id = m.match_id
    WHERE m.tournament_id = 1  -- CHANGE THIS TO YOUR TOURNAMENT ID
    AND me.event_type IN ('GOAL', 'Goal', 'PENALTY_GOAL', 'PenaltyGoal')
    AND me.player_id IS NOT NULL
    GROUP BY me.player_id
)
SELECT 
    p.player_id AS "PlayerId",
    COALESCE(p.first_name || ' ' || p.last_name, p.first_name) AS "PlayerName",
    t.team_id AS "TeamId",
    t.team_name AS "TeamName",
    t.team_code AS "TeamCode",
    t.tournament_id AS "TeamTournamentId",
    COALESCE(pg.total_goals, 0) AS "Goals"
FROM players p
JOIN teams t ON p.team_id = t.team_id
LEFT JOIN player_goals pg ON p.player_id = pg.player_id
WHERE t.tournament_id = 1  -- CHANGE THIS TO YOUR TOURNAMENT ID
AND p.is_active = true
AND COALESCE(pg.total_goals, 0) > 0
ORDER BY "Goals" DESC;

-- =====================================================
-- STEP 6: Check event_type values (case sensitivity issue?)
-- =====================================================
SELECT '=== STEP 6: All unique event_type values ===' AS step;

SELECT DISTINCT event_type, COUNT(*) AS count
FROM match_events
GROUP BY event_type
ORDER BY count DESC;

-- =====================================================
-- STEP 7: Check players table
-- =====================================================
SELECT '=== STEP 7: Players table structure check ===' AS step;

SELECT 
    p.player_id,
    p.first_name,
    p.last_name,
    p.team_id,
    p.is_active,
    t.team_name,
    t.tournament_id
FROM players p
LEFT JOIN teams t ON p.team_id = t.team_id
LIMIT 10;

-- =====================================================
-- SUMMARY
-- =====================================================
SELECT '=== SUMMARY ===' AS step;

SELECT 
    (SELECT COUNT(*) FROM match_events WHERE UPPER(event_type) IN ('GOAL', 'PENALTY_GOAL')) AS total_goal_events,
    (SELECT COUNT(*) FROM match_events WHERE player_id IS NOT NULL AND UPPER(event_type) IN ('GOAL', 'PENALTY_GOAL')) AS goals_with_player,
    (SELECT COUNT(DISTINCT player_id) FROM match_events WHERE UPPER(event_type) IN ('GOAL', 'PENALTY_GOAL')) AS unique_scorers,
    (SELECT COUNT(*) FROM players WHERE is_active = true) AS active_players,
    (SELECT COUNT(*) FROM teams) AS total_teams,
    (SELECT COUNT(*) FROM tournaments) AS total_tournaments;
