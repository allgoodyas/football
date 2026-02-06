using Dapper;
using FootballTournament.API.Data;
using FootballTournament.API.Models.Entities;

namespace FootballTournament.API.Repositories;

public interface IUserRepository
{
    Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail);
    Task<User?> GetByIdAsync(int userId);
    Task<IEnumerable<User>> GetAllAsync();
    Task<int> RegisterUserAsync(string username, string email, string passwordHash, string passwordSalt, string fullName, string? phoneNumber, string role = "Spectator");
    Task<bool> UpdateUserRoleAsync(int userId, string role, int? updatedBy);
    Task<bool> UpdateUserStatusAsync(int userId, bool isActive, int? updatedBy);
    Task SaveRefreshTokenAsync(int userId, string refreshToken, string jwtId, DateTime expiryDate);
    Task<(int TokenId, int UserId, string JwtId, bool IsValid)?> ValidateRefreshTokenAsync(string refreshToken);
}

public class UserRepository : IUserRepository
{
    private readonly IDbContext _context;

    public UserRepository(IDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT 
                user_id AS UserId,
                username AS Username,
                email AS Email,
                password_hash AS PasswordHash,
                password_salt AS PasswordSalt,
                full_name AS FullName,
                role::TEXT AS Role
            FROM fn_user_login(@UsernameOrEmail)";

        return await connection.QueryFirstOrDefaultAsync<User>(sql, new { UsernameOrEmail = usernameOrEmail });
    }

    public async Task<User?> GetByIdAsync(int userId)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT 
                user_id AS UserId,
                username AS Username,
                email AS Email,
                full_name AS FullName,
                phone_number AS PhoneNumber,
                role::TEXT AS Role,
                is_active AS IsActive,
                last_login AS LastLogin,
                created_on AS CreatedOn
            FROM users
            WHERE user_id = @UserId";

        return await connection.QueryFirstOrDefaultAsync<User>(sql, new { UserId = userId });
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT 
                user_id AS UserId,
                username AS Username,
                email AS Email,
                full_name AS FullName,
                phone_number AS PhoneNumber,
                role::TEXT AS Role,
                is_active AS IsActive,
                last_login AS LastLogin,
                created_on AS CreatedOn
            FROM users
            ORDER BY created_on DESC";

        return await connection.QueryAsync<User>(sql);
    }

    public async Task<int> RegisterUserAsync(string username, string email, string passwordHash, string passwordSalt, string fullName, string? phoneNumber, string role = "Spectator")
    {
        using var connection = _context.CreateConnection();
        
        const string sql = "SELECT fn_register_user(@Username, @Email, @PasswordHash, @PasswordSalt, @FullName, @PhoneNumber, @Role)";

        return await connection.ExecuteScalarAsync<int>(sql, new
        {
            Username = username,
            Email = email,
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt,
            FullName = fullName,
            PhoneNumber = phoneNumber,
            Role = role
        });
    }

    public async Task<bool> UpdateUserRoleAsync(int userId, string role, int? updatedBy)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            UPDATE users 
            SET role = @Role::user_role,
                edited_on = NOW(),
                edited_by = @UpdatedBy
            WHERE user_id = @UserId";

        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            UserId = userId,
            Role = role,
            UpdatedBy = updatedBy
        });

        return rowsAffected > 0;
    }

    public async Task<bool> UpdateUserStatusAsync(int userId, bool isActive, int? updatedBy)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            UPDATE users 
            SET is_active = @IsActive,
                edited_on = NOW(),
                edited_by = @UpdatedBy
            WHERE user_id = @UserId";

        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            UserId = userId,
            IsActive = isActive,
            UpdatedBy = updatedBy
        });

        return rowsAffected > 0;
    }

    public async Task SaveRefreshTokenAsync(int userId, string refreshToken, string jwtId, DateTime expiryDate)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = "SELECT fn_save_refresh_token(@UserId, @RefreshToken, @JwtId, @ExpiryDate)";

        await connection.ExecuteAsync(sql, new
        {
            UserId = userId,
            RefreshToken = refreshToken,
            JwtId = jwtId,
            ExpiryDate = expiryDate
        });
    }

    public async Task<(int TokenId, int UserId, string JwtId, bool IsValid)?> ValidateRefreshTokenAsync(string refreshToken)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT 
                token_id AS TokenId,
                user_id AS UserId,
                jwt_id AS JwtId,
                is_valid AS IsValid
            FROM fn_validate_refresh_token(@RefreshToken)";

        var result = await connection.QueryFirstOrDefaultAsync<dynamic>(sql, new { RefreshToken = refreshToken });
        
        if (result == null)
            return null;

        return (result.tokenid, result.userid, result.jwtid, result.isvalid);
    }
}
