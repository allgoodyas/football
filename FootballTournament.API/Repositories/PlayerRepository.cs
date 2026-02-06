using Dapper;
using FootballTournament.API.Data;
using FootballTournament.API.Models.Entities;

namespace FootballTournament.API.Repositories;

public interface IPlayerRepository
{
    Task<IEnumerable<Player>> GetAllAsync();
    Task<IEnumerable<Player>> GetAllByTournamentAsync(int tournamentId);
    Task<IEnumerable<Player>> GetAllByTeamAsync(int teamId);
    Task<IEnumerable<Player>> GetUnassignedAsync(int tournamentId);
    Task<Player?> GetByIdAsync(int playerId);
    Task<int> CreateAsync(int? tournamentId, int? teamId, string playerName, DateTime? dateOfBirth,
        int? jerseyNumber, string? position, string? className, string? contactEmail, string? contactPhone,
        string? guardianName, string? guardianContact, int? institutionId, bool isCaptain, 
        int? userId, string? username, string? ipAddress);
    Task UpdateAsync(int playerId, int? teamId, string playerName, DateTime? dateOfBirth,
        int? jerseyNumber, string? position, string? className, string? contactEmail, string? contactPhone,
        string? guardianName, string? guardianContact, int? institutionId, bool isCaptain, bool isActive,
        int? userId, string? username, string? ipAddress);
    Task DeleteAsync(int playerId, int? userId, string? username, string? ipAddress);
    Task TransferPlayerAsync(int playerId, int? toTeamId, string? reason, int? userId, string? username, string? ipAddress);
    Task BulkAssignAsync(int teamId, List<int> playerIds, int? userId, string? username, string? ipAddress);
    Task SetCaptainAsync(int teamId, int playerId, int? userId, string? username, string? ipAddress);
    Task<IEnumerable<PlayerTransfer>> GetTransferHistoryAsync(int playerId);
    Task<IEnumerable<Player>> SearchAsync(string searchTerm, int? tournamentId);
    Task<bool> IsJerseyNumberAvailableAsync(int teamId, int jerseyNumber, int? excludePlayerId = null);
    Task<IEnumerable<PlayerStats>> GetTopScorersAsync(int tournamentId, int limit = 10);
    Task<IEnumerable<PlayerStats>> GetTopAssistsAsync(int tournamentId, int limit = 10);
    Task<IEnumerable<PlayerStats>> GetPlayerStatsAsync(int tournamentId);
}

public class PlayerRepository : IPlayerRepository
{
    private readonly IDbContext _context;

    public PlayerRepository(IDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Player>> GetAllAsync()
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT 
                p.player_id AS PlayerId,
                p.team_id AS TeamId,
                p.tournament_id AS TournamentId,
                p.institution_id AS InstitutionId,
                p.first_name AS FirstName,
                p.last_name AS LastName,
                COALESCE(p.first_name || ' ' || p.last_name, p.first_name, p.last_name) AS PlayerName,
                p.date_of_birth AS DateOfBirth,
                p.jersey_number AS JerseyNumber,
                p.position::text AS Position,
                p.class_name AS ClassName,
                p.is_captain AS IsCaptain,
                p.is_active AS IsActive,
                t.team_name AS TeamName,
                t.team_code AS TeamCode,
                0 AS Goals,
                0 AS Assists,
                0 AS YellowCards,
                0 AS RedCards,
                0 AS MatchesPlayed
            FROM players p
            LEFT JOIN teams t ON p.team_id = t.team_id
            WHERE p.is_active = true
            ORDER BY p.first_name, p.last_name";

        return await connection.QueryAsync<Player>(sql);
    }

