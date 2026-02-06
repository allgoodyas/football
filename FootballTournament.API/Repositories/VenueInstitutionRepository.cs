using Dapper;
using FootballTournament.API.Data;
using FootballTournament.API.Models.Entities;

namespace FootballTournament.API.Repositories;

public interface IVenueRepository
{
    Task<IEnumerable<Venue>> GetAllAsync();
    Task<Venue?> GetByIdAsync(int venueId);
    Task<VenueStats> GetStatsAsync();
    Task<IEnumerable<VenueSchedule>> GetTodayScheduleAsync();
    Task<IEnumerable<FacilityStatus>> GetFacilityStatusAsync();
    Task<int> CreateAsync(string venueName, string location, int? capacity, bool hasFloodlights,
        string? fieldSize, string? surfaceType, string? facilities, int? institutionId,
        int? userId, string? username, string? ipAddress);
    Task UpdateAsync(int venueId, string venueName, string location, int? capacity, bool hasFloodlights,
        string? fieldSize, string? surfaceType, string? facilities, int? institutionId,
        int? userId, string? username, string? ipAddress);
    Task UpdateStatusAsync(int venueId, string status, int? userId, string? username, string? ipAddress);
    Task DeleteAsync(int venueId, int? userId, string? username, string? ipAddress);
    Task<IEnumerable<VenueBooking>> GetBookingsAsync(int venueId, DateTime? date = null);
    Task<int> CreateBookingAsync(int venueId, int? matchId, DateTime bookingDate, TimeSpan startTime,
        TimeSpan endTime, string? purpose, int? userId, string? username, string? ipAddress);
    Task<bool> IsAvailableAsync(int venueId, DateTime date, TimeSpan startTime, TimeSpan endTime);
}

public interface IInstitutionRepository
{
    Task<IEnumerable<Institution>> GetAllAsync();
    Task<Institution?> GetByIdAsync(int institutionId);
    Task<int> CreateAsync(string institutionName, string institutionCode, string? address, string? city,
        string? country, string? contactEmail, string? contactPhone, string? website,
        int? userId, string? username, string? ipAddress);
    Task UpdateAsync(int institutionId, string institutionName, string institutionCode, string? address,
        string? city, string? country, string? contactEmail, string? contactPhone, string? website,
        int? userId, string? username, string? ipAddress);
    Task DeleteAsync(int institutionId, int? userId, string? username, string? ipAddress);
}

public class VenueRepository : IVenueRepository
{
    private readonly IDbContext _context;

    public VenueRepository(IDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Venue>> GetAllAsync()
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT 
                venue_id AS VenueId,
                venue_name AS VenueName,
                location AS Location,
                capacity AS Capacity,
                has_floodlights AS HasFloodlights,
                field_size AS FieldSize,
                surface_type AS SurfaceType,
                facilities AS Facilities,
                status AS Status,
                institution_id AS InstitutionId,
                institution_name AS InstitutionName,
                total_matches_hosted AS TotalMatchesHosted,
                rating AS Rating,
                is_active AS IsActive
            FROM fn_get_all_venues()";

        return await connection.QueryAsync<Venue>(sql);
    }

    public async Task<Venue?> GetByIdAsync(int venueId)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT 
                venue_id AS VenueId,
                venue_name AS VenueName,
                location AS Location,
                capacity AS Capacity,
                has_floodlights AS HasFloodlights,
                field_size AS FieldSize,
                surface_type AS SurfaceType,
                facilities AS Facilities,
                status AS Status,
                image_url AS ImageUrl,
                institution_id AS InstitutionId,
                total_matches_hosted AS TotalMatchesHosted,
                rating AS Rating,
                is_active AS IsActive
            FROM fn_get_venue_by_id(@VenueId)";

