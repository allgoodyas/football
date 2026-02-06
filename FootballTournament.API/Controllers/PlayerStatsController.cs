using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FootballTournament.API.Common;
using FootballTournament.API.Repositories;
using FootballTournament.API.Models.DTOs;
using FootballTournament.API.Models.Entities;
using System.Security.Claims;

namespace FootballTournament.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlayerStatsController : ControllerBase
{
    private readonly IPlayerStatsRepository _statsRepository;
    private readonly IMatchRepository _matchRepository;
    private readonly ILogger<PlayerStatsController> _logger;

    public PlayerStatsController(
        IPlayerStatsRepository statsRepository,
        IMatchRepository matchRepository,
        ILogger<PlayerStatsController> logger)
    {
        _statsRepository = statsRepository;
        _matchRepository = matchRepository;
        _logger = logger;
    }

    private int? GetUserId() => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
    private string? GetUsername() => User.FindFirstValue(ClaimTypes.Name);
    private string? GetIpAddress() => HttpContext.Connection.RemoteIpAddress?.ToString();

    /// <summary>
    /// Get top scorers for a tournament
    /// </summary>
    [HttpGet("tournament/{tournamentId}/top-scorers")]
    public async Task<ActionResult<ApiResponse<IEnumerable<TopScorer>>>> GetTopScorers(
        int tournamentId, [FromQuery] int limit = 10)
    {
        try
        {
            var scorers = await _statsRepository.GetTopScorersAsync(tournamentId, limit);
            return Ok(ApiResponse<IEnumerable<TopScorer>>.SuccessResponse(scorers));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting top scorers for tournament {TournamentId}", tournamentId);
            return StatusCode(500, ApiResponse<IEnumerable<TopScorer>>.ErrorResponse("Failed to get top scorers"));
        }
    }

    /// <summary>
    /// Get top assisters for a tournament
    /// </summary>
    [HttpGet("tournament/{tournamentId}/top-assisters")]
    public async Task<ActionResult<ApiResponse<IEnumerable<TopAssister>>>> GetTopAssisters(
        int tournamentId, [FromQuery] int limit = 10)
    {
        try
        {
            var assisters = await _statsRepository.GetTopAssistersAsync(tournamentId, limit);
            return Ok(ApiResponse<IEnumerable<TopAssister>>.SuccessResponse(assisters));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting top assisters for tournament {TournamentId}", tournamentId);
            return StatusCode(500, ApiResponse<IEnumerable<TopAssister>>.ErrorResponse("Failed to get top assisters"));
        }
    }

    /// <summary>
    /// Get player statistics for a specific tournament
    /// </summary>
    [HttpGet("player/{playerId}/tournament/{tournamentId}")]
    public async Task<ActionResult<ApiResponse<PlayerStatistics>>> GetPlayerStats(int playerId, int tournamentId)
    {
        try
        {
            var stats = await _statsRepository.GetPlayerStatsAsync(playerId, tournamentId);
            if (stats == null)
                return NotFound(ApiResponse<PlayerStatistics>.ErrorResponse("Player stats not found"));

            return Ok(ApiResponse<PlayerStatistics>.SuccessResponse(stats));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting stats for player {PlayerId}", playerId);
            return StatusCode(500, ApiResponse<PlayerStatistics>.ErrorResponse("Failed to get player stats"));
        }
    }

    /// <summary>
    /// Get team statistics for a tournament
    /// </summary>
    [HttpGet("tournament/{tournamentId}/teams")]
    public async Task<ActionResult<ApiResponse<IEnumerable<TeamStatistics>>>> GetTeamStatistics(int tournamentId)
    {
        try
        {
            var stats = await _statsRepository.GetTeamStatisticsAsync(tournamentId);
            return Ok(ApiResponse<IEnumerable<TeamStatistics>>.SuccessResponse(stats));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting team statistics for tournament {TournamentId}", tournamentId);
            return StatusCode(500, ApiResponse<IEnumerable<TeamStatistics>>.ErrorResponse("Failed to get team statistics"));
        }
    }