    public async Task<IEnumerable<Player>> GetAllByTournamentAsync(int tournamentId)
    {
        using var connection = _context.CreateConnection();

        const string sql = @"
            SELECT 
                p.player_id AS PlayerId,
                p.team_id AS TeamId,
                p.tournament_id AS TournamentId,
                p.institution_id AS InstitutionId,
                p.first_name AS FirstName,
                p.last_name AS LastName,
                COALESCE(p.first_name || ' ' || p.last_name, p.first_name, p.last_name) AS PlayerName,
                p.date_of_birth AS DateOfBirth,
                p.jersey_number AS JerseyNumber,
                p.position::text AS Position,
                p.class_name AS ClassName,
                p.is_captain AS IsCaptain,
                p.is_active AS IsActive,
                t.team_name AS TeamName,
                t.team_code AS TeamCode,
                0 AS Goals,
                0 AS Assists,
                0 AS YellowCards,
                0 AS RedCards,
                0 AS MatchesPlayed
            FROM players p
            LEFT JOIN teams t ON p.team_id = t.team_id
            WHERE p.tournament_id = @TournamentId AND p.is_active = true
            ORDER BY t.team_name, p.jersey_number";

        return await connection.QueryAsync<Player>(sql, new { TournamentId = tournamentId });
    }

    public async Task<IEnumerable<Player>> GetAllByTeamAsync(int teamId)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT 
                p.player_id AS PlayerId,
                p.team_id AS TeamId,
                p.tournament_id AS TournamentId,
                p.first_name AS FirstName,
                p.last_name AS LastName,
                COALESCE(p.first_name || ' ' || p.last_name, p.first_name, p.last_name) AS PlayerName,
                p.date_of_birth AS DateOfBirth,
                p.jersey_number AS JerseyNumber,
                p.position::text AS Position,
                p.class_name AS ClassName,
                p.is_captain AS IsCaptain,
                p.is_active AS IsActive,
                t.team_name AS TeamName,
                t.team_code AS TeamCode,
                0 AS Goals,
                0 AS Assists,
                0 AS YellowCards,
                0 AS RedCards,
                0 AS MatchesPlayed
            FROM players p
            LEFT JOIN teams t ON p.team_id = t.team_id
            WHERE p.team_id = @TeamId AND p.is_active = true
            ORDER BY p.jersey_number, p.first_name";

