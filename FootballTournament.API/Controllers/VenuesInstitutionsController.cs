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
public class VenuesController : ControllerBase
{
    private readonly IVenueRepository _venueRepository;

    public VenuesController(IVenueRepository venueRepository)
    {
        _venueRepository = venueRepository;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<Venue>>>> GetAll()
    {
        var venues = await _venueRepository.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<Venue>>.SuccessResponse(venues));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<Venue>>> GetById(int id)
    {
        var venue = await _venueRepository.GetByIdAsync(id);
        
        if (venue == null)
            return NotFound(ApiResponse<Venue>.FailResponse("Venue not found"));

        return Ok(ApiResponse<Venue>.SuccessResponse(venue));
    }

    [HttpGet("stats")]
    public async Task<ActionResult<ApiResponse<VenueStats>>> GetStats()
    {
        var stats = await _venueRepository.GetStatsAsync();
        return Ok(ApiResponse<VenueStats>.SuccessResponse(stats));
    }

    [HttpGet("today-schedule")]
    public async Task<ActionResult<ApiResponse<IEnumerable<VenueSchedule>>>> GetTodaySchedule()
    {
        var schedule = await _venueRepository.GetTodayScheduleAsync();
        return Ok(ApiResponse<IEnumerable<VenueSchedule>>.SuccessResponse(schedule));
    }

    [HttpGet("facilities")]
    public async Task<ActionResult<ApiResponse<IEnumerable<FacilityStatus>>>> GetFacilityStatus()
    {
        var facilities = await _venueRepository.GetFacilityStatusAsync();
        return Ok(ApiResponse<IEnumerable<FacilityStatus>>.SuccessResponse(facilities));
    }

    [HttpGet("{venueId}/bookings")]
    public async Task<ActionResult<ApiResponse<IEnumerable<VenueBooking>>>> GetBookings(
        int venueId, [FromQuery] DateTime? date = null)
    {
        var bookings = await _venueRepository.GetBookingsAsync(venueId, date);
        return Ok(ApiResponse<IEnumerable<VenueBooking>>.SuccessResponse(bookings));
    }

    [HttpGet("{venueId}/available")]
    public async Task<ActionResult<ApiResponse<bool>>> CheckAvailability(
        int venueId, [FromQuery] DateTime date, [FromQuery] TimeSpan startTime, [FromQuery] TimeSpan endTime)
    {
        var isAvailable = await _venueRepository.IsAvailableAsync(venueId, date, startTime, endTime);
        return Ok(ApiResponse<bool>.SuccessResponse(isAvailable));
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<int>>> Create([FromBody] CreateVenueRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<int>.FailResponse("Invalid request",
                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

        var userId = GetCurrentUserId();
        var username = User.Identity?.Name;
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        var venueId = await _venueRepository.CreateAsync(
            request.VenueName, request.Location, request.Capacity, request.HasFloodlights,
            request.FieldSize, request.SurfaceType, request.Facilities, request.InstitutionId,
            userId, username, ipAddress
        );

        return Ok(ApiResponse<int>.SuccessResponse(venueId, "Venue created successfully"));
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpPut]
    public async Task<ActionResult<ApiResponse<bool>>> Update([FromBody] UpdateVenueRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<bool>.FailResponse("Invalid request",
                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

        var userId = GetCurrentUserId();
        var username = User.Identity?.Name;
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        await _venueRepository.UpdateAsync(
            request.VenueId, request.VenueName, request.Location, request.Capacity, request.HasFloodlights,
            request.FieldSize, request.SurfaceType, request.Facilities, request.InstitutionId,
            userId, username, ipAddress
        );

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Venue updated successfully"));
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpPut("status")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateStatus([FromBody] UpdateVenueStatusRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<bool>.FailResponse("Invalid request",
                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

        var userId = GetCurrentUserId();
        var username = User.Identity?.Name;
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        await _venueRepository.UpdateStatusAsync(request.VenueId, request.Status, userId, username, ipAddress);
        return Ok(ApiResponse<bool>.SuccessResponse(true, "Venue status updated successfully"));
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpPost("booking")]
    public async Task<ActionResult<ApiResponse<int>>> CreateBooking([FromBody] CreateVenueBookingRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<int>.FailResponse("Invalid request",
                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

        // Check availability
        var isAvailable = await _venueRepository.IsAvailableAsync(
            request.VenueId, request.BookingDate, request.StartTime, request.EndTime);
        
        if (!isAvailable)
            return BadRequest(ApiResponse<int>.FailResponse("Venue is not available for the specified time slot"));

        var userId = GetCurrentUserId();
        var username = User.Identity?.Name;
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        var bookingId = await _venueRepository.CreateBookingAsync(
            request.VenueId, request.MatchId, request.BookingDate, request.StartTime,
            request.EndTime, request.Purpose, userId, username, ipAddress
        );

        return Ok(ApiResponse<int>.SuccessResponse(bookingId, "Booking created successfully"));
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        var userId = GetCurrentUserId();
        var username = User.Identity?.Name;
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        await _venueRepository.DeleteAsync(id, userId, username, ipAddress);
        return Ok(ApiResponse<bool>.SuccessResponse(true, "Venue deleted successfully"));
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}

[ApiController]
[Route("api/[controller]")]
public class InstitutionsController : ControllerBase
{
    private readonly IInstitutionRepository _institutionRepository;

    public InstitutionsController(IInstitutionRepository institutionRepository)
    {
        _institutionRepository = institutionRepository;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<Institution>>>> GetAll()
    {
        var institutions = await _institutionRepository.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<Institution>>.SuccessResponse(institutions));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<Institution>>> GetById(int id)
    {
        var institution = await _institutionRepository.GetByIdAsync(id);
        
        if (institution == null)
            return NotFound(ApiResponse<Institution>.FailResponse("Institution not found"));

        return Ok(ApiResponse<Institution>.SuccessResponse(institution));
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<int>>> Create([FromBody] CreateInstitutionRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<int>.FailResponse("Invalid request",
                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

        var userId = GetCurrentUserId();
        var username = User.Identity?.Name;
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        var institutionId = await _institutionRepository.CreateAsync(
            request.InstitutionName, request.InstitutionCode, request.Address, request.City,
            request.Country, request.ContactEmail, request.ContactPhone, request.Website,
            userId, username, ipAddress
        );

        return Ok(ApiResponse<int>.SuccessResponse(institutionId, "Institution created successfully"));
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpPut]
    public async Task<ActionResult<ApiResponse<bool>>> Update([FromBody] UpdateInstitutionRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<bool>.FailResponse("Invalid request",
                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

        var userId = GetCurrentUserId();
        var username = User.Identity?.Name;
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        await _institutionRepository.UpdateAsync(
            request.InstitutionId, request.InstitutionName, request.InstitutionCode, request.Address,
            request.City, request.Country, request.ContactEmail, request.ContactPhone, request.Website,
            userId, username, ipAddress
        );

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Institution updated successfully"));
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        var userId = GetCurrentUserId();
        var username = User.Identity?.Name;
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        await _institutionRepository.DeleteAsync(id, userId, username, ipAddress);
        return Ok(ApiResponse<bool>.SuccessResponse(true, "Institution deleted successfully"));
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}
