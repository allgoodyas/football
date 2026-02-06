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
public class TeamsController : ControllerBase
{
    private readonly ITeamRepository _teamRepository;

    public TeamsController(ITeamRepository teamRepository)
    {
        _teamRepository = teamRepository;
    }

    [HttpGet("tournament/{tournamentId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<Team>>>> GetByTournament(int tournamentId)
    {
        var teams = await _teamRepository.GetAllByTournamentAsync(tournamentId);
        return Ok(ApiResponse<IEnumerable<Team>>.SuccessResponse(teams));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<Team>>>> GetAll()
    {
        var teams = await _teamRepository.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<Team>>.SuccessResponse(teams));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<Team>>> GetById(int id)
    {
        var team = await _teamRepository.GetByIdAsync(id);
        
        if (team == null)
            return NotFound(ApiResponse<Team>.FailResponse("Team not found"));

        return Ok(ApiResponse<Team>.SuccessResponse(team));
    }

    [HttpGet("standings/{tournamentId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<TeamStanding>>>> GetStandings(int tournamentId, [FromQuery] string? groupName = null)
    {
        var standings = await _teamRepository.GetStandingsAsync(tournamentId, groupName);
        return Ok(ApiResponse<IEnumerable<TeamStanding>>.SuccessResponse(standings));
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<int>>> Create([FromBody] CreateTeamRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<int>.FailResponse("Invalid request",
                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

        var userId = GetCurrentUserId();
        var username = User.Identity?.Name;
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        var teamId = await _teamRepository.CreateAsync(
            request.TournamentId,
            request.TeamName,
            request.TeamCode,
            request.ClassName,
            request.CaptainName,
            request.CaptainContact,
            request.CoachName,
            request.TeamColor,
            request.GroupName,
            request.InstitutionId,
            userId,
            username,
            ipAddress
        );

        return Ok(ApiResponse<int>.SuccessResponse(teamId, "Team created successfully"));
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpPut]
    public async Task<ActionResult<ApiResponse<bool>>> Update([FromBody] UpdateTeamRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<bool>.FailResponse("Invalid request",
                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

        var userId = GetCurrentUserId();
        var username = User.Identity?.Name;
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        await _teamRepository.UpdateAsync(
            request.TeamId,
            request.TeamName,
            request.TeamCode,
            request.ClassName,
            request.CaptainName,
            request.CaptainContact,
            request.CoachName,
            request.TeamColor,
            request.GroupName,
            request.InstitutionId,
            userId,
            username,
            ipAddress
        );

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Team updated successfully"));
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        var userId = GetCurrentUserId();
        var username = User.Identity?.Name;
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        await _teamRepository.DeleteAsync(id, userId, username, ipAddress);
        return Ok(ApiResponse<bool>.SuccessResponse(true, "Team deleted successfully"));
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}
