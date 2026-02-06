-- =====================================================
-- DIAGNOSTIC SCRIPT FOR GOLDEN BOOT FIX
-- Run this in PostgreSQL to verify goals are recorded
-- =====================================================

-- 1. CHECK WHAT'S IN MATCH_EVENTS TABLE (GOALS)
SELECT 
    'Goals in match_events' AS check_type,
    COUNT(*) AS count
FROM match_events me
WHERE UPPER(me.event_type) IN ('GOAL', 'PENALTY_GOAL', 'PENALTYGOAL');

-- 2. DETAILED GOAL LIST
SELECT 
    me.event_id,
    me.match_id,
    me.player_id,
    me.team_id,
    me.event_type,
    me.event_minute,
    COALESCE(p.first_name || ' ' || COALESCE(p.last_name, ''), 'Unknown') AS player_name,
    t.team_name,
    m.tournament_id
FROM match_events me
LEFT JOIN players p ON me.player_id = p.player_id
LEFT JOIN teams t ON me.team_id = t.team_id
LEFT JOIN matches m ON me.match_id = m.match_id
WHERE UPPER(me.event_type) IN ('GOAL', 'PENALTY_GOAL', 'PENALTYGOAL', 'ASSIST', 'YELLOWCARD', 'REDCARD')
ORDER BY me.event_id DESC
LIMIT 30;

-- 3. VERIFY PLAYER->TEAM->TOURNAMENT RELATIONSHIP
SELECT 
    'Players with tournament path' AS check_type,
    p.player_id,
    p.first_name || ' ' || COALESCE(p.last_name, '') AS player_name,
    t.team_id,
    t.team_name,
    t.tournament_id
FROM players p
JOIN teams t ON p.team_id = t.team_id
WHERE p.is_active = true
LIMIT 10;

-- 4. GOLDEN BOOT STANDINGS (FIXED QUERY)
-- This is the fixed query that uses t.tournament_id instead of p.tournament_id
WITH player_goals AS (
    SELECT 
        me.player_id,
        COUNT(*) AS total_goals
    FROM match_events me
    JOIN matches m ON me.match_id = m.match_id
    WHERE m.tournament_id = 1  -- Change to your tournament ID
    AND UPPER(me.event_type) IN ('GOAL', 'PENALTY_GOAL', 'PENALTYGOAL')
    AND me.player_id IS NOT NULL
    GROUP BY me.player_id
),
player_assists AS (
    SELECT 
        COALESCE(me.assist_player_id, me.player_id) AS player_id,
        COUNT(*) AS assists
    FROM match_events me
    JOIN matches m ON me.match_id = m.match_id
    WHERE m.tournament_id = 1  -- Change to your tournament ID
    AND (UPPER(me.event_type) = 'ASSIST' OR me.assist_player_id IS NOT NULL)
    GROUP BY COALESCE(me.assist_player_id, me.player_id)
)
SELECT 
    p.player_id,
    p.first_name || ' ' || COALESCE(p.last_name, '') AS player_name,
    t.team_name,
    t.team_code,
    COALESCE(pg.total_goals, 0) AS goals,
    COALESCE(pa.assists, 0) AS assists,
    ROW_NUMBER() OVER (ORDER BY COALESCE(pg.total_goals, 0) DESC, COALESCE(pa.assists, 0) DESC) AS rank
FROM players p
JOIN teams t ON p.team_id = t.team_id
LEFT JOIN player_goals pg ON p.player_id = pg.player_id
LEFT JOIN player_assists pa ON p.player_id = pa.player_id
WHERE t.tournament_id = 1  -- Change to your tournament ID (FIXED: was p.tournament_id)
AND p.is_active = true
AND (COALESCE(pg.total_goals, 0) > 0 OR COALESCE(pa.assists, 0) > 0)
ORDER BY goals DESC, assists DESC
LIMIT 10;

-- =====================================================
-- SUMMARY OF THE FIX
-- =====================================================
/*
The bug was in CFAAwardsRepository.cs:

BEFORE (WRONG):
  WHERE p.tournament_id = @TournamentId

AFTER (FIXED):
  WHERE t.tournament_id = @TournamentId

The players table doesn't have tournament_id directly.
Players link to teams (p.team_id), and teams have tournament_id (t.tournament_id).

The fix was applied to:
1. GetGoldenBootStandingsAsync() 
2. GetAwardSummaryAsync()

Now rebuild the API and test!
*/

SELECT 'Diagnostic complete! Check the results above.' AS status;
