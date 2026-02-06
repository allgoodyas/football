using Microsoft.AspNetCore.Mvc;
using FootballTournament.API.Common;
using FootballTournament.API.Models.DTOs;
using FootballTournament.API.Services;

namespace FootballTournament.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<AuthResponse>.FailResponse("Invalid request", 
                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

        var result = await _authService.LoginAsync(request.UsernameOrEmail, request.Password);
        
        if (result == null)
            return Unauthorized(ApiResponse<AuthResponse>.FailResponse("Invalid username/email or password"));

        return Ok(ApiResponse<AuthResponse>.SuccessResponse(result, "Login successful"));
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<AuthResponse>.FailResponse("Invalid request",
                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

        var result = await _authService.RegisterAsync(request);
        
        if (result == null)
            return BadRequest(ApiResponse<AuthResponse>.FailResponse("Username or email already exists"));

        return Ok(ApiResponse<AuthResponse>.SuccessResponse(result, "Registration successful"));
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<AuthResponse>.FailResponse("Invalid request",
                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

        var result = await _authService.RefreshTokenAsync(request.Token, request.RefreshToken);
        
        if (result == null)
            return Unauthorized(ApiResponse<AuthResponse>.FailResponse("Invalid or expired token"));

        return Ok(ApiResponse<AuthResponse>.SuccessResponse(result, "Token refreshed successfully"));
    }
}
