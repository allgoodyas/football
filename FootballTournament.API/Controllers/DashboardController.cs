using Microsoft.AspNetCore.Mvc;
using FootballTournament.API.Common;
using FootballTournament.API.Models.Entities;
using FootballTournament.API.Repositories;

namespace FootballTournament.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly ITournamentRepository _tournamentRepository;

    public DashboardController(ITournamentRepository tournamentRepository)
    {
        _tournamentRepository = tournamentRepository;
    }

    [HttpGet("tournament")]
    public async Task<ActionResult<ApiResponse<Tournament>>> GetActiveTournament()
    {
        var tournament = await _tournamentRepository.GetActiveAsync();
        
        if (tournament == null)
            return NotFound(ApiResponse<Tournament>.FailResponse("No active tournament found"));

        return Ok(ApiResponse<Tournament>.SuccessResponse(tournament));
    }

    [HttpGet("stats/{tournamentId}")]
    public async Task<ActionResult<ApiResponse<DashboardStats>>> GetStats(int tournamentId)
    {
        var stats = await _tournamentRepository.GetDashboardStatsAsync(tournamentId);
        
        if (stats == null)
            return NotFound(ApiResponse<DashboardStats>.FailResponse("Tournament not found"));

        return Ok(ApiResponse<DashboardStats>.SuccessResponse(stats));
    }

    [HttpGet("venues")]
    public async Task<ActionResult<ApiResponse<IEnumerable<Venue>>>> GetVenues()
    {
        var venues = await _tournamentRepository.GetVenuesAsync();
        return Ok(ApiResponse<IEnumerable<Venue>>.SuccessResponse(venues));
    }
}
