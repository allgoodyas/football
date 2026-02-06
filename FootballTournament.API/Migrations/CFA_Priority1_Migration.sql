-- =====================================================
-- CFA Tournament 2026 - Priority 1 Features
-- Database Migration Script
-- =====================================================
-- Run this script against your PostgreSQL database
-- =====================================================

-- =====================================================
-- 1. ADD CATEGORY COLUMN TO TOURNAMENTS TABLE
-- =====================================================
DO $$ 
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name = 'tournaments' AND column_name = 'category'
    ) THEN
        ALTER TABLE tournaments 
        ADD COLUMN category VARCHAR(20) DEFAULT 'Senior';
        
        COMMENT ON COLUMN tournaments.category IS 'Tournament category: SuperJunior, Junior, Senior';
    END IF;
END $$;

-- =====================================================
-- 2. ADD GOAL_TYPE COLUMN TO MATCH_EVENTS TABLE
-- =====================================================
DO $$ 
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name = 'match_events' AND column_name = 'goal_type'
    ) THEN
        ALTER TABLE match_events 
        ADD COLUMN goal_type VARCHAR(20) DEFAULT 'Regular';
        
        COMMENT ON COLUMN match_events.goal_type IS 'Goal type: Regular, Penalty, OwnGoal, ShootoutPenalty';
    END IF;
END $$;

-- =====================================================
-- 3. CREATE GOALKEEPER_MATCH_STATS TABLE
-- =====================================================
CREATE TABLE IF NOT EXISTS goalkeeper_match_stats (
    goalkeeper_stats_id SERIAL PRIMARY KEY,
    match_id INTEGER NOT NULL REFERENCES matches(match_id) ON DELETE CASCADE,
    player_id INTEGER NOT NULL REFERENCES players(player_id) ON DELETE CASCADE,
    team_id INTEGER NOT NULL REFERENCES teams(team_id) ON DELETE CASCADE,
    tournament_id INTEGER NOT NULL REFERENCES tournaments(tournament_id) ON DELETE CASCADE,
    
    -- Statistics
    saves INTEGER NOT NULL DEFAULT 0,
    goals_conceded INTEGER NOT NULL DEFAULT 0,
    penalties_saved INTEGER NOT NULL DEFAULT 0,
    penalties_faced INTEGER NOT NULL DEFAULT 0,
    minutes_played INTEGER NOT NULL DEFAULT 0,
    is_clean_sheet BOOLEAN NOT NULL DEFAULT FALSE,
    
    -- Audit columns
    created_on TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    created_by INTEGER,
    edited_on TIMESTAMP WITH TIME ZONE,
    edited_by INTEGER,
    
    -- Unique constraint: one record per goalkeeper per match
    CONSTRAINT uq_goalkeeper_match UNIQUE (match_id, player_id)
);

-- Indexes for performance
CREATE INDEX IF NOT EXISTS idx_goalkeeper_stats_tournament ON goalkeeper_match_stats(tournament_id);
CREATE INDEX IF NOT EXISTS idx_goalkeeper_stats_player ON goalkeeper_match_stats(player_id);
CREATE INDEX IF NOT EXISTS idx_goalkeeper_stats_team ON goalkeeper_match_stats(team_id);

COMMENT ON TABLE goalkeeper_match_stats IS 'Goalkeeper statistics per match for Golden Glove tracking';

-- =====================================================
-- 4. CREATE DEFENDER_MATCH_RATINGS TABLE
-- =====================================================
CREATE TABLE IF NOT EXISTS defender_match_ratings (
    rating_id SERIAL PRIMARY KEY,
    match_id INTEGER NOT NULL REFERENCES matches(match_id) ON DELETE CASCADE,
    player_id INTEGER NOT NULL REFERENCES players(player_id) ON DELETE CASCADE,
    team_id INTEGER NOT NULL REFERENCES teams(team_id) ON DELETE CASCADE,
    tournament_id INTEGER NOT NULL REFERENCES tournaments(tournament_id) ON DELETE CASCADE,
    
    -- Rating (1 = Best, 2 = Second Best per match)
    rating_position INTEGER NOT NULL CHECK (rating_position IN (1, 2)),
    points_awarded INTEGER NOT NULL DEFAULT 0, -- 2 for 1st, 1 for 2nd
    
    -- Evaluation criteria (1-10 scale)
    tackling_score INTEGER CHECK (tackling_score >= 1 AND tackling_score <= 10),
    interception_score INTEGER CHECK (interception_score >= 1 AND interception_score <= 10),
    marking_score INTEGER CHECK (marking_score >= 1 AND marking_score <= 10),
    blocking_score INTEGER CHECK (blocking_score >= 1 AND blocking_score <= 10),
    
    notes TEXT,
    
    -- Audit columns
    created_on TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    created_by INTEGER,
    edited_on TIMESTAMP WITH TIME ZONE,
    edited_by INTEGER,
    
    -- Unique constraint: one rating position per match
    CONSTRAINT uq_defender_rating_position UNIQUE (match_id, rating_position)
);

