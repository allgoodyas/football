using Dapper;
using FootballTournament.API.Data;
using FootballTournament.API.Models.Entities;

namespace FootballTournament.API.Repositories;

public interface IAnnouncementRepository
{
    Task<IEnumerable<Announcement>> GetAllAsync(int? tournamentId = null, int limit = 10);
    Task<Announcement?> GetByIdAsync(int announcementId);
    Task<int> CreateAsync(string title, string content, int? tournamentId, string? announcementType,
        string? targetAudience, bool isPinned, DateTime? expiryDate, int? userId, string? username, string? ipAddress);
    Task UpdateAsync(int announcementId, string title, string content, string? announcementType,
        string? targetAudience, bool isPinned, DateTime? expiryDate, int? userId, string? username, string? ipAddress);
    Task DeleteAsync(int announcementId, int? userId, string? username, string? ipAddress);
}

public class AnnouncementRepository : IAnnouncementRepository
{
    private readonly IDbContext _context;

    public AnnouncementRepository(IDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Announcement>> GetAllAsync(int? tournamentId = null, int limit = 10)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT 
                announcement_id AS AnnouncementId,
                tournament_id AS TournamentId,
                title AS Title,
                content AS Content,
                announcement_type::TEXT AS AnnouncementType,
                target_audience AS TargetAudience,
                is_published AS IsPublished,
                is_pinned AS IsPinned,
                publish_date AS PublishDate,
                expiry_date AS ExpiryDate,
                view_count AS ViewCount,
                created_by_name AS CreatedByName,
                created_on AS CreatedOn
            FROM fn_get_announcements(@TournamentId, @Limit)";

        return await connection.QueryAsync<Announcement>(sql, new { TournamentId = tournamentId, Limit = limit });
    }

    public async Task<Announcement?> GetByIdAsync(int announcementId)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT 
                announcement_id AS AnnouncementId,
                tournament_id AS TournamentId,
                title AS Title,
                content AS Content,
                announcement_type::TEXT AS AnnouncementType,
                target_audience AS TargetAudience,
                is_published AS IsPublished,
                is_pinned AS IsPinned,
                publish_date AS PublishDate,
                expiry_date AS ExpiryDate,
                view_count AS ViewCount,
                created_by_name AS CreatedByName,
                created_on AS CreatedOn
            FROM fn_get_announcement_by_id(@AnnouncementId)";

        return await connection.QueryFirstOrDefaultAsync<Announcement>(sql, new { AnnouncementId = announcementId });
    }

    public async Task<int> CreateAsync(string title, string content, int? tournamentId, string? announcementType,
        string? targetAudience, bool isPinned, DateTime? expiryDate, int? userId, string? username, string? ipAddress)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT fn_create_announcement(
                @Title, @Content, @TournamentId, @AnnouncementType,
                @TargetAudience, @IsPinned, @ExpiryDate,
                @UserId, @Username, @IpAddress
            )";

        return await connection.ExecuteScalarAsync<int>(sql, new
        {
            Title = title,
            Content = content,
            TournamentId = tournamentId,
            AnnouncementType = announcementType ?? "General",
            TargetAudience = targetAudience ?? "All",
            IsPinned = isPinned,
            ExpiryDate = expiryDate,
            UserId = userId,
            Username = username,
            IpAddress = ipAddress
        });
    }

    public async Task UpdateAsync(int announcementId, string title, string content, string? announcementType,
        string? targetAudience, bool isPinned, DateTime? expiryDate, int? userId, string? username, string? ipAddress)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT fn_update_announcement(
                @AnnouncementId, @Title, @Content, @AnnouncementType,
                @TargetAudience, @IsPinned, @ExpiryDate,
                @UserId, @Username, @IpAddress
            )";

        await connection.ExecuteAsync(sql, new
        {
            AnnouncementId = announcementId,
            Title = title,
            Content = content,
            AnnouncementType = announcementType,
            TargetAudience = targetAudience,
            IsPinned = isPinned,
            ExpiryDate = expiryDate,
            UserId = userId,
            Username = username,
            IpAddress = ipAddress
        });
    }

    public async Task DeleteAsync(int announcementId, int? userId, string? username, string? ipAddress)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = "SELECT fn_delete_announcement(@AnnouncementId, @UserId, @Username, @IpAddress)";

        await connection.ExecuteAsync(sql, new
        {
            AnnouncementId = announcementId,
            UserId = userId,
            Username = username,
            IpAddress = ipAddress
        });
    }
}
