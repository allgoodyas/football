using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FootballTournament.API.Common;
using FootballTournament.API.Models.DTOs;
using FootballTournament.API.Models.Entities;
using FootballTournament.API.Repositories;
using System.Security.Claims;

namespace FootballTournament.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RefereesController : ControllerBase
{
    private readonly IRefereeRepository _refereeRepository;

    public RefereesController(IRefereeRepository refereeRepository)
    {
        _refereeRepository = refereeRepository;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<Referee>>>> GetAll()
    {
        var referees = await _refereeRepository.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<Referee>>.SuccessResponse(referees));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<Referee>>> GetById(int id)
    {
        var referee = await _refereeRepository.GetByIdAsync(id);
        
        if (referee == null)
            return NotFound(ApiResponse<Referee>.FailResponse("Referee not found"));

        return Ok(ApiResponse<Referee>.SuccessResponse(referee));
    }

    [HttpGet("available")]
    public async Task<ActionResult<ApiResponse<IEnumerable<Referee>>>> GetAvailable([FromQuery] DateTime date)
    {
        var referees = await _refereeRepository.GetAvailableAsync(date);
        return Ok(ApiResponse<IEnumerable<Referee>>.SuccessResponse(referees));
    }

    [HttpGet("{refereeId}/schedule")]
    public async Task<ActionResult<ApiResponse<IEnumerable<MatchReferee>>>> GetSchedule(
        int refereeId, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var schedule = await _refereeRepository.GetRefereeScheduleAsync(refereeId, startDate, endDate);
        return Ok(ApiResponse<IEnumerable<MatchReferee>>.SuccessResponse(schedule));
    }

    [HttpGet("match/{matchId}/assignments")]
    public async Task<ActionResult<ApiResponse<IEnumerable<MatchReferee>>>> GetMatchAssignments(int matchId)
    {
        var assignments = await _refereeRepository.GetMatchAssignmentsAsync(matchId);
        return Ok(ApiResponse<IEnumerable<MatchReferee>>.SuccessResponse(assignments));
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<int>>> Create([FromBody] CreateRefereeRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<int>.FailResponse("Invalid request",
                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

        var userId = GetCurrentUserId();
        var username = User.Identity?.Name;
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        var refereeId = await _refereeRepository.CreateAsync(
            request.FullName, request.Email, request.PhoneNumber, request.Role,
            request.ExperienceYears, request.Certification, userId, username, ipAddress
        );

        return Ok(ApiResponse<int>.SuccessResponse(refereeId, "Referee created successfully"));
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpPut]
    public async Task<ActionResult<ApiResponse<bool>>> Update([FromBody] UpdateRefereeRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<bool>.FailResponse("Invalid request",
                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

        var userId = GetCurrentUserId();
        var username = User.Identity?.Name;
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        await _refereeRepository.UpdateAsync(
            request.RefereeId, request.FullName, request.Email, request.PhoneNumber,
            request.Role, request.ExperienceYears, request.Certification, userId, username, ipAddress
        );

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Referee updated successfully"));
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpPut("availability")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateAvailability([FromBody] UpdateRefereeAvailabilityRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<bool>.FailResponse("Invalid request",
                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

        var userId = GetCurrentUserId();
        var username = User.Identity?.Name;
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        await _refereeRepository.UpdateAvailabilityAsync(
            request.RefereeId, request.Date, request.StartTime, request.EndTime,
            request.Status, request.Notes, userId, username, ipAddress
        );

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Availability updated successfully"));
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpPost("assign")]
    public async Task<ActionResult<ApiResponse<bool>>> AssignToMatch([FromBody] AssignRefereeRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<bool>.FailResponse("Invalid request",
                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

        var userId = GetCurrentUserId();
        var username = User.Identity?.Name;
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        await _refereeRepository.AssignToMatchAsync(
            request.MatchId, request.RefereeId, request.RefereeRole, userId, username, ipAddress
        );

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Referee assigned successfully"));
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpDelete("{refereeId}/match/{matchId}")]
    public async Task<ActionResult<ApiResponse<bool>>> RemoveFromMatch(int refereeId, int matchId)
    {
        var userId = GetCurrentUserId();
        var username = User.Identity?.Name;
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        await _refereeRepository.RemoveFromMatchAsync(matchId, refereeId, userId, username, ipAddress);
        return Ok(ApiResponse<bool>.SuccessResponse(true, "Referee removed from match successfully"));
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        var userId = GetCurrentUserId();
        var username = User.Identity?.Name;
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        await _refereeRepository.DeleteAsync(id, userId, username, ipAddress);
        return Ok(ApiResponse<bool>.SuccessResponse(true, "Referee deleted successfully"));
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}