-- Indexes for performance
CREATE INDEX IF NOT EXISTS idx_defender_ratings_tournament ON defender_match_ratings(tournament_id);
CREATE INDEX IF NOT EXISTS idx_defender_ratings_player ON defender_match_ratings(player_id);
CREATE INDEX IF NOT EXISTS idx_defender_ratings_team ON defender_match_ratings(team_id);

COMMENT ON TABLE defender_match_ratings IS 'Top 2 defender ratings per match for Best Defender award';

-- =====================================================
-- 5. CREATE MATCH_WALKOVERS TABLE
-- =====================================================
CREATE TABLE IF NOT EXISTS match_walkovers (
    match_id INTEGER PRIMARY KEY REFERENCES matches(match_id) ON DELETE CASCADE,
    winner_team_id INTEGER NOT NULL REFERENCES teams(team_id),
    absent_team_id INTEGER REFERENCES teams(team_id),
    reason TEXT,
    delay_minutes INTEGER NOT NULL DEFAULT 5,
    declared_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    declared_by VARCHAR(100),
    winner_score INTEGER NOT NULL DEFAULT 3,
    loser_score INTEGER NOT NULL DEFAULT 0
);

COMMENT ON TABLE match_walkovers IS 'Walkover match details when a team fails to appear';

-- =====================================================
-- 6. UPDATE MATCHES STATUS TO INCLUDE WALKOVER
-- =====================================================
-- Note: If using ENUM type, you may need to add the value
-- If using VARCHAR, this is already supported

-- =====================================================
-- 7. CREATE TRIGGER FOR AUTO-CALCULATING POINTS
-- =====================================================

-- Trigger to auto-set points_awarded based on rating_position
CREATE OR REPLACE FUNCTION fn_set_defender_points()
RETURNS TRIGGER AS $$
BEGIN
    NEW.points_awarded := CASE WHEN NEW.rating_position = 1 THEN 2 ELSE 1 END;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS tr_defender_points ON defender_match_ratings;
CREATE TRIGGER tr_defender_points
    BEFORE INSERT OR UPDATE ON defender_match_ratings
    FOR EACH ROW
    EXECUTE FUNCTION fn_set_defender_points();

-- Trigger to auto-set is_clean_sheet based on goals_conceded
CREATE OR REPLACE FUNCTION fn_set_clean_sheet()
RETURNS TRIGGER AS $$
BEGIN
    NEW.is_clean_sheet := (NEW.goals_conceded = 0);
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS tr_clean_sheet ON goalkeeper_match_stats;
CREATE TRIGGER tr_clean_sheet
    BEFORE INSERT OR UPDATE ON goalkeeper_match_stats
    FOR EACH ROW
    EXECUTE FUNCTION fn_set_clean_sheet();

-- =====================================================
-- 8. VIEWS FOR AWARD STANDINGS
-- =====================================================

-- Golden Boot View
CREATE OR REPLACE VIEW vw_golden_boot_standings AS
WITH player_goals AS (
    SELECT 
        me.player_id,
        COUNT(*) AS total_goals,
        COUNT(CASE WHEN COALESCE(me.goal_type, 'Regular') != 'ShootoutPenalty' THEN 1 END) AS eligible_goals,
        COUNT(CASE WHEN COALESCE(me.goal_type, 'Regular') = 'Regular' THEN 1 END) AS regular_goals,
        COUNT(CASE WHEN me.goal_type = 'Penalty' THEN 1 END) AS penalty_goals,
        COUNT(CASE WHEN me.goal_type = 'ShootoutPenalty' THEN 1 END) AS shootout_goals
    FROM match_events me
    JOIN matches m ON me.match_id = m.match_id
    WHERE me.event_type IN ('GOAL', 'Goal', 'PENALTY_GOAL', 'PenaltyGoal')
    AND me.player_id IS NOT NULL
    GROUP BY me.player_id
),
player_assists AS (
    SELECT 
        me.assist_player_id AS player_id,
        COUNT(*) AS assists
    FROM match_events me
    WHERE me.assist_player_id IS NOT NULL
    AND me.event_type IN ('GOAL', 'Goal')
    GROUP BY me.assist_player_id
)
SELECT 
    p.player_id,
    COALESCE(p.first_name || ' ' || p.last_name, p.first_name) AS player_name,
    p.team_id,
    t.team_name,
    t.team_code,
    p.jersey_number,
    p.position,
    p.tournament_id,
    COALESCE(pg.total_goals, 0) AS total_goals,
    COALESCE(pg.eligible_goals, 0) AS eligible_goals,
    COALESCE(pg.regular_goals, 0) AS regular_goals,
    COALESCE(pg.penalty_goals, 0) AS penalty_goals,
    COALESCE(pg.shootout_goals, 0) AS shootout_goals,
    COALESCE(pa.assists, 0) AS assists,
    ROW_NUMBER() OVER (
        PARTITION BY p.tournament_id
        ORDER BY COALESCE(pg.eligible_goals, 0) DESC, COALESCE(pa.assists, 0) DESC
    ) AS rank
