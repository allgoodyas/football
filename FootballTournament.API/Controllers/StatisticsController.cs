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
public class StatisticsController : ControllerBase
{
    private readonly IPlayerStatsRepository _playerStatsRepository;

    public StatisticsController(IPlayerStatsRepository playerStatsRepository)
    {
        _playerStatsRepository = playerStatsRepository;
    }

    [HttpGet("top-scorers/{tournamentId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<TopScorer>>>> GetTopScorers(
        int tournamentId, [FromQuery] int limit = 10)
    {
        var scorers = await _playerStatsRepository.GetTopScorersAsync(tournamentId, limit);
        return Ok(ApiResponse<IEnumerable<TopScorer>>.SuccessResponse(scorers));
    }

    [HttpGet("top-assisters/{tournamentId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<TopAssister>>>> GetTopAssisters(
        int tournamentId, [FromQuery] int limit = 10)
    {
        var assisters = await _playerStatsRepository.GetTopAssistersAsync(tournamentId, limit);
        return Ok(ApiResponse<IEnumerable<TopAssister>>.SuccessResponse(assisters));
    }

    [HttpGet("player/{playerId}/tournament/{tournamentId}")]
    public async Task<ActionResult<ApiResponse<PlayerStatistics>>> GetPlayerStats(int playerId, int tournamentId)
    {
        var stats = await _playerStatsRepository.GetPlayerStatsAsync(playerId, tournamentId);
        
        if (stats == null)
            return NotFound(ApiResponse<PlayerStatistics>.FailResponse("Player statistics not found"));

        return Ok(ApiResponse<PlayerStatistics>.SuccessResponse(stats));
    }

    [HttpGet("teams/{tournamentId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<TeamStatistics>>>> GetTeamStatistics(int tournamentId)
    {
        var stats = await _playerStatsRepository.GetTeamStatisticsAsync(tournamentId);
        return Ok(ApiResponse<IEnumerable<TeamStatistics>>.SuccessResponse(stats));
    }

    [HttpGet("tournament-summary/{tournamentId}")]
    public async Task<ActionResult<ApiResponse<TournamentSummary>>> GetTournamentSummary(int tournamentId)
    {
        var summary = await _playerStatsRepository.GetTournamentSummaryAsync(tournamentId);
        
        if (summary == null)
            return NotFound(ApiResponse<TournamentSummary>.FailResponse("Tournament not found"));

        return Ok(ApiResponse<TournamentSummary>.SuccessResponse(summary));
    }

    [HttpGet("goals-by-time/{tournamentId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<GoalsByTime>>>> GetGoalsByTime(int tournamentId)
    {
        var goalsByTime = await _playerStatsRepository.GetGoalsByTimeAsync(tournamentId);
        return Ok(ApiResponse<IEnumerable<GoalsByTime>>.SuccessResponse(goalsByTime));
    }

    [HttpGet("goals-by-day/{tournamentId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<GoalsByMatchDay>>>> GetGoalsByMatchDay(int tournamentId)
    {
        var goalsByDay = await _playerStatsRepository.GetGoalsByMatchDayAsync(tournamentId);
        return Ok(ApiResponse<IEnumerable<GoalsByMatchDay>>.SuccessResponse(goalsByDay));
    }

    [HttpGet("most-carded/{tournamentId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<CardedPlayer>>>> GetMostCardedPlayers(
        int tournamentId, [FromQuery] int limit = 10)
    {
        var cardedPlayers = await _playerStatsRepository.GetMostCardedPlayersAsync(tournamentId, limit);
        return Ok(ApiResponse<IEnumerable<CardedPlayer>>.SuccessResponse(cardedPlayers));
    }

    [HttpGet("man-of-the-match/{tournamentId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ManOfTheMatch>>>> GetManOfTheMatchList(int tournamentId)
    {
        var motmList = await _playerStatsRepository.GetManOfTheMatchListAsync(tournamentId);
        return Ok(ApiResponse<IEnumerable<ManOfTheMatch>>.SuccessResponse(motmList));
    }

    [Authorize(Policy = "OfficialAccess")]
    [HttpPost("record-match-stats")]
    public async Task<ActionResult<ApiResponse<bool>>> RecordMatchStats([FromBody] UpdatePlayerStatsRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<bool>.FailResponse("Invalid request",
                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

        var userId = GetCurrentUserId();
        var username = User.Identity?.Name;
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        // Get team ID from player (would need to add this method or get from request)
        await _playerStatsRepository.RecordPlayerMatchStatsAsync(
            request.MatchId, request.PlayerId, 0, // TeamId would need to be determined
            request.Goals, request.Assists, request.YellowCards, request.RedCards,
            request.MinutesPlayed, request.IsManOfTheMatch, userId, username, ipAddress
        );

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Match stats recorded successfully"));
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}