    /// <summary>
    /// Get tournament summary statistics
    /// </summary>
    [HttpGet("tournament/{tournamentId}/summary")]
    public async Task<ActionResult<ApiResponse<TournamentSummary>>> GetTournamentSummary(int tournamentId)
    {
        try
        {
            var summary = await _statsRepository.GetTournamentSummaryAsync(tournamentId);
            if (summary == null)
                return NotFound(ApiResponse<TournamentSummary>.ErrorResponse("Tournament not found"));

            return Ok(ApiResponse<TournamentSummary>.SuccessResponse(summary));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tournament summary for {TournamentId}", tournamentId);
            return StatusCode(500, ApiResponse<TournamentSummary>.ErrorResponse("Failed to get tournament summary"));
        }
    }

    /// <summary>
    /// Get goals distribution by time period
    /// </summary>
    [HttpGet("tournament/{tournamentId}/goals-by-time")]
    public async Task<ActionResult<ApiResponse<IEnumerable<GoalsByTime>>>> GetGoalsByTime(int tournamentId)
    {
        try
        {
            var goalsByTime = await _statsRepository.GetGoalsByTimeAsync(tournamentId);
            return Ok(ApiResponse<IEnumerable<GoalsByTime>>.SuccessResponse(goalsByTime));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting goals by time for tournament {TournamentId}", tournamentId);
            return StatusCode(500, ApiResponse<IEnumerable<GoalsByTime>>.ErrorResponse("Failed to get goals by time"));
        }
    }

    /// <summary>
    /// Get goals by match day
    /// </summary>
    [HttpGet("tournament/{tournamentId}/goals-by-match-day")]
    public async Task<ActionResult<ApiResponse<IEnumerable<GoalsByMatchDay>>>> GetGoalsByMatchDay(int tournamentId)
    {
        try
        {
            var goalsByDay = await _statsRepository.GetGoalsByMatchDayAsync(tournamentId);
            return Ok(ApiResponse<IEnumerable<GoalsByMatchDay>>.SuccessResponse(goalsByDay));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting goals by match day for tournament {TournamentId}", tournamentId);
            return StatusCode(500, ApiResponse<IEnumerable<GoalsByMatchDay>>.ErrorResponse("Failed to get goals by match day"));
        }
    }

    /// <summary>
    /// Get most carded players
    /// </summary>
    [HttpGet("tournament/{tournamentId}/most-carded")]
    public async Task<ActionResult<ApiResponse<IEnumerable<CardedPlayer>>>> GetMostCardedPlayers(
        int tournamentId, [FromQuery] int limit = 10)
    {
        try
        {
            var players = await _statsRepository.GetMostCardedPlayersAsync(tournamentId, limit);
            return Ok(ApiResponse<IEnumerable<CardedPlayer>>.SuccessResponse(players));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting most carded players for tournament {TournamentId}", tournamentId);
            return StatusCode(500, ApiResponse<IEnumerable<CardedPlayer>>.ErrorResponse("Failed to get most carded players"));
        }
    }