FROM players p
JOIN teams t ON p.team_id = t.team_id
LEFT JOIN player_goals pg ON p.player_id = pg.player_id
LEFT JOIN player_assists pa ON p.player_id = pa.player_id
WHERE p.is_active = true
AND (COALESCE(pg.total_goals, 0) > 0 OR COALESCE(pa.assists, 0) > 0);

-- Golden Glove View
CREATE OR REPLACE VIEW vw_golden_glove_standings AS
SELECT 
    gms.player_id,
    COALESCE(p.first_name || ' ' || p.last_name, p.first_name) AS player_name,
    gms.team_id,
    t.team_name,
    t.team_code,
    p.jersey_number,
    gms.tournament_id,
    SUM(gms.saves) AS total_saves,
    SUM(gms.goals_conceded) AS total_goals_conceded,
    SUM(gms.penalties_saved) AS total_penalties_saved,
    COUNT(*) AS matches_played,
    SUM(CASE WHEN gms.is_clean_sheet THEN 1 ELSE 0 END) AS clean_sheets,
    SUM(gms.saves - gms.goals_conceded) AS golden_glove_points,
    ROW_NUMBER() OVER (
        PARTITION BY gms.tournament_id
        ORDER BY SUM(gms.saves - gms.goals_conceded) DESC, SUM(CASE WHEN gms.is_clean_sheet THEN 1 ELSE 0 END) DESC
    ) AS rank
FROM goalkeeper_match_stats gms
JOIN players p ON gms.player_id = p.player_id
JOIN teams t ON gms.team_id = t.team_id
WHERE p.position = 'GK'
GROUP BY gms.player_id, p.first_name, p.last_name, gms.team_id, t.team_name, t.team_code, p.jersey_number, gms.tournament_id;

-- Best Defender View
CREATE OR REPLACE VIEW vw_best_defender_standings AS
SELECT 
    dmr.player_id,
    COALESCE(p.first_name || ' ' || p.last_name, p.first_name) AS player_name,
    dmr.team_id,
    t.team_name,
    t.team_code,
    p.jersey_number,
    p.position,
    dmr.tournament_id,
    SUM(dmr.points_awarded) AS total_points,
    COUNT(*) AS matches_rated,
    SUM(CASE WHEN dmr.rating_position = 1 THEN 1 ELSE 0 END) AS first_place_count,
    SUM(CASE WHEN dmr.rating_position = 2 THEN 1 ELSE 0 END) AS second_place_count,
    ROUND(AVG((COALESCE(dmr.tackling_score, 0) + COALESCE(dmr.interception_score, 0) + 
               COALESCE(dmr.marking_score, 0) + COALESCE(dmr.blocking_score, 0)) / 4.0), 2) AS average_score,
    ROW_NUMBER() OVER (
        PARTITION BY dmr.tournament_id
        ORDER BY SUM(dmr.points_awarded) DESC, COUNT(*) DESC
    ) AS rank
FROM defender_match_ratings dmr
JOIN players p ON dmr.player_id = p.player_id
JOIN teams t ON dmr.team_id = t.team_id
GROUP BY dmr.player_id, p.first_name, p.last_name, dmr.team_id, t.team_name, t.team_code, p.jersey_number, p.position, dmr.tournament_id;

-- =====================================================
-- 9. GRANT PERMISSIONS (adjust as needed)
-- =====================================================
-- GRANT SELECT, INSERT, UPDATE, DELETE ON goalkeeper_match_stats TO your_app_user;
-- GRANT SELECT, INSERT, UPDATE, DELETE ON defender_match_ratings TO your_app_user;
-- GRANT SELECT, INSERT, UPDATE, DELETE ON match_walkovers TO your_app_user;
-- GRANT USAGE, SELECT ON SEQUENCE goalkeeper_match_stats_goalkeeper_stats_id_seq TO your_app_user;
-- GRANT USAGE, SELECT ON SEQUENCE defender_match_ratings_rating_id_seq TO your_app_user;

-- =====================================================
-- MIGRATION COMPLETE
-- =====================================================
SELECT 'CFA Tournament 2026 Priority 1 Migration Complete!' AS status;
