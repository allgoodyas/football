using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FootballTournament.API.Common;
using FootballTournament.API.Models.Entities;
using FootballTournament.API.Repositories;

namespace FootballTournament.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TournamentsController : ControllerBase
{
    private readonly ITournamentRepository _tournamentRepository;
    private readonly ILogger<TournamentsController> _logger;

    public TournamentsController(
        ITournamentRepository tournamentRepository,
        ILogger<TournamentsController> logger)
    {
        _tournamentRepository = tournamentRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get all tournaments
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<Tournament>>>> GetAll()
    {
        try
        {
            var tournaments = await _tournamentRepository.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<Tournament>>.SuccessResponse(tournaments));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tournaments");
            return StatusCode(500, ApiResponse<IEnumerable<Tournament>>.FailResponse("Failed to retrieve tournaments"));
        }
    }

    /// <summary>
    /// Get tournament by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<Tournament>>> GetById(int id)
    {
        try
        {
            var tournament = await _tournamentRepository.GetByIdAsync(id);
            if (tournament == null)
            {
                return NotFound(ApiResponse<Tournament>.FailResponse("Tournament not found"));
            }
            return Ok(ApiResponse<Tournament>.SuccessResponse(tournament));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tournament {Id}", id);
            return StatusCode(500, ApiResponse<Tournament>.FailResponse("Failed to retrieve tournament"));
        }
    }

    /// <summary>
    /// Get active tournament
    /// </summary>
    [HttpGet("active")]
    public async Task<ActionResult<ApiResponse<Tournament>>> GetActive()
    {
        try
        {
            var tournament = await _tournamentRepository.GetActiveAsync();
            if (tournament == null)
            {
                return NotFound(ApiResponse<Tournament>.FailResponse("No active tournament found"));
            }
            return Ok(ApiResponse<Tournament>.SuccessResponse(tournament));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active tournament");
            return StatusCode(500, ApiResponse<Tournament>.FailResponse("Failed to retrieve active tournament"));
        }
    }

    /// <summary>
    /// Create a new tournament
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<int>>> Create([FromBody] CreateTournamentRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var username = GetCurrentUsername();
            var ipAddress = GetClientIpAddress();

            var id = await _tournamentRepository.CreateAsync(
                request.TournamentName,
                request.Description,
                request.StartDate,
                request.EndDate,
                request.TournamentType,
                request.MaxTeams,
                request.InstitutionId,
                request.OrganizerName,
                request.OrganizerContact,
                userId,
                username,
                ipAddress
            );

            return Ok(ApiResponse<int>.SuccessResponse(id, "Tournament created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating tournament");
            return StatusCode(500, ApiResponse<int>.FailResponse("Failed to create tournament"));
        }
    }

    /// <summary>
    /// Update a tournament
    /// </summary>
    [HttpPut]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<bool>>> Update([FromBody] UpdateTournamentRequest request)
    {
        try
        {
            var existing = await _tournamentRepository.GetByIdAsync(request.TournamentId);
            if (existing == null)
            {
                return NotFound(ApiResponse<bool>.FailResponse("Tournament not found"));
            }

            var userId = GetCurrentUserId();
            var username = GetCurrentUsername();
            var ipAddress = GetClientIpAddress();

            await _tournamentRepository.UpdateAsync(
                request.TournamentId,
                request.TournamentName,
                request.Description,
                request.StartDate,
                request.EndDate,
                request.TournamentType,
                request.Status,
                request.MaxTeams,
                request.InstitutionId,
                request.OrganizerName,
                request.OrganizerContact,
                userId,
                username,
                ipAddress
            );

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Tournament updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating tournament {Id}", request.TournamentId);
            return StatusCode(500, ApiResponse<bool>.FailResponse("Failed to update tournament"));
        }
    }