        return await connection.QueryFirstOrDefaultAsync<Venue>(sql, new { VenueId = venueId });
    }

    public async Task<VenueStats> GetStatsAsync()
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT 
                total_venues AS TotalVenues,
                available_now AS AvailableNow,
                total_matches_hosted AS TotalMatchesHosted
            FROM fn_get_venue_stats()";

        return await connection.QueryFirstOrDefaultAsync<VenueStats>(sql) ?? new VenueStats();
    }

    public async Task<IEnumerable<VenueSchedule>> GetTodayScheduleAsync()
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT 
                schedule_id AS ScheduleId,
                venue_id AS VenueId,
                venue_name AS VenueName,
                match_id AS MatchId,
                event_type AS EventType,
                title AS Title,
                start_time AS StartTime,
                end_time AS EndTime,
                status AS Status
            FROM fn_get_today_venue_schedule()";

        return await connection.QueryAsync<VenueSchedule>(sql);
    }

    public async Task<IEnumerable<FacilityStatus>> GetFacilityStatusAsync()
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT 
                name AS Name,
                icon AS Icon,
                status AS Status,
                details AS Details
            FROM fn_get_facility_status()";

        return await connection.QueryAsync<FacilityStatus>(sql);
    }

    public async Task<int> CreateAsync(string venueName, string location, int? capacity, bool hasFloodlights,
        string? fieldSize, string? surfaceType, string? facilities, int? institutionId,
        int? userId, string? username, string? ipAddress)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT fn_create_venue(
                @VenueName, @Location, @Capacity, @HasFloodlights,
                @FieldSize, @SurfaceType, @Facilities, @InstitutionId,
                @UserId, @Username, @IpAddress
            )";

        return await connection.ExecuteScalarAsync<int>(sql, new
        {
            VenueName = venueName,
            Location = location,
            Capacity = capacity,
            HasFloodlights = hasFloodlights,
            FieldSize = fieldSize,
            SurfaceType = surfaceType,
            Facilities = facilities,
            InstitutionId = institutionId,
            UserId = userId,
            Username = username,
            IpAddress = ipAddress
        });
    }

    public async Task UpdateAsync(int venueId, string venueName, string location, int? capacity, bool hasFloodlights,
        string? fieldSize, string? surfaceType, string? facilities, int? institutionId,
        int? userId, string? username, string? ipAddress)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT fn_update_venue(
                @VenueId, @VenueName, @Location, @Capacity, @HasFloodlights,
                @FieldSize, @SurfaceType, @Facilities, @InstitutionId,
                @UserId, @Username, @IpAddress
            )";

        await connection.ExecuteAsync(sql, new
        {
            VenueId = venueId,
            VenueName = venueName,
            Location = location,
            Capacity = capacity,
            HasFloodlights = hasFloodlights,
            FieldSize = fieldSize,
            SurfaceType = surfaceType,
            Facilities = facilities,
            InstitutionId = institutionId,
            UserId = userId,
            Username = username,
            IpAddress = ipAddress
        });
    }

    public async Task UpdateStatusAsync(int venueId, string status, int? userId, string? username, string? ipAddress)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = "SELECT fn_update_venue_status(@VenueId, @Status, @UserId, @Username, @IpAddress)";

        await connection.ExecuteAsync(sql, new
        {
            VenueId = venueId,
            Status = status,
            UserId = userId,
            Username = username,
            IpAddress = ipAddress
        });
    }

    public async Task DeleteAsync(int venueId, int? userId, string? username, string? ipAddress)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = "SELECT fn_delete_venue(@VenueId, @UserId, @Username, @IpAddress)";

        await connection.ExecuteAsync(sql, new
        {
            VenueId = venueId,
            UserId = userId,
            Username = username,
            IpAddress = ipAddress
        });
    }

    public async Task<IEnumerable<VenueBooking>> GetBookingsAsync(int venueId, DateTime? date = null)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT 
                booking_id AS BookingId,
                venue_id AS VenueId,
                match_id AS MatchId,
                booking_date AS BookingDate,
                start_time AS StartTime,
                end_time AS EndTime,
                purpose AS Purpose,
                status AS Status,
                booked_by AS BookedBy,
                match_info AS MatchInfo
            FROM fn_get_venue_bookings(@VenueId, @Date)";

        return await connection.QueryAsync<VenueBooking>(sql, new { VenueId = venueId, Date = date });
    }

    public async Task<int> CreateBookingAsync(int venueId, int? matchId, DateTime bookingDate, TimeSpan startTime,
        TimeSpan endTime, string? purpose, int? userId, string? username, string? ipAddress)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT fn_create_venue_booking(
                @VenueId, @MatchId, @BookingDate, @StartTime, @EndTime, @Purpose,
                @UserId, @Username, @IpAddress
            )";

        return await connection.ExecuteScalarAsync<int>(sql, new
        {
            VenueId = venueId,
            MatchId = matchId,
            BookingDate = bookingDate,
            StartTime = startTime,
            EndTime = endTime,
            Purpose = purpose ?? "Match",
            UserId = userId,
            Username = username,
            IpAddress = ipAddress
        });
    }

    public async Task<bool> IsAvailableAsync(int venueId, DateTime date, TimeSpan startTime, TimeSpan endTime)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = "SELECT fn_is_venue_available(@VenueId, @Date, @StartTime, @EndTime)";

        return await connection.ExecuteScalarAsync<bool>(sql, new
        {
            VenueId = venueId,
            Date = date,
            StartTime = startTime,
            EndTime = endTime
        });
    }
}

