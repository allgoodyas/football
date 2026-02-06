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
public class PlayersController : ControllerBase
{
    private readonly IPlayerRepository _playerRepository;

    public PlayersController(IPlayerRepository playerRepository)
    {
        _playerRepository = playerRepository;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<Player>>>> GetAll()
    {
        var players = await _playerRepository.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<Player>>.SuccessResponse(players));
    }

    [HttpGet("tournament/{tournamentId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<Player>>>> GetByTournament(int tournamentId)
    {
        var players = await _playerRepository.GetAllByTournamentAsync(tournamentId);
        return Ok(ApiResponse<IEnumerable<Player>>.SuccessResponse(players));
    }

    [HttpGet("team/{teamId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<Player>>>> GetByTeam(int teamId)
    {
        var players = await _playerRepository.GetAllByTeamAsync(teamId);
        return Ok(ApiResponse<IEnumerable<Player>>.SuccessResponse(players));
    }

    [HttpGet("unassigned/{tournamentId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<Player>>>> GetUnassigned(int tournamentId)
    {
        var players = await _playerRepository.GetUnassignedAsync(tournamentId);
        return Ok(ApiResponse<IEnumerable<Player>>.SuccessResponse(players));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<Player>>> GetById(int id)
    {
        var player = await _playerRepository.GetByIdAsync(id);
        
        if (player == null)
            return NotFound(ApiResponse<Player>.FailResponse("Player not found"));

        return Ok(ApiResponse<Player>.SuccessResponse(player));
    }

    [HttpGet("search")]
    public async Task<ActionResult<ApiResponse<IEnumerable<Player>>>> Search(
        [FromQuery] string searchTerm, [FromQuery] int? tournamentId = null)
    {
        var players = await _playerRepository.SearchAsync(searchTerm, tournamentId);
        return Ok(ApiResponse<IEnumerable<Player>>.SuccessResponse(players));
    }

    [HttpGet("{playerId}/transfers")]
    public async Task<ActionResult<ApiResponse<IEnumerable<PlayerTransfer>>>> GetTransferHistory(int playerId)
    {
        var transfers = await _playerRepository.GetTransferHistoryAsync(playerId);
        return Ok(ApiResponse<IEnumerable<PlayerTransfer>>.SuccessResponse(transfers));
    }

    [HttpGet("top-scorers/{tournamentId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<PlayerStats>>>> GetTopScorers(int tournamentId, [FromQuery] int limit = 10)
    {
        var scorers = await _playerRepository.GetTopScorersAsync(tournamentId, limit);
        return Ok(ApiResponse<IEnumerable<PlayerStats>>.SuccessResponse(scorers));
    }

    [HttpGet("top-assists/{tournamentId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<PlayerStats>>>> GetTopAssists(int tournamentId, [FromQuery] int limit = 10)
    {
        var assists = await _playerRepository.GetTopAssistsAsync(tournamentId, limit);
        return Ok(ApiResponse<IEnumerable<PlayerStats>>.SuccessResponse(assists));
    }

    [HttpGet("stats/{tournamentId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<PlayerStats>>>> GetPlayerStats(int tournamentId)
    {
        var stats = await _playerRepository.GetPlayerStatsAsync(tournamentId);
        return Ok(ApiResponse<IEnumerable<PlayerStats>>.SuccessResponse(stats));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<int>>> Create([FromBody] CreatePlayerRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<int>.FailResponse("Invalid request",
                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

        // Get the full name (handles both playerName and firstName/lastName)
        var fullName = request.GetFullName();
        if (string.IsNullOrWhiteSpace(fullName))
            return BadRequest(ApiResponse<int>.FailResponse("Player name is required"));

        // Get class name (handles both className and grade)
        var className = request.GetClassName();

        // Check jersey number availability if team and jersey number specified
        if (request.TeamId.HasValue && request.JerseyNumber.HasValue)
        {
            var isAvailable = await _playerRepository.IsJerseyNumberAvailableAsync(
                request.TeamId.Value, request.JerseyNumber.Value);
            
            if (!isAvailable)
                return BadRequest(ApiResponse<int>.FailResponse("Jersey number already taken in this team"));
        }

        var userId = GetCurrentUserId();
        var username = User.Identity?.Name;
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        var playerId = await _playerRepository.CreateAsync(
            request.TournamentId, request.TeamId, fullName,
            request.DateOfBirth, request.JerseyNumber, request.Position, className,
            request.ContactEmail, request.ContactPhone, request.GuardianName, request.GuardianContact,
            request.InstitutionId, request.IsCaptain, userId, username, ipAddress
        );

        return Ok(ApiResponse<int>.SuccessResponse(playerId, "Player created successfully"));
    }

    [HttpPut]
    public async Task<ActionResult<ApiResponse<bool>>> Update([FromBody] UpdatePlayerRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<bool>.FailResponse("Invalid request",
                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

        // Get the full name (handles both playerName and firstName/lastName)
        var fullName = request.GetFullName();
        if (string.IsNullOrWhiteSpace(fullName))
            return BadRequest(ApiResponse<bool>.FailResponse("Player name is required"));

        // Get class name (handles both className and grade)
        var className = request.GetClassName();

        // Check jersey number availability if team and jersey number specified
        if (request.TeamId.HasValue && request.JerseyNumber.HasValue)
        {
            var isAvailable = await _playerRepository.IsJerseyNumberAvailableAsync(
                request.TeamId.Value, request.JerseyNumber.Value, request.PlayerId);
            
            if (!isAvailable)
                return BadRequest(ApiResponse<bool>.FailResponse("Jersey number already taken in this team"));
        }

        var userId = GetCurrentUserId();
        var username = User.Identity?.Name;
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        await _playerRepository.UpdateAsync(
            request.PlayerId, request.TeamId, fullName,
            request.DateOfBirth, request.JerseyNumber, request.Position, className,
            request.ContactEmail, request.ContactPhone, request.GuardianName, request.GuardianContact,
            request.InstitutionId, request.IsCaptain, request.IsActive, userId, username, ipAddress
        );

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Player updated successfully"));
    }

    [HttpPost("transfer")]
    public async Task<ActionResult<ApiResponse<bool>>> Transfer([FromBody] TransferPlayerRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<bool>.FailResponse("Invalid request",
                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

        var userId = GetCurrentUserId();
        var username = User.Identity?.Name;
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        var targetTeamId = request.GetTargetTeamId();

        await _playerRepository.TransferPlayerAsync(
            request.PlayerId, targetTeamId, request.Reason, userId, username, ipAddress
        );

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Player transferred successfully"));
    }

    [HttpPost("bulk-assign")]
    public async Task<ActionResult<ApiResponse<bool>>> BulkAssign([FromBody] BulkAssignPlayersRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<bool>.FailResponse("Invalid request",
                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

        var userId = GetCurrentUserId();
        var username = User.Identity?.Name;
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        await _playerRepository.BulkAssignAsync(request.TeamId, request.PlayerIds, userId, username, ipAddress);
        return Ok(ApiResponse<bool>.SuccessResponse(true, "Players assigned successfully"));
    }

    [HttpPost("set-captain")]
    [HttpPut("set-captain")]
    public async Task<ActionResult<ApiResponse<bool>>> SetCaptain([FromBody] SetCaptainRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<bool>.FailResponse("Invalid request",
                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

        var userId = GetCurrentUserId();
        var username = User.Identity?.Name;
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        await _playerRepository.SetCaptainAsync(request.TeamId, request.PlayerId, userId, username, ipAddress);
        return Ok(ApiResponse<bool>.SuccessResponse(true, "Captain set successfully"));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        var userId = GetCurrentUserId();
        var username = User.Identity?.Name;
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        await _playerRepository.DeleteAsync(id, userId, username, ipAddress);
        return Ok(ApiResponse<bool>.SuccessResponse(true, "Player deleted successfully"));
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}
