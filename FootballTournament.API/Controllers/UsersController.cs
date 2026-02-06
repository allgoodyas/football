using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FootballTournament.API.Common;
using FootballTournament.API.Models.DTOs;
using FootballTournament.API.Repositories;
using System.Security.Claims;

namespace FootballTournament.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    public UsersController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <summary>
    /// Get all users (Admin only)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<UserDto>>>> GetAll()
    {
        var users = await _userRepository.GetAllAsync();
        
        var userDtos = users.Select(u => new UserDto
        {
            UserId = u.UserId,
            Username = u.Username!,
            Email = u.Email!,
            FullName = u.FullName!,
            PhoneNumber = u.PhoneNumber,
            Role = u.Role!,
            IsActive = u.IsActive,
            CreatedOn = u.CreatedOn
        });

        return Ok(ApiResponse<IEnumerable<UserDto>>.SuccessResponse(userDtos));
    }

    /// <summary>
    /// Get user by ID (Admin only)
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetById(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        
        if (user == null)
            return NotFound(ApiResponse<UserDto>.FailResponse("User not found"));

        var userDto = new UserDto
        {
            UserId = user.UserId,
            Username = user.Username!,
            Email = user.Email!,
            FullName = user.FullName!,
            PhoneNumber = user.PhoneNumber,
            Role = user.Role!,
            IsActive = user.IsActive,
            CreatedOn = user.CreatedOn
        };

        return Ok(ApiResponse<UserDto>.SuccessResponse(userDto));
    }

    /// <summary>
    /// Update user role (Admin only)
    /// </summary>
    [HttpPut("role")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateRole([FromBody] UpdateUserRoleRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<bool>.FailResponse("Invalid request",
                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

        // Validate role
        if (request.Role is not ("Admin" or "Official" or "Spectator"))
            return BadRequest(ApiResponse<bool>.FailResponse("Invalid role. Must be Admin, Official, or Spectator"));

        // Get current user ID from token
        var currentUserId = GetCurrentUserId();

        // Prevent user from changing their own role
        if (request.UserId == currentUserId)
            return BadRequest(ApiResponse<bool>.FailResponse("You cannot change your own role"));

        // Check if target user exists
        var targetUser = await _userRepository.GetByIdAsync(request.UserId);
        if (targetUser == null)
            return NotFound(ApiResponse<bool>.FailResponse("User not found"));

        var result = await _userRepository.UpdateUserRoleAsync(request.UserId, request.Role, currentUserId);
        
        if (!result)
            return BadRequest(ApiResponse<bool>.FailResponse("Failed to update user role"));

        return Ok(ApiResponse<bool>.SuccessResponse(true, $"User role updated to {request.Role}"));
    }

    /// <summary>
    /// Update user status (activate/deactivate) (Admin only)
    /// </summary>
    [HttpPut("status")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateStatus([FromBody] UpdateUserStatusRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<bool>.FailResponse("Invalid request",
                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

        // Get current user ID from token
        var currentUserId = GetCurrentUserId();

        // Prevent user from deactivating themselves
        if (request.UserId == currentUserId && !request.IsActive)
            return BadRequest(ApiResponse<bool>.FailResponse("You cannot deactivate your own account"));

        // Check if target user exists
        var targetUser = await _userRepository.GetByIdAsync(request.UserId);
        if (targetUser == null)
            return NotFound(ApiResponse<bool>.FailResponse("User not found"));

        var result = await _userRepository.UpdateUserStatusAsync(request.UserId, request.IsActive, currentUserId);
        
        if (!result)
            return BadRequest(ApiResponse<bool>.FailResponse("Failed to update user status"));

        var statusText = request.IsActive ? "activated" : "deactivated";
        return Ok(ApiResponse<bool>.SuccessResponse(true, $"User account {statusText}"));
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                          ?? User.FindFirst("sub")?.Value;
        
        if (int.TryParse(userIdClaim, out var userId))
            return userId;
        
        return null;
    }
}
