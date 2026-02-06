using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FootballTournament.API.Models.DTOs;
using FootballTournament.API.Models.Entities;
using FootballTournament.API.Repositories;
using System.Security.Claims;
using FootballTournament.API.Common;

namespace FootballTournament.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CFAAwardsController : ControllerBase
{
    private readonly ICFAAwardsRepository _awardsRepository;
    private readonly ITournamentRepository _tournamentRepository;
    private readonly ILogger<CFAAwardsController> _logger;

    public CFAAwardsController(
        ICFAAwardsRepository awardsRepository,
        ITournamentRepository tournamentRepository,
        ILogger<CFAAwardsController> logger)
    {
        _awardsRepository = awardsRepository;
        _tournamentRepository = tournamentRepository;
        _logger = logger;
    }

    private int? GetUserId() => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
    private string? GetUsername() => User.FindFirstValue(ClaimTypes.Name);
    private string? GetIpAddress() => HttpContext.Connection.RemoteIpAddress?.ToString();

    #region Golden Glove (Goalkeeper Stats)

    /// <summary>
    /// Record goalkeeper statistics for a match
    /// </summary>
    [HttpPost("goalkeeper-stats")]
    [Authorize(Roles = "Admin,Official")]
    public async Task<ActionResult<ApiResponse<int>>> RecordGoalkeeperStats([FromBody] RecordGoalkeeperStatsRequest request)
    {
        try
        {
            // Get tournament ID from match
            var tournament = await _tournamentRepository.GetActiveAsync();
            if (tournament == null)
                return BadRequest(ApiResponse<int>.Fail("No active tournament found"));

            var id = await _awardsRepository.RecordGoalkeeperStatsAsync(
                request.MatchId,
                request.PlayerId,
                request.TeamId,
                tournament.TournamentId,
                request.Saves,
                request.GoalsConceded,
                request.PenaltiesSaved,
                request.PenaltiesFaced,
                request.MinutesPlayed,
                GetUserId(),
                GetUsername(),
                GetIpAddress());

            return Ok(ApiResponse<int>.Ok(id, "Goalkeeper stats recorded successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording goalkeeper stats");
            return StatusCode(500, ApiResponse<int>.Fail("Error recording goalkeeper stats"));
        }
    }

    /// <summary>
    /// Update existing goalkeeper statistics
    /// </summary>
    [HttpPut("goalkeeper-stats")]
    [Authorize(Roles = "Admin,Official")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateGoalkeeperStats([FromBody] UpdateGoalkeeperStatsRequest request)
    {
        try
        {
            await _awardsRepository.UpdateGoalkeeperStatsAsync(
                request.GoalkeeperStatsId,
                request.Saves,
                request.GoalsConceded,
                request.PenaltiesSaved,
                request.PenaltiesFaced,
                request.MinutesPlayed,
                GetUserId(),
                GetUsername(),
                GetIpAddress());

            return Ok(ApiResponse<bool>.Ok(true, "Goalkeeper stats updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating goalkeeper stats");
            return StatusCode(500, ApiResponse<bool>.Fail("Error updating goalkeeper stats"));
        }
    }

    /// <summary>
    /// Get goalkeeper stats for a specific match
    /// </summary>
    [HttpGet("goalkeeper-stats/match/{matchId}/player/{playerId}")]
    public async Task<ActionResult<ApiResponse<GoalkeeperMatchStats>>> GetGoalkeeperStatsByMatch(int matchId, int playerId)
    {
        try
        {
            var stats = await _awardsRepository.GetGoalkeeperStatsByMatchAsync(matchId, playerId);
            if (stats == null)
                return NotFound(ApiResponse<GoalkeeperMatchStats>.Fail("Goalkeeper stats not found"));

            return Ok(ApiResponse<GoalkeeperMatchStats>.Ok(stats));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting goalkeeper stats");
            return StatusCode(500, ApiResponse<GoalkeeperMatchStats>.Fail("Error getting goalkeeper stats"));
        }
    }

    /// <summary>
    /// Get all goalkeeper stats for a player in a tournament
    /// </summary>
    [HttpGet("goalkeeper-stats/tournament/{tournamentId}/player/{playerId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<GoalkeeperMatchStats>>>> GetGoalkeeperStatsByTournament(int tournamentId, int playerId)
    {
        try
        {
            var stats = await _awardsRepository.GetGoalkeeperStatsByTournamentAsync(tournamentId, playerId);
            return Ok(ApiResponse<IEnumerable<GoalkeeperMatchStats>>.Ok(stats));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting goalkeeper stats");
            return StatusCode(500, ApiResponse<IEnumerable<GoalkeeperMatchStats>>.Fail("Error getting goalkeeper stats"));
        }
    }

    /// <summary>
    /// Get Golden Glove leaderboard
    /// </summary>
    [HttpGet("golden-glove/{tournamentId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<GoldenGloveStanding>>>> GetGoldenGloveStandings(
        int tournamentId, 
        [FromQuery] int limit = 10,
        [FromQuery] bool eligibleOnly = false)
    {
        try
        {
            var standings = await _awardsRepository.GetGoldenGloveStandingsAsync(tournamentId, limit, eligibleOnly);
            return Ok(ApiResponse<IEnumerable<GoldenGloveStanding>>.Ok(standings));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Golden Glove standings");
            return StatusCode(500, ApiResponse<IEnumerable<GoldenGloveStanding>>.Fail("Error getting Golden Glove standings"));
        }
    }

    #endregion

    #region Best Defender (Defender Ratings)

    /// <summary>
    /// Record defender rating for a match
    /// </summary>
    [HttpPost("defender-rating")]
    [Authorize(Roles = "Admin,Official")]
    public async Task<ActionResult<ApiResponse<int>>> RecordDefenderRating([FromBody] RecordDefenderRatingRequest request)
    {
        try
        {
            var tournament = await _tournamentRepository.GetActiveAsync();
            if (tournament == null)
                return BadRequest(ApiResponse<int>.Fail("No active tournament found"));

            var id = await _awardsRepository.RecordDefenderRatingAsync(
                request.MatchId,
                request.PlayerId,
                request.TeamId,
                tournament.TournamentId,
                request.RatingPosition,
                request.TacklingScore,
                request.InterceptionScore,
                request.MarkingScore,
                request.BlockingScore,
                request.Notes,
                GetUserId(),
                GetUsername(),
                GetIpAddress());

            return Ok(ApiResponse<int>.Ok(id, "Defender rating recorded successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording defender rating");
            return StatusCode(500, ApiResponse<int>.Fail("Error recording defender rating"));
        }
    }

    /// <summary>
    /// Submit both top defenders for a match at once
    /// </summary>
    [HttpPost("defender-ratings/match")]
    [Authorize(Roles = "Admin,Official")]
    public async Task<ActionResult<ApiResponse<bool>>> SubmitMatchDefenderRatings([FromBody] SubmitMatchDefenderRatingsRequest request)
    {
        try
        {
            var tournament = await _tournamentRepository.GetActiveAsync();
            if (tournament == null)
                return BadRequest(ApiResponse<bool>.Fail("No active tournament found"));

            // Delete existing ratings for this match
            await _awardsRepository.DeleteDefenderRatingsByMatchAsync(
                request.MatchId, GetUserId(), GetUsername(), GetIpAddress());

            // Add first place (2 points)
            await _awardsRepository.RecordDefenderRatingAsync(
                request.MatchId,
                request.FirstPlace.PlayerId,
                request.FirstPlace.TeamId,
                tournament.TournamentId,
                1, // First place
                request.FirstPlace.TacklingScore,
                request.FirstPlace.InterceptionScore,
                request.FirstPlace.MarkingScore,
                request.FirstPlace.BlockingScore,
                request.FirstPlace.Notes,
                GetUserId(),
                GetUsername(),
                GetIpAddress());

            // Add second place (1 point)
            await _awardsRepository.RecordDefenderRatingAsync(
                request.MatchId,
                request.SecondPlace.PlayerId,
                request.SecondPlace.TeamId,
                tournament.TournamentId,
                2, // Second place
                request.SecondPlace.TacklingScore,
                request.SecondPlace.InterceptionScore,
                request.SecondPlace.MarkingScore,
                request.SecondPlace.BlockingScore,
                request.SecondPlace.Notes,
                GetUserId(),
                GetUsername(),
                GetIpAddress());

            return Ok(ApiResponse<bool>.Ok(true, "Match defender ratings submitted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting match defender ratings");
            return StatusCode(500, ApiResponse<bool>.Fail("Error submitting match defender ratings"));
        }
    }

    /// <summary>
    /// Get defender ratings for a match
    /// </summary>
    [HttpGet("defender-ratings/match/{matchId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<DefenderMatchRating>>>> GetDefenderRatingsByMatch(int matchId)
    {
        try
        {
            var ratings = await _awardsRepository.GetDefenderRatingsByMatchAsync(matchId);
            return Ok(ApiResponse<IEnumerable<DefenderMatchRating>>.Ok(ratings));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting defender ratings");
            return StatusCode(500, ApiResponse<IEnumerable<DefenderMatchRating>>.Fail("Error getting defender ratings"));
        }
    }

    /// <summary>
    /// Get defender ratings history for a player
    /// </summary>
    [HttpGet("defender-ratings/player/{playerId}/tournament/{tournamentId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<DefenderMatchRating>>>> GetDefenderRatingsByPlayer(int playerId, int tournamentId)
    {
        try
        {
            var ratings = await _awardsRepository.GetDefenderRatingsByPlayerAsync(playerId, tournamentId);
            return Ok(ApiResponse<IEnumerable<DefenderMatchRating>>.Ok(ratings));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting defender ratings");
            return StatusCode(500, ApiResponse<IEnumerable<DefenderMatchRating>>.Fail("Error getting defender ratings"));
        }
    }

    /// <summary>
    /// Get Best Defender leaderboard
    /// </summary>
    [HttpGet("best-defender/{tournamentId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<BestDefenderStanding>>>> GetBestDefenderStandings(
        int tournamentId,
        [FromQuery] int limit = 10)
    {
        try
        {
            var standings = await _awardsRepository.GetBestDefenderStandingsAsync(tournamentId, limit);
            return Ok(ApiResponse<IEnumerable<BestDefenderStanding>>.Ok(standings));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Best Defender standings");
            return StatusCode(500, ApiResponse<IEnumerable<BestDefenderStanding>>.Fail("Error getting Best Defender standings"));
        }
    }

    #endregion

    #region Golden Boot

    /// <summary>
    /// Get Golden Boot leaderboard (excludes shootout penalties)
    /// </summary>
    [HttpGet("golden-boot/{tournamentId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<GoldenBootStanding>>>> GetGoldenBootStandings(
        int tournamentId,
        [FromQuery] int limit = 10,
        [FromQuery] string? category = null)
    {
        try
        {
            var standings = await _awardsRepository.GetGoldenBootStandingsAsync(tournamentId, limit, category);
            return Ok(ApiResponse<IEnumerable<GoldenBootStanding>>.Ok(standings));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Golden Boot standings");
            return StatusCode(500, ApiResponse<IEnumerable<GoldenBootStanding>>.Fail("Error getting Golden Boot standings"));
        }
    }

    #endregion

    #region Airline Ticket (Award Summary)

    /// <summary>
    /// Get combined award summary for Airline Ticket calculation (Senior category only)
    /// </summary>
    [HttpGet("airline-ticket/{tournamentId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<AwardSummary>>>> GetAirlineTicketStandings(
        int tournamentId,
        [FromQuery] int limit = 10)
    {
        try
        {
            var standings = await _awardsRepository.GetAwardSummaryAsync(tournamentId, limit);
            return Ok(ApiResponse<IEnumerable<AwardSummary>>.Ok(standings));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Airline Ticket standings");
            return StatusCode(500, ApiResponse<IEnumerable<AwardSummary>>.Fail("Error getting Airline Ticket standings"));
        }
    }

    #endregion

    #region Walkover

    /// <summary>
    /// Declare a match as walkover (team absent)
    /// </summary>
    [HttpPost("walkover")]
    [Authorize(Roles = "Admin,Official")]
    public async Task<ActionResult<ApiResponse<bool>>> DeclareWalkover([FromBody] DeclareWalkoverRequest request)
    {
        try
        {
            await _awardsRepository.DeclareWalkoverAsync(
                request.MatchId,
                request.WinnerTeamId,
                request.AbsentTeamId,
                request.Reason,
                request.DelayMinutes,
                GetUserId(),
                GetUsername(),
                GetIpAddress());

            return Ok(ApiResponse<bool>.Ok(true, "Walkover declared successfully. Score set to 3-0."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error declaring walkover");
            return StatusCode(500, ApiResponse<bool>.Fail("Error declaring walkover"));
        }
    }

    /// <summary>
    /// Get walkover details for a match
    /// </summary>
    [HttpGet("walkover/{matchId}")]
    public async Task<ActionResult<ApiResponse<WalkoverResult>>> GetWalkoverDetails(int matchId)
    {
        try
        {
            var details = await _awardsRepository.GetWalkoverDetailsAsync(matchId);
            if (details == null)
                return NotFound(ApiResponse<WalkoverResult>.Fail("Walkover details not found"));

            return Ok(ApiResponse<WalkoverResult>.Ok(details));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting walkover details");
            return StatusCode(500, ApiResponse<WalkoverResult>.Fail("Error getting walkover details"));
        }
    }

    #endregion

    #region All Awards Summary

    /// <summary>
    /// Get all award standings in one call
    /// </summary>
    [HttpGet("all/{tournamentId}")]
    public async Task<ActionResult<ApiResponse<AllAwardsResponse>>> GetAllAwards(
        int tournamentId,
        [FromQuery] int limit = 10)
    {
        try
        {
            var goldenBoot = await _awardsRepository.GetGoldenBootStandingsAsync(tournamentId, limit);
            var goldenGlove = await _awardsRepository.GetGoldenGloveStandingsAsync(tournamentId, limit, false);
            var bestDefender = await _awardsRepository.GetBestDefenderStandingsAsync(tournamentId, limit);
            var airlineTicket = await _awardsRepository.GetAwardSummaryAsync(tournamentId, limit);

            var response = new AllAwardsResponse
            {
                GoldenBoot = goldenBoot.ToList(),
                GoldenGlove = goldenGlove.ToList(),
                BestDefender = bestDefender.ToList(),
                AirlineTicket = airlineTicket.ToList()
            };

            return Ok(ApiResponse<AllAwardsResponse>.Ok(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all awards");
            return StatusCode(500, ApiResponse<AllAwardsResponse>.Fail("Error getting all awards"));
        }
    }

    #endregion
}

/// <summary>
/// Combined response for all award standings
/// </summary>
public class AllAwardsResponse
{
    public List<GoldenBootStanding> GoldenBoot { get; set; } = new();
    public List<GoldenGloveStanding> GoldenGlove { get; set; } = new();
    public List<BestDefenderStanding> BestDefender { get; set; } = new();
    public List<AwardSummary> AirlineTicket { get; set; } = new();
}