    /// <summary>
    /// Get man of the match history
    /// </summary>
    [HttpGet("tournament/{tournamentId}/man-of-the-match")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ManOfTheMatch>>>> GetManOfTheMatchList(int tournamentId)
    {
        try
        {
            var momList = await _statsRepository.GetManOfTheMatchListAsync(tournamentId);
            return Ok(ApiResponse<IEnumerable<ManOfTheMatch>>.SuccessResponse(momList));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting man of the match list for tournament {TournamentId}", tournamentId);
            return StatusCode(500, ApiResponse<IEnumerable<ManOfTheMatch>>.ErrorResponse("Failed to get man of the match list"));
        }
    }

    /// <summary>
    /// Get player statistics for a specific match (includes goals, assists, cards, MoM status)
    /// </summary>
    [HttpGet("match/{matchId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<PlayerMatchStatsResponse>>>> GetMatchStats(int matchId)
    {
        try
        {
            var stats = await _statsRepository.GetMatchStatsAsync(matchId);
            return Ok(ApiResponse<IEnumerable<PlayerMatchStatsResponse>>.SuccessResponse(stats));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting player stats for match {MatchId}", matchId);
            return StatusCode(500, ApiResponse<IEnumerable<PlayerMatchStatsResponse>>.ErrorResponse("Failed to get match stats"));
        }
    }

    /// <summary>
    /// Record a player action (goal, assist, card, etc.) - Generic action endpoint
    /// </summary>
    [HttpPost("action")]
    [Authorize(Policy = "OfficialAccess")]
    public async Task<ActionResult<ApiResponse<bool>>> RecordPlayerAction([FromBody] RecordPlayerActionRequest request)
    {
        try
        {
            // Validate match exists
            var match = await _matchRepository.GetByIdAsync(request.MatchId);
            if (match == null)
                return NotFound(ApiResponse<bool>.ErrorResponse("Match not found"));

            // Convert action type to stats
            int goals = 0, assists = 0, yellowCards = 0, redCards = 0;

            switch (request.ActionType?.ToLower())
            {
                case "goal":
                    goals = 1;
                    break;
                case "assist":
                    assists = 1;
                    break;
                case "yellowcard":
                case "yellow_card":
                    yellowCards = 1;
                    break;
                case "redcard":
                case "red_card":
                    redCards = 1;
                    break;
                case "manofthematch":
                case "man_of_the_match":
                case "mom":
                    // Handle MoM separately
                    break;
                default:
                    return BadRequest(ApiResponse<bool>.ErrorResponse($"Unknown action type: {request.ActionType}"));
            }

            await _statsRepository.RecordPlayerMatchStatsAsync(
                request.MatchId,
                request.PlayerId,
                request.TeamId,
                goals,
                assists,
                yellowCards,
                redCards,
                request.MinutesPlayed ?? 0,
                request.ActionType?.ToLower() == "manofthematch" || request.ActionType?.ToLower() == "mom",
                GetUserId(),
                GetUsername(),
                GetIpAddress()
            );

            return Ok(ApiResponse<bool>.SuccessResponse(true, $"{request.ActionType} recorded successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording player action for player {PlayerId} in match {MatchId}", 
                request.PlayerId, request.MatchId);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Failed to record player action"));
        }
    }

    /// <summary>
    /// Record a goal - Shortcut endpoint
    /// </summary>
    [HttpPost("goal")]
    [Authorize(Policy = "OfficialAccess")]
    public async Task<ActionResult<ApiResponse<bool>>> RecordGoal([FromBody] RecordGoalRequest request)
    {
        try
        {
            var match = await _matchRepository.GetByIdAsync(request.MatchId);
            if (match == null)
                return NotFound(ApiResponse<bool>.ErrorResponse("Match not found"));

            // Record the goal for scorer
            await _statsRepository.RecordPlayerMatchStatsAsync(
                request.MatchId,
                request.PlayerId,
                request.TeamId,
                goals: 1,
                assists: 0,
                yellowCards: 0,
                redCards: 0,
                minutesPlayed: request.Minute ?? 0,
                isManOfTheMatch: false,
                GetUserId(),
                GetUsername(),
                GetIpAddress()
            );

            // Record assist if provided
            if (request.AssistPlayerId.HasValue)
            {
                await _statsRepository.RecordPlayerMatchStatsAsync(
                    request.MatchId,
                    request.AssistPlayerId.Value,
                    request.TeamId,
                    goals: 0,
                    assists: 1,
                    yellowCards: 0,
                    redCards: 0,
                    minutesPlayed: request.Minute ?? 0,
                    isManOfTheMatch: false,
                    GetUserId(),
                    GetUsername(),
                    GetIpAddress()
                );
            }

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Goal recorded successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording goal for player {PlayerId} in match {MatchId}", 
                request.PlayerId, request.MatchId);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Failed to record goal"));
        }
    }

    /// <summary>
    /// Record an assist - Shortcut endpoint
    /// </summary>
    [HttpPost("assist")]
    [Authorize(Policy = "OfficialAccess")]
    public async Task<ActionResult<ApiResponse<bool>>> RecordAssist([FromBody] RecordAssistRequest request)
    {
        try
        {
            var match = await _matchRepository.GetByIdAsync(request.MatchId);
            if (match == null)
                return NotFound(ApiResponse<bool>.ErrorResponse("Match not found"));

            await _statsRepository.RecordPlayerMatchStatsAsync(
                request.MatchId,
                request.PlayerId,
                request.TeamId,
                goals: 0,
                assists: 1,
                yellowCards: 0,
                redCards: 0,
                minutesPlayed: request.Minute ?? 0,
                isManOfTheMatch: false,
                GetUserId(),
                GetUsername(),
                GetIpAddress()
            );

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Assist recorded successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording assist for player {PlayerId} in match {MatchId}", 
                request.PlayerId, request.MatchId);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Failed to record assist"));
        }
    }

    /// <summary>
    /// Record a yellow card
    /// </summary>
    [HttpPost("yellow-card")]
    [Authorize(Policy = "OfficialAccess")]
    public async Task<ActionResult<ApiResponse<bool>>> RecordYellowCard([FromBody] RecordCardRequest request)
    {
        try
        {
            var match = await _matchRepository.GetByIdAsync(request.MatchId);
            if (match == null)
                return NotFound(ApiResponse<bool>.ErrorResponse("Match not found"));

            await _statsRepository.RecordPlayerMatchStatsAsync(
                request.MatchId,
                request.PlayerId,
                request.TeamId,
                goals: 0,
                assists: 0,
                yellowCards: 1,
                redCards: 0,
                minutesPlayed: request.Minute ?? 0,
                isManOfTheMatch: false,
                GetUserId(),
                GetUsername(),
                GetIpAddress()
            );

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Yellow card recorded successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording yellow card for player {PlayerId} in match {MatchId}", 
                request.PlayerId, request.MatchId);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Failed to record yellow card"));
        }
    }

    /// <summary>
    /// Record a red card
    /// </summary>
    [HttpPost("red-card")]
    [Authorize(Policy = "OfficialAccess")]
    public async Task<ActionResult<ApiResponse<bool>>> RecordRedCard([FromBody] RecordCardRequest request)
    {
        try
        {
            var match = await _matchRepository.GetByIdAsync(request.MatchId);
            if (match == null)
                return NotFound(ApiResponse<bool>.ErrorResponse("Match not found"));

            await _statsRepository.RecordPlayerMatchStatsAsync(
                request.MatchId,
                request.PlayerId,
                request.TeamId,
                goals: 0,
                assists: 0,
                yellowCards: 0,
                redCards: 1,
                minutesPlayed: request.Minute ?? 0,
                isManOfTheMatch: false,
                GetUserId(),
                GetUsername(),
                GetIpAddress()
            );

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Red card recorded successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording red card for player {PlayerId} in match {MatchId}", 
                request.PlayerId, request.MatchId);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Failed to record red card"));
        }
    }

