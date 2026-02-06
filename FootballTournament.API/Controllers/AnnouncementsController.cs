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
public class AnnouncementsController : ControllerBase
{
    private readonly IAnnouncementRepository _announcementRepository;

    public AnnouncementsController(IAnnouncementRepository announcementRepository)
    {
        _announcementRepository = announcementRepository;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<Announcement>>>> GetAll(
        [FromQuery] int? tournamentId = null, [FromQuery] int limit = 10)
    {
        var announcements = await _announcementRepository.GetAllAsync(tournamentId, limit);
        return Ok(ApiResponse<IEnumerable<Announcement>>.SuccessResponse(announcements));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<Announcement>>> GetById(int id)
    {
        var announcement = await _announcementRepository.GetByIdAsync(id);
        
        if (announcement == null)
            return NotFound(ApiResponse<Announcement>.FailResponse("Announcement not found"));

        return Ok(ApiResponse<Announcement>.SuccessResponse(announcement));
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<int>>> Create([FromBody] CreateAnnouncementRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<int>.FailResponse("Invalid request",
                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

        var userId = GetCurrentUserId();
        var username = User.Identity?.Name;
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        var announcementId = await _announcementRepository.CreateAsync(
            request.Title, request.Content, request.TournamentId, request.AnnouncementType,
            request.TargetAudience, request.IsPinned, request.ExpiryDate, userId, username, ipAddress
        );

        return Ok(ApiResponse<int>.SuccessResponse(announcementId, "Announcement created successfully"));
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpPut]
    public async Task<ActionResult<ApiResponse<bool>>> Update([FromBody] UpdateAnnouncementRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<bool>.FailResponse("Invalid request",
                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

        var userId = GetCurrentUserId();
        var username = User.Identity?.Name;
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        await _announcementRepository.UpdateAsync(
            request.AnnouncementId, request.Title, request.Content, request.AnnouncementType,
            request.TargetAudience, request.IsPinned, request.ExpiryDate, userId, username, ipAddress
        );

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Announcement updated successfully"));
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        var userId = GetCurrentUserId();
        var username = User.Identity?.Name;
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        await _announcementRepository.DeleteAsync(id, userId, username, ipAddress);
        return Ok(ApiResponse<bool>.SuccessResponse(true, "Announcement deleted successfully"));
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}
