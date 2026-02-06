-- Add Man of the Match column to matches table
-- Run this once to add the mom_player_id column

-- Add the column if it doesn't exist
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name = 'matches' AND column_name = 'mom_player_id'
    ) THEN
        ALTER TABLE matches ADD COLUMN mom_player_id INTEGER REFERENCES players(player_id);
        RAISE NOTICE 'Column mom_player_id added to matches table';
    ELSE
        RAISE NOTICE 'Column mom_player_id already exists';
    END IF;
END $$;

-- Verify the column exists
SELECT column_name, data_type, is_nullable
FROM information_schema.columns
WHERE table_name = 'matches' AND column_name = 'mom_player_id';