    /// <summary>
    /// Set man of the match
    /// </summary>
    [HttpPost("man-of-the-match")]
    [Authorize(Policy = "OfficialAccess")]
    public async Task<ActionResult<ApiResponse<bool>>> SetManOfTheMatch([FromBody] SetManOfTheMatchRequest request)
    {
        return await SetManOfTheMatchInternal(request);
    }

    /// <summary>
    /// Set man of the match (alias endpoint)
    /// </summary>
    [HttpPost("mom")]
    [Authorize(Policy = "OfficialAccess")]
    public async Task<ActionResult<ApiResponse<bool>>> SetManOfTheMatchAlias([FromBody] SetManOfTheMatchRequest request)
    {
        return await SetManOfTheMatchInternal(request);
    }

    private async Task<ActionResult<ApiResponse<bool>>> SetManOfTheMatchInternal(SetManOfTheMatchRequest request)
    {
        try
        {
            var match = await _matchRepository.GetByIdAsync(request.MatchId);
            if (match == null)
                return NotFound(ApiResponse<bool>.ErrorResponse("Match not found"));

            await _statsRepository.SetManOfTheMatchAsync(
                request.MatchId,
                request.PlayerId,
                request.TeamId,
                GetUserId(),
                GetUsername(),
                GetIpAddress()
            );

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Man of the Match set successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting man of the match for player {PlayerId} in match {MatchId}", 
                request.PlayerId, request.MatchId);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Failed to set man of the match"));
        }
    }

    /// <summary>
    /// Record full player match stats (for bulk updates)
    /// </summary>
    [HttpPost("match-stats")]
    [Authorize(Policy = "OfficialAccess")]
    public async Task<ActionResult<ApiResponse<bool>>> RecordMatchStats([FromBody] RecordMatchStatsRequest request)
    {
        try
        {
            var match = await _matchRepository.GetByIdAsync(request.MatchId);
            if (match == null)
                return NotFound(ApiResponse<bool>.ErrorResponse("Match not found"));

            await _statsRepository.RecordPlayerMatchStatsAsync(
                request.MatchId,
                request.PlayerId,
                request.TeamId,
                request.Goals,
                request.Assists,
                request.YellowCards,
                request.RedCards,
                request.MinutesPlayed,
                request.IsManOfTheMatch,
                GetUserId(),
                GetUsername(),
                GetIpAddress()
            );

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Player match stats recorded successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording match stats for player {PlayerId} in match {MatchId}", 
                request.PlayerId, request.MatchId);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Failed to record player match stats"));
        }
    }
}
