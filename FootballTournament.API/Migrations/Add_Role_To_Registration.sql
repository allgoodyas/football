-- Migration: Add Role to User Registration
-- Date: 2026-01-31
-- Description: Update fn_register_user to accept role parameter

-- Drop existing function
DROP FUNCTION IF EXISTS fn_register_user(TEXT, TEXT, TEXT, TEXT, TEXT, TEXT);

-- Create updated function with role parameter
CREATE OR REPLACE FUNCTION fn_register_user(
    p_username TEXT,
    p_email TEXT,
    p_password_hash TEXT,
    p_password_salt TEXT,
    p_full_name TEXT,
    p_phone_number TEXT DEFAULT NULL,
    p_role TEXT DEFAULT 'Spectator'
)
RETURNS INTEGER
LANGUAGE plpgsql
AS $$
DECLARE
    v_user_id INTEGER;
    v_role user_role;
BEGIN
    -- Validate and cast role
    BEGIN
        v_role := p_role::user_role;
    EXCEPTION WHEN OTHERS THEN
        v_role := 'Spectator'::user_role;
    END;

    -- Insert new user
    INSERT INTO users (
        username,
        email,
        password_hash,
        password_salt,
        full_name,
        phone_number,
        role,
        is_active,
        created_on
    )
    VALUES (
        p_username,
        p_email,
        p_password_hash,
        p_password_salt,
        p_full_name,
        p_phone_number,
        v_role,
        TRUE,
        NOW()
    )
    RETURNING user_id INTO v_user_id;

    RETURN v_user_id;
END;
$$;

-- Grant execute permission
GRANT EXECUTE ON FUNCTION fn_register_user(TEXT, TEXT, TEXT, TEXT, TEXT, TEXT, TEXT) TO PUBLIC;

-- Test (optional)
-- SELECT fn_register_user('testuser', 'test@email.com', 'hash', 'salt', 'Test User', '1234567890', 'Official');