        return await connection.QueryAsync<Player>(sql, new { TeamId = teamId });
    }

    public async Task<IEnumerable<Player>> GetUnassignedAsync(int tournamentId)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT 
                player_id AS PlayerId,
                tournament_id AS TournamentId,
                first_name AS FirstName,
                last_name AS LastName,
                COALESCE(first_name || ' ' || last_name, first_name, last_name) AS PlayerName,
                date_of_birth AS DateOfBirth,
                position::text AS Position,
                class_name AS ClassName,
                is_active AS IsActive
            FROM players
            WHERE tournament_id = @TournamentId 
              AND team_id IS NULL 
              AND is_active = true
            ORDER BY first_name, last_name";

        return await connection.QueryAsync<Player>(sql, new { TournamentId = tournamentId });
    }

    public async Task<Player?> GetByIdAsync(int playerId)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT 
                p.player_id AS PlayerId,
                p.team_id AS TeamId,
                p.tournament_id AS TournamentId,
                p.institution_id AS InstitutionId,
                p.first_name AS FirstName,
                p.last_name AS LastName,
                COALESCE(p.first_name || ' ' || p.last_name, p.first_name, p.last_name) AS PlayerName,
                p.date_of_birth AS DateOfBirth,
                p.jersey_number AS JerseyNumber,
                p.position::text AS Position,
                p.class_name AS ClassName,
                p.contact_email AS ContactEmail,
                p.contact_phone AS ContactPhone,
                p.guardian_name AS GuardianName,
                p.guardian_contact AS GuardianContact,
                p.photo_url AS PhotoUrl,
                p.is_captain AS IsCaptain,
                p.is_active AS IsActive,
                t.team_name AS TeamName,
                t.team_code AS TeamCode,
                0 AS Goals,
                0 AS Assists,
                0 AS YellowCards,
                0 AS RedCards,
                0 AS MatchesPlayed
            FROM players p
            LEFT JOIN teams t ON p.team_id = t.team_id
            WHERE p.player_id = @PlayerId";

        return await connection.QueryFirstOrDefaultAsync<Player>(sql, new { PlayerId = playerId });
    }

    public async Task<int> CreateAsync(int? tournamentId, int? teamId, string playerName, 
        DateTime? dateOfBirth, int? jerseyNumber, string? position, string? className, string? contactEmail, 
        string? contactPhone, string? guardianName, string? guardianContact, int? institutionId, bool isCaptain, 
        int? userId, string? username, string? ipAddress)
    {
        try
        {

            using var connection = _context.CreateConnection();

            // Split playerName into first and last name
            var nameParts = playerName.Trim().Split(' ', 2);
            var firstName = nameParts[0];
            var lastName = nameParts.Length > 1 ? nameParts[1] : null;

            // Get tournament_id from team if not provided
            if (!tournamentId.HasValue && teamId.HasValue)
            {
                const string getTournamentSql = "SELECT tournament_id FROM teams WHERE team_id = @TeamId";
                tournamentId = await connection.ExecuteScalarAsync<int?>(getTournamentSql, new { TeamId = teamId });
            }

            // Build SQL dynamically based on whether position is provided
            var hasPosition = !string.IsNullOrWhiteSpace(position);
            var sql = $@"
            INSERT INTO players (
                tournament_id, team_id, first_name, last_name, date_of_birth,
                jersey_number, position, class_name, contact_email, contact_phone,
                guardian_name, guardian_contact, institution_id, is_captain,
                is_active, created_on, created_by
            ) VALUES (
                @TournamentId, @TeamId, @FirstName, @LastName, @DateOfBirth,
                @JerseyNumber, {(hasPosition ? "@Position" : "NULL")}, @ClassName, @ContactEmail, @ContactPhone,
                @GuardianName, @GuardianContact, @InstitutionId, @IsCaptain,
                true, NOW(), @UserId
            )
            RETURNING player_id";

            return await connection.ExecuteScalarAsync<int>(sql, new
            {
                TournamentId = tournamentId,
                TeamId = teamId,
                FirstName = firstName,
                LastName = lastName,
                DateOfBirth = dateOfBirth,
                JerseyNumber = jerseyNumber,
                Position = position,
                ClassName = className,
                ContactEmail = contactEmail,
                ContactPhone = contactPhone,
                GuardianName = guardianName,
                GuardianContact = guardianContact,
                InstitutionId = institutionId,
                IsCaptain = isCaptain,
                UserId = userId
            });

        }
        catch (Exception Ex)
        {

            throw;
        }
    }

    public async Task UpdateAsync(int playerId, int? teamId, string playerName, 
        DateTime? dateOfBirth, int? jerseyNumber, string? position, string? className, string? contactEmail, 
        string? contactPhone, string? guardianName, string? guardianContact, int? institutionId, bool isCaptain,
        bool isActive, int? userId, string? username, string? ipAddress)
    {
        using var connection = _context.CreateConnection();
        
        // Split playerName into first and last name
        var nameParts = playerName.Trim().Split(' ', 2);
        var firstName = nameParts[0];
        var lastName = nameParts.Length > 1 ? nameParts[1] : null;

        // Build SQL dynamically based on whether position is provided
        var hasPosition = !string.IsNullOrWhiteSpace(position);
        var sql = $@"
            UPDATE players SET
                team_id = @TeamId,
                first_name = @FirstName,
                last_name = @LastName,
                date_of_birth = @DateOfBirth,
                jersey_number = @JerseyNumber,
                position = {(hasPosition ? "@Position::player_position" : "NULL")},
                class_name = @ClassName,
                contact_email = @ContactEmail,
                contact_phone = @ContactPhone,
                guardian_name = @GuardianName,
                guardian_contact = @GuardianContact,
                institution_id = @InstitutionId,
                is_captain = @IsCaptain,
                is_active = @IsActive,
                edited_on = NOW(),
                edited_by = @UserId
            WHERE player_id = @PlayerId";

        await connection.ExecuteAsync(sql, new
        {
            PlayerId = playerId,
            TeamId = teamId,
            FirstName = firstName,
            LastName = lastName,
            DateOfBirth = dateOfBirth,
            JerseyNumber = jerseyNumber,
            Position = position,
            ClassName = className,
            ContactEmail = contactEmail,
            ContactPhone = contactPhone,
            GuardianName = guardianName,
            GuardianContact = guardianContact,
            InstitutionId = institutionId,
            IsCaptain = isCaptain,
            IsActive = isActive,
            UserId = userId
        });
    }

    public async Task DeleteAsync(int playerId, int? userId, string? username, string? ipAddress)
    {
        using var connection = _context.CreateConnection();
        
        // Soft delete
        const string sql = @"
            UPDATE players SET 
                is_active = false, 
                edited_on = NOW(), 
                edited_by = @UserId 
            WHERE player_id = @PlayerId";

        await connection.ExecuteAsync(sql, new { PlayerId = playerId, UserId = userId });
    }

    public async Task TransferPlayerAsync(int playerId, int? toTeamId, string? reason, 
        int? userId, string? username, string? ipAddress)
    {
        using var connection = _context.CreateConnection();
        
        // Get current team
        const string getCurrentTeamSql = "SELECT team_id FROM players WHERE player_id = @PlayerId";
        var fromTeamId = await connection.ExecuteScalarAsync<int?>(getCurrentTeamSql, new { PlayerId = playerId });

        // Update player's team
        const string updateSql = @"
            UPDATE players SET 
                team_id = @ToTeamId,
                is_captain = false, -- Remove captain status on transfer
                edited_on = NOW(),
                edited_by = @UserId
            WHERE player_id = @PlayerId";

        await connection.ExecuteAsync(updateSql, new { PlayerId = playerId, ToTeamId = toTeamId, UserId = userId });

        // Log transfer (only if player_transfers table exists)
        try
        {
            const string logSql = @"
                INSERT INTO player_transfers (player_id, from_team_id, to_team_id, transfer_date, reason, created_on, created_by)
                VALUES (@PlayerId, @FromTeamId, @ToTeamId, NOW(), @Reason, NOW(), @UserId)";

            await connection.ExecuteAsync(logSql, new
            {
                PlayerId = playerId,
                FromTeamId = fromTeamId,
                ToTeamId = toTeamId,
                Reason = reason,
                UserId = userId
            });
        }
        catch
        {
            // Ignore if player_transfers table doesn't exist
        }
    }

    public async Task BulkAssignAsync(int teamId, List<int> playerIds, 
        int? userId, string? username, string? ipAddress)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            UPDATE players SET 
                team_id = @TeamId,
                edited_on = NOW(),
                edited_by = @UserId
            WHERE player_id = ANY(@PlayerIds)";

        await connection.ExecuteAsync(sql, new
        {
            TeamId = teamId,
            PlayerIds = playerIds.ToArray(),
            UserId = userId
        });
    }

    public async Task SetCaptainAsync(int teamId, int playerId, int? userId, string? username, string? ipAddress)
    {
        using var connection = _context.CreateConnection();
        
        // Remove captain from all players in team
        const string removeSql = @"
            UPDATE players SET 
                is_captain = false,
                edited_on = NOW(),
                edited_by = @UserId
            WHERE team_id = @TeamId AND is_captain = true";

        await connection.ExecuteAsync(removeSql, new { TeamId = teamId, UserId = userId });

        // Set new captain
        const string setSql = @"
            UPDATE players SET 
                is_captain = true,
                edited_on = NOW(),
                edited_by = @UserId
            WHERE player_id = @PlayerId AND team_id = @TeamId";

        await connection.ExecuteAsync(setSql, new { PlayerId = playerId, TeamId = teamId, UserId = userId });
    }

    public async Task<IEnumerable<PlayerTransfer>> GetTransferHistoryAsync(int playerId)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT 
                pt.transfer_id AS TransferId,
                pt.player_id AS PlayerId,
                pt.from_team_id AS FromTeamId,
                pt.to_team_id AS ToTeamId,
                pt.transfer_date AS TransferDate,
                pt.reason AS Reason,
                COALESCE(p.first_name || ' ' || p.last_name, p.first_name) AS PlayerName,
                ft.team_name AS FromTeamName,
                tt.team_name AS ToTeamName,
                pt.created_on AS CreatedOn
            FROM player_transfers pt
            JOIN players p ON pt.player_id = p.player_id
            LEFT JOIN teams ft ON pt.from_team_id = ft.team_id
            LEFT JOIN teams tt ON pt.to_team_id = tt.team_id
            WHERE pt.player_id = @PlayerId
            ORDER BY pt.transfer_date DESC";

        try
        {
            return await connection.QueryAsync<PlayerTransfer>(sql, new { PlayerId = playerId });
        }
        catch
        {
            return Enumerable.Empty<PlayerTransfer>();
        }
    }

    public async Task<IEnumerable<Player>> SearchAsync(string searchTerm, int? tournamentId)
    {
        using var connection = _context.CreateConnection();
        
        var searchPattern = $"%{searchTerm}%";
        
        var sql = @"
            SELECT 
                p.player_id AS PlayerId,
                p.team_id AS TeamId,
                p.tournament_id AS TournamentId,
                p.first_name AS FirstName,
                p.last_name AS LastName,
                COALESCE(p.first_name || ' ' || p.last_name, p.first_name, p.last_name) AS PlayerName,
                p.jersey_number AS JerseyNumber,
                p.position::text AS Position,
                t.team_name AS TeamName,
                t.team_code AS TeamCode
            FROM players p
            LEFT JOIN teams t ON p.team_id = t.team_id
            WHERE p.is_active = true
              AND (LOWER(p.first_name) LIKE LOWER(@SearchPattern)
                   OR LOWER(p.last_name) LIKE LOWER(@SearchPattern)
                   OR LOWER(COALESCE(p.first_name || ' ' || p.last_name, '')) LIKE LOWER(@SearchPattern)
                   OR CAST(p.jersey_number AS TEXT) LIKE @SearchPattern)";

        if (tournamentId.HasValue)
        {
            sql += " AND p.tournament_id = @TournamentId";
        }

        sql += " ORDER BY p.first_name, p.last_name LIMIT 50";

        return await connection.QueryAsync<Player>(sql, new { SearchPattern = searchPattern, TournamentId = tournamentId });
    }

    public async Task<bool> IsJerseyNumberAvailableAsync(int teamId, int jerseyNumber, int? excludePlayerId = null)
    {
        using var connection = _context.CreateConnection();
        
        var sql = @"
            SELECT COUNT(*) = 0 
            FROM players 
            WHERE team_id = @TeamId 
              AND jersey_number = @JerseyNumber 
              AND is_active = true";

        if (excludePlayerId.HasValue)
        {
            sql += " AND player_id != @ExcludePlayerId";
        }

        return await connection.ExecuteScalarAsync<bool>(sql, new 
        { 
            TeamId = teamId, 
            JerseyNumber = jerseyNumber,
            ExcludePlayerId = excludePlayerId 
        });
    }

    public async Task<IEnumerable<PlayerStats>> GetTopScorersAsync(int tournamentId, int limit = 10)
    {
        using var connection = _context.CreateConnection();
        
        // Get goals from match_events table
        const string sql = @"
            SELECT 
                p.player_id AS PlayerId,
                p.team_id AS TeamId,
                COALESCE(p.first_name || ' ' || p.last_name, p.first_name, p.last_name) AS PlayerName,
                p.jersey_number AS JerseyNumber,
                p.position::text AS Position,
                t.team_name AS TeamName,
                t.team_code AS TeamCode,
                COUNT(CASE WHEN me.event_type = 'GOAL' THEN 1 END) AS Goals,
                COUNT(CASE WHEN me.event_type = 'ASSIST' OR me.assist_player_id = p.player_id THEN 1 END) AS Assists,
                COUNT(CASE WHEN me.event_type = 'YELLOW_CARD' THEN 1 END) AS YellowCards,
                COUNT(CASE WHEN me.event_type = 'RED_CARD' THEN 1 END) AS RedCards
            FROM players p
            INNER JOIN teams t ON p.team_id = t.team_id
            LEFT JOIN match_events me ON me.player_id = p.player_id
            LEFT JOIN matches m ON me.match_id = m.match_id AND m.tournament_id = @TournamentId
            WHERE p.tournament_id = @TournamentId 
              AND p.is_active = true
            GROUP BY p.player_id, p.team_id, p.first_name, p.last_name, p.jersey_number, p.position, t.team_name, t.team_code
            HAVING COUNT(CASE WHEN me.event_type = 'GOAL' THEN 1 END) > 0
            ORDER BY Goals DESC, Assists DESC
            LIMIT @Limit";

        try
        {
            return await connection.QueryAsync<PlayerStats>(sql, new { TournamentId = tournamentId, Limit = limit });
        }
        catch
        {
            // If match_events table doesn't exist or has different structure, return empty
            return Enumerable.Empty<PlayerStats>();
        }
    }

    public async Task<IEnumerable<PlayerStats>> GetTopAssistsAsync(int tournamentId, int limit = 10)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT 
                p.player_id AS PlayerId,
                p.team_id AS TeamId,
                COALESCE(p.first_name || ' ' || p.last_name, p.first_name, p.last_name) AS PlayerName,
                p.jersey_number AS JerseyNumber,
                p.position::text AS Position,
                t.team_name AS TeamName,
                t.team_code AS TeamCode,
                COUNT(CASE WHEN me.event_type = 'GOAL' THEN 1 END) AS Goals,
                COUNT(CASE WHEN me.event_type = 'ASSIST' OR me.assist_player_id = p.player_id THEN 1 END) AS Assists,
                COUNT(CASE WHEN me.event_type = 'YELLOW_CARD' THEN 1 END) AS YellowCards,
                COUNT(CASE WHEN me.event_type = 'RED_CARD' THEN 1 END) AS RedCards
            FROM players p
            INNER JOIN teams t ON p.team_id = t.team_id
            LEFT JOIN match_events me ON me.player_id = p.player_id OR me.assist_player_id = p.player_id
            LEFT JOIN matches m ON me.match_id = m.match_id AND m.tournament_id = @TournamentId
            WHERE p.tournament_id = @TournamentId 
              AND p.is_active = true
            GROUP BY p.player_id, p.team_id, p.first_name, p.last_name, p.jersey_number, p.position, t.team_name, t.team_code
            HAVING COUNT(CASE WHEN me.event_type = 'ASSIST' OR me.assist_player_id = p.player_id THEN 1 END) > 0
            ORDER BY Assists DESC, Goals DESC
            LIMIT @Limit";

        try
        {
            return await connection.QueryAsync<PlayerStats>(sql, new { TournamentId = tournamentId, Limit = limit });
        }
        catch
        {
            return Enumerable.Empty<PlayerStats>();
        }
    }

    public async Task<IEnumerable<PlayerStats>> GetPlayerStatsAsync(int tournamentId)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT 
                p.player_id AS PlayerId,
                p.team_id AS TeamId,
                COALESCE(p.first_name || ' ' || p.last_name, p.first_name, p.last_name) AS PlayerName,
                p.jersey_number AS JerseyNumber,
                p.position::text AS Position,
                t.team_name AS TeamName,
                t.team_code AS TeamCode,
                COUNT(CASE WHEN me.event_type = 'GOAL' THEN 1 END) AS Goals,
                COUNT(CASE WHEN me.event_type = 'ASSIST' OR me.assist_player_id = p.player_id THEN 1 END) AS Assists,
                COUNT(CASE WHEN me.event_type = 'YELLOW_CARD' THEN 1 END) AS YellowCards,
                COUNT(CASE WHEN me.event_type = 'RED_CARD' THEN 1 END) AS RedCards
            FROM players p
            INNER JOIN teams t ON p.team_id = t.team_id
            LEFT JOIN match_events me ON me.player_id = p.player_id OR me.assist_player_id = p.player_id
            LEFT JOIN matches m ON me.match_id = m.match_id AND m.tournament_id = @TournamentId
            WHERE p.tournament_id = @TournamentId 
              AND p.is_active = true
            GROUP BY p.player_id, p.team_id, p.first_name, p.last_name, p.jersey_number, p.position, t.team_name, t.team_code
            ORDER BY Goals DESC, Assists DESC, p.first_name";

        try
        {
            return await connection.QueryAsync<PlayerStats>(sql, new { TournamentId = tournamentId });
        }
        catch
        {
            return Enumerable.Empty<PlayerStats>();
        }
    }
}