public class InstitutionRepository : IInstitutionRepository
{
    private readonly IDbContext _context;

    public InstitutionRepository(IDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Institution>> GetAllAsync()
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT 
                institution_id AS InstitutionId,
                institution_name AS InstitutionName,
                institution_code AS InstitutionCode,
                address AS Address,
                city AS City,
                country AS Country,
                contact_email AS ContactEmail,
                contact_phone AS ContactPhone,
                website AS Website,
                logo_url AS LogoUrl,
                is_active AS IsActive
            FROM fn_get_institutions()";

        return await connection.QueryAsync<Institution>(sql);
    }

    public async Task<Institution?> GetByIdAsync(int institutionId)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT 
                institution_id AS InstitutionId,
                institution_name AS InstitutionName,
                institution_code AS InstitutionCode,
                address AS Address,
                city AS City,
                country AS Country,
                contact_email AS ContactEmail,
                contact_phone AS ContactPhone,
                website AS Website,
                logo_url AS LogoUrl,
                is_active AS IsActive
            FROM fn_get_institution_by_id(@InstitutionId)";

        return await connection.QueryFirstOrDefaultAsync<Institution>(sql, new { InstitutionId = institutionId });
    }

    public async Task<int> CreateAsync(string institutionName, string institutionCode, string? address, string? city,
        string? country, string? contactEmail, string? contactPhone, string? website,
        int? userId, string? username, string? ipAddress)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT fn_create_institution(
                @InstitutionName, @InstitutionCode, @Address, @City, @Country,
                @ContactEmail, @ContactPhone, @Website,
                @UserId, @Username, @IpAddress
            )";

        return await connection.ExecuteScalarAsync<int>(sql, new
        {
            InstitutionName = institutionName,
            InstitutionCode = institutionCode,
            Address = address,
            City = city,
            Country = country,
            ContactEmail = contactEmail,
            ContactPhone = contactPhone,
            Website = website,
            UserId = userId,
            Username = username,
            IpAddress = ipAddress
        });
    }

    public async Task UpdateAsync(int institutionId, string institutionName, string institutionCode, string? address,
        string? city, string? country, string? contactEmail, string? contactPhone, string? website,
        int? userId, string? username, string? ipAddress)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT fn_update_institution(
                @InstitutionId, @InstitutionName, @InstitutionCode, @Address, @City, @Country,
                @ContactEmail, @ContactPhone, @Website,
                @UserId, @Username, @IpAddress
            )";

        await connection.ExecuteAsync(sql, new
        {
            InstitutionId = institutionId,
            InstitutionName = institutionName,
            InstitutionCode = institutionCode,
            Address = address,
            City = city,
            Country = country,
            ContactEmail = contactEmail,
            ContactPhone = contactPhone,
            Website = website,
            UserId = userId,
            Username = username,
            IpAddress = ipAddress
        });
    }

    public async Task DeleteAsync(int institutionId, int? userId, string? username, string? ipAddress)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = "SELECT fn_delete_institution(@InstitutionId, @UserId, @Username, @IpAddress)";

        await connection.ExecuteAsync(sql, new
        {
            InstitutionId = institutionId,
            UserId = userId,
            Username = username,
            IpAddress = ipAddress
        });
    }
}
