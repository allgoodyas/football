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
public class MatchesController : ControllerBase
{
    private readonly IMatchRepository _matchRepository;

    public MatchesController(IMatchRepository matchRepository)
    {
        _matchRepository = matchRepository;
    }

    [HttpGet("tournament/{tournamentId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<Match>>>> GetByTournament(int tournamentId)
    {
        var matches = await _matchRepository.GetAllByTournamentAsync(tournamentId);
        return Ok(ApiResponse<IEnumerable<Match>>.SuccessResponse(matches));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<Match>>> GetById(int id)
    {
        var match = await _matchRepository.GetByIdAsync(id);
        
        if (match == null)
            return NotFound(ApiResponse<Match>.FailResponse("Match not found"));

        return Ok(ApiResponse<Match>.SuccessResponse(match));
    }

    [HttpGet("upcoming/{tournamentId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<Match>>>> GetUpcoming(int tournamentId, [FromQuery] int count = 5)
    {
        var matches = await _matchRepository.GetUpcomingAsync(tournamentId, count);
        return Ok(ApiResponse<IEnumerable<Match>>.SuccessResponse(matches));
    }

    [HttpGet("bracket/{tournamentId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<Match>>>> GetBracket(int tournamentId)
    {
        var matches = await _matchRepository.GetBracketAsync(tournamentId);
        return Ok(ApiResponse<IEnumerable<Match>>.SuccessResponse(matches));
    }

    [HttpGet("live/{matchId}")]
    public async Task<ActionResult<ApiResponse<Match>>> GetLive(int matchId)
    {
        var match = await _matchRepository.GetLiveMatchAsync(matchId);
        
        if (match == null)
            return NotFound(ApiResponse<Match>.FailResponse("Match not found or not live"));

        return Ok(ApiResponse<Match>.SuccessResponse(match));
    }

    [HttpGet("{matchId}/events")]
    public async Task<ActionResult<ApiResponse<IEnumerable<MatchEvent>>>> GetEvents(int matchId)
    {
        var events = await _matchRepository.GetEventsAsync(matchId);
        return Ok(ApiResponse<IEnumerable<MatchEvent>>.SuccessResponse(events));
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<int>>> Create([FromBody] CreateMatchRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<int>.FailResponse("Invalid request",
                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

        var userId = GetCurrentUserId();
        var username = User.Identity?.Name;
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        var matchId = await _matchRepository.CreateAsync(
            request.TournamentId, request.MatchDate, request.Round, request.HomeTeamId,
            request.AwayTeamId, request.VenueId, request.MatchTime, request.GroupName,
            request.IsKnockout, request.RefereeName, request.Notes, userId, username, ipAddress
        );

        return Ok(ApiResponse<int>.SuccessResponse(matchId, "Match created successfully"));
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpPut]
    public async Task<ActionResult<ApiResponse<bool>>> Update([FromBody] UpdateMatchRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<bool>.FailResponse("Invalid request",
                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

        var userId = GetCurrentUserId();
        var username = User.Identity?.Name;
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        await _matchRepository.UpdateAsync(
            request.MatchId, request.MatchDate, request.Round, request.HomeTeamId,
            request.AwayTeamId, request.VenueId, request.MatchTime, request.GroupName,
            request.IsKnockout, request.RefereeName, request.Notes, userId, username, ipAddress
        );

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Match updated successfully"));
    }

    [Authorize(Policy = "OfficialAccess")]
    [HttpPut("score")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateScore([FromBody] UpdateMatchScoreRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<bool>.FailResponse("Invalid request",
                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

        var userId = GetCurrentUserId();
        var username = User.Identity?.Name;
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        await _matchRepository.UpdateScoreAsync(
            request.MatchId, request.HomeScore, request.AwayScore, request.Status,
            request.CurrentMinute, request.HomeScoreHT, request.AwayScoreHT,
            request.HomePenalties, request.AwayPenalties, request.IsExtraTime,
            request.IsPenaltyShootout, userId, username, ipAddress
        );

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Score updated successfully"));
    }

    [Authorize(Policy = "OfficialAccess")]
    [HttpPost("events")]
    public async Task<ActionResult<ApiResponse<int>>> AddEvent([FromBody] AddMatchEventRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<int>.FailResponse("Invalid request",
                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

        var userId = GetCurrentUserId();
        var username = User.Identity?.Name;
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        int eventId;
        
        // Use the new method with GoalType if it's a goal event
        if (request.EventType.Equals("Goal", StringComparison.OrdinalIgnoreCase) ||
            request.EventType.Equals("GOAL", StringComparison.OrdinalIgnoreCase) ||
            request.EventType.Equals("Penalty", StringComparison.OrdinalIgnoreCase) ||
            request.EventType.Equals("OwnGoal", StringComparison.OrdinalIgnoreCase))
        {
            eventId = await _matchRepository.AddEventWithGoalTypeAsync(
                request.MatchId, request.TeamId, request.EventType, request.EventMinute,
                request.PlayerId, request.IsExtraTime, request.AssistPlayerId,
                request.SubstitutedPlayerId, request.Description, request.GoalType,
                userId, username, ipAddress
            );
        }
        else
        {
            eventId = await _matchRepository.AddEventAsync(
                request.MatchId, request.TeamId, request.EventType, request.EventMinute,
                request.PlayerId, request.IsExtraTime, request.AssistPlayerId,
                request.SubstitutedPlayerId, request.Description, userId, username, ipAddress
            );
        }

        return Ok(ApiResponse<int>.SuccessResponse(eventId, "Event added successfully"));
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        var userId = GetCurrentUserId();
        var username = User.Identity?.Name;
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        await _matchRepository.DeleteAsync(id, userId, username, ipAddress);
        return Ok(ApiResponse<bool>.SuccessResponse(true, "Match deleted successfully"));
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}