    /// <summary>
    /// Update tournament status
    /// </summary>
    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateStatus(int id, [FromBody] UpdateStatusRequest request)
    {
        try
        {
            var existing = await _tournamentRepository.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound(ApiResponse<bool>.FailResponse("Tournament not found"));
            }

            var userId = GetCurrentUserId();
            var username = GetCurrentUsername();
            var ipAddress = GetClientIpAddress();

            await _tournamentRepository.UpdateAsync(
                id,
                existing.TournamentName ?? "",
                existing.Description,
                existing.StartDate,
                existing.EndDate,
                existing.TournamentType ?? "Knockout",
                request.Status,
                existing.MaxTeams,
                existing.InstitutionId,
                existing.OrganizerName,
                existing.OrganizerContact,
                userId,
                username,
                ipAddress
            );

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Status updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating tournament status {Id}", id);
            return StatusCode(500, ApiResponse<bool>.FailResponse("Failed to update status"));
        }
    }

    /// <summary>
    /// Delete a tournament
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        try
        {
            var tournament = await _tournamentRepository.GetByIdAsync(id);
            if (tournament == null)
            {
                return NotFound(ApiResponse<bool>.FailResponse("Tournament not found"));
            }

            // Check if tournament can be deleted (not in progress or completed)
            if (tournament.Status == "Active" || tournament.Status == "InProgress" || tournament.Status == "Completed")
            {
                return BadRequest(ApiResponse<bool>.FailResponse("Cannot delete a tournament that is active, in progress or completed. Cancel it first."));
            }

            var userId = GetCurrentUserId();
            var username = GetCurrentUsername();
            var ipAddress = GetClientIpAddress();

            // Soft delete by setting status to Cancelled
            await _tournamentRepository.UpdateAsync(
                id,
                tournament.TournamentName ?? "",
                tournament.Description,
                tournament.StartDate,
                tournament.EndDate,
                tournament.TournamentType ?? "Knockout",
                "Cancelled",
                tournament.MaxTeams,
                tournament.InstitutionId,
                tournament.OrganizerName,
                tournament.OrganizerContact,
                userId,
                username,
                ipAddress
            );

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Tournament deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting tournament {Id}", id);
            return StatusCode(500, ApiResponse<bool>.FailResponse("Failed to delete tournament"));
        }
    }

    /// <summary>
    /// Get tournament statistics
    /// </summary>
    [HttpGet("{id}/stats")]
    public async Task<ActionResult<ApiResponse<DashboardStats>>> GetStats(int id)
    {
        try
        {
            var stats = await _tournamentRepository.GetDashboardStatsAsync(id);
            if (stats == null)
            {
                return NotFound(ApiResponse<DashboardStats>.FailResponse("Tournament not found"));
            }
            return Ok(ApiResponse<DashboardStats>.SuccessResponse(stats));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tournament stats {Id}", id);
            return StatusCode(500, ApiResponse<DashboardStats>.FailResponse("Failed to retrieve stats"));
        }
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("UserId");
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
        {
            return userId;
        }
        return null;
    }

    private string? GetCurrentUsername()
    {
        return User.FindFirst("Username")?.Value ?? User.Identity?.Name;
    }

    private string? GetClientIpAddress()
    {
        return HttpContext.Connection.RemoteIpAddress?.ToString();
    }
}

// ===================== REQUEST DTOs =====================

public class CreateTournamentRequest
{
    public required string TournamentName { get; set; }
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public required string TournamentType { get; set; } // League, Knockout, GroupKnockout
    public int MaxTeams { get; set; } = 8;
    public int? InstitutionId { get; set; }
    public string? OrganizerName { get; set; }
    public string? OrganizerContact { get; set; }
}

public class UpdateTournamentRequest
{
    public int TournamentId { get; set; }
    public required string TournamentName { get; set; }
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public required string TournamentType { get; set; }
    public required string Status { get; set; } // Upcoming, Active, Completed, Cancelled
    public int MaxTeams { get; set; }
    public int? InstitutionId { get; set; }
    public string? OrganizerName { get; set; }
    public string? OrganizerContact { get; set; }
}

public class UpdateStatusRequest
{
    public required string Status { get; set; }
}
