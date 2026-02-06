using FootballTournament.API.Models.DTOs;
using FootballTournament.API.Models.Entities;
using FootballTournament.API.Repositories;
using System.Security.Cryptography;
using System.Text;

namespace FootballTournament.API.Services;

public interface IAuthService
{
    Task<AuthResponse?> LoginAsync(string usernameOrEmail, string password);
    Task<AuthResponse?> RegisterAsync(RegisterRequest request);
    Task<AuthResponse?> RefreshTokenAsync(string token, string refreshToken);
}

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _configuration;

    public AuthService(IUserRepository userRepository, ITokenService tokenService, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _configuration = configuration;
    }

    public async Task<AuthResponse?> LoginAsync(string usernameOrEmail, string password)
    {
        var user = await _userRepository.GetByUsernameOrEmailAsync(usernameOrEmail);
        
        if (user == null)
            return null;

        // Verify password
        var passwordHash = HashPassword(password, user.PasswordSalt!);
        if (passwordHash != user.PasswordHash)
            return null;

        // Generate tokens
        var accessToken = _tokenService.GenerateAccessToken(user.UserId, user.Username!, user.Email!, user.Role!);
        var refreshToken = _tokenService.GenerateRefreshToken();
        var jwtId = _tokenService.GetJwtIdFromToken(accessToken);

        // Save refresh token
        var refreshTokenExpiryDays = int.Parse(_configuration["JwtSettings:RefreshTokenExpiryDays"] ?? "7");
        await _userRepository.SaveRefreshTokenAsync(user.UserId, refreshToken, jwtId!, DateTime.UtcNow.AddDays(refreshTokenExpiryDays));

        var expiryMinutes = int.Parse(_configuration["JwtSettings:ExpiryMinutes"] ?? "60");

        return new AuthResponse
        {
            UserId = user.UserId,
            Username = user.Username,
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role,
            Token = accessToken,
            RefreshToken = refreshToken,
            TokenExpiry = DateTime.UtcNow.AddMinutes(expiryMinutes)
        };
    }

    public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
    {
        // Check if user already exists
        var existingUser = await _userRepository.GetByUsernameOrEmailAsync(request.Username);
        if (existingUser != null)
            return null;

        existingUser = await _userRepository.GetByUsernameOrEmailAsync(request.Email);
        if (existingUser != null)
            return null;

        // Generate salt and hash password
        var salt = GenerateSalt();
        var passwordHash = HashPassword(request.Password, salt);

        // Always register as Spectator - role can only be changed by Admin later
        var role = "Spectator";

        // Register user
        var userId = await _userRepository.RegisterUserAsync(
            request.Username,
            request.Email,
            passwordHash,
            salt,
            request.FullName,
            request.PhoneNumber,
            role
        );

        // Generate tokens
        var accessToken = _tokenService.GenerateAccessToken(userId, request.Username, request.Email, role);
        var refreshToken = _tokenService.GenerateRefreshToken();
        var jwtId = _tokenService.GetJwtIdFromToken(accessToken);

        // Save refresh token
        var refreshTokenExpiryDays = int.Parse(_configuration["JwtSettings:RefreshTokenExpiryDays"] ?? "7");
        await _userRepository.SaveRefreshTokenAsync(userId, refreshToken, jwtId!, DateTime.UtcNow.AddDays(refreshTokenExpiryDays));

        var expiryMinutes = int.Parse(_configuration["JwtSettings:ExpiryMinutes"] ?? "60");

        return new AuthResponse
        {
            UserId = userId,
            Username = request.Username,
            Email = request.Email,
            FullName = request.FullName,
            Role = role,
            Token = accessToken,
            RefreshToken = refreshToken,
            TokenExpiry = DateTime.UtcNow.AddMinutes(expiryMinutes)
        };
    }

    public async Task<AuthResponse?> RefreshTokenAsync(string token, string refreshToken)
    {
        // Validate the access token
        var principal = _tokenService.ValidateToken(token);
        if (principal == null)
            return null;

        // Get the JWT ID from the token
        var jwtId = _tokenService.GetJwtIdFromToken(token);
        if (jwtId == null)
            return null;

        // Validate the refresh token
        var tokenValidation = await _userRepository.ValidateRefreshTokenAsync(refreshToken);
        if (tokenValidation == null || !tokenValidation.Value.IsValid || tokenValidation.Value.JwtId != jwtId)
            return null;

        // Get user info
        var userIdClaim = principal.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier || c.Type == "sub");
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            return null;

        var username = principal.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Name || c.Type == "unique_name")?.Value;
        var email = principal.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email || c.Type == "email")?.Value;
        var role = principal.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value;

        if (username == null || email == null || role == null)
            return null;

        // Generate new tokens
        var newAccessToken = _tokenService.GenerateAccessToken(userId, username, email, role);
        var newRefreshToken = _tokenService.GenerateRefreshToken();
        var newJwtId = _tokenService.GetJwtIdFromToken(newAccessToken);

        // Save new refresh token
        var refreshTokenExpiryDays = int.Parse(_configuration["JwtSettings:RefreshTokenExpiryDays"] ?? "7");
        await _userRepository.SaveRefreshTokenAsync(userId, newRefreshToken, newJwtId!, DateTime.UtcNow.AddDays(refreshTokenExpiryDays));

        var expiryMinutes = int.Parse(_configuration["JwtSettings:ExpiryMinutes"] ?? "60");

        return new AuthResponse
        {
            UserId = userId,
            Username = username,
            Email = email,
            Role = role,
            Token = newAccessToken,
            RefreshToken = newRefreshToken,
            TokenExpiry = DateTime.UtcNow.AddMinutes(expiryMinutes)
        };
    }

    private static string GenerateSalt()
    {
        var saltBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(saltBytes);
        return Convert.ToBase64String(saltBytes);
    }

    private static string HashPassword(string password, string salt)
    {
        using var sha256 = SHA256.Create();
        var combinedBytes = Encoding.UTF8.GetBytes(password + salt);
        var hashBytes = sha256.ComputeHash(combinedBytes);
        return Convert.ToBase64String(hashBytes);
    }

}
