using System.ComponentModel.DataAnnotations;

namespace FootballTournament.API.Models.DTOs;

// ===================== VENUE DTOs =====================
public class CreateVenueRequest
{
    [Required]
    [StringLength(100)]
    public string VenueName { get; set; } = null!;
    
    [Required]
    [StringLength(200)]
    public string Location { get; set; } = null!;
    
    [Range(0, 100000)]
    public int? Capacity { get; set; }
    
    public bool HasFloodlights { get; set; }
    
    [StringLength(50)]
    public string? FieldSize { get; set; } // 100m x 64m, 80m x 50m, etc.
    
    [StringLength(50)]
    public string? SurfaceType { get; set; } // Natural Grass, Artificial Turf, etc.
    
    [StringLength(500)]
    public string? Facilities { get; set; }
    
    public int? InstitutionId { get; set; }
}

public class UpdateVenueRequest
{
    [Required]
    public int VenueId { get; set; }
    
    [Required]
    [StringLength(100)]
    public string VenueName { get; set; } = null!;
    
    [Required]
    [StringLength(200)]
    public string Location { get; set; } = null!;
    
    [Range(0, 100000)]
    public int? Capacity { get; set; }
    
    public bool HasFloodlights { get; set; }
    
    [StringLength(50)]
    public string? FieldSize { get; set; }
    
    [StringLength(50)]
    public string? SurfaceType { get; set; }
    
    [StringLength(500)]
    public string? Facilities { get; set; }
    
    public int? InstitutionId { get; set; }
}

public class UpdateVenueStatusRequest
{
    [Required]
    public int VenueId { get; set; }
    
    [Required]
    public string Status { get; set; } = null!; // Available, InUse, Maintenance, Reserved
}

public class CreateVenueBookingRequest
{
    [Required]
    public int VenueId { get; set; }
    
    public int? MatchId { get; set; }
    
    [Required]
    public DateTime BookingDate { get; set; }
    
    [Required]
    public TimeSpan StartTime { get; set; }
    
    [Required]
    public TimeSpan EndTime { get; set; }
    
    [StringLength(50)]
    public string? Purpose { get; set; } // Match, Practice, Event
}

// ===================== INSTITUTION DTOs =====================
public class CreateInstitutionRequest
{
    [Required]
    [StringLength(200)]
    public string InstitutionName { get; set; } = null!;
    
    [Required]
    [StringLength(20)]
    public string InstitutionCode { get; set; } = null!;
    
    [StringLength(500)]
    public string? Address { get; set; }
    
    [StringLength(100)]
    public string? City { get; set; }
    
    [StringLength(100)]
    public string? Country { get; set; }
    
    [EmailAddress]
    public string? ContactEmail { get; set; }
    
    [Phone]
    public string? ContactPhone { get; set; }
    
    [Url]
    public string? Website { get; set; }
}

public class UpdateInstitutionRequest
{
    [Required]
    public int InstitutionId { get; set; }
    
    [Required]
    [StringLength(200)]
    public string InstitutionName { get; set; } = null!;
    
    [Required]
    [StringLength(20)]
    public string InstitutionCode { get; set; } = null!;
    
    [StringLength(500)]
    public string? Address { get; set; }
    
    [StringLength(100)]
    public string? City { get; set; }
    
    [StringLength(100)]
    public string? Country { get; set; }
    
    [EmailAddress]
    public string? ContactEmail { get; set; }
    
    [Phone]
    public string? ContactPhone { get; set; }
    
    [Url]
    public string? Website { get; set; }
}

// ===================== TOURNAMENT DTOs =====================
public class CreateTournamentRequest
{
    [Required]
    [StringLength(200)]
    public string TournamentName { get; set; } = null!;
    
    [StringLength(1000)]
    public string? Description { get; set; }
    
    [Required]
    public DateTime StartDate { get; set; }
    
    [Required]
    public DateTime EndDate { get; set; }
    
    [Required]
    public string TournamentType { get; set; } = null!; // League, Knockout, GroupKnockout
    
    [Range(2, 128)]
    public int MaxTeams { get; set; } = 16;
    
    public int? InstitutionId { get; set; }
    
    [StringLength(100)]
    public string? OrganizerName { get; set; }
    
    [StringLength(100)]
    public string? OrganizerContact { get; set; }
}

public class UpdateTournamentRequest
{
    [Required]
    public int TournamentId { get; set; }
    
    [Required]
    [StringLength(200)]
    public string TournamentName { get; set; } = null!;
    
    [StringLength(1000)]
    public string? Description { get; set; }
    
    [Required]
    public DateTime StartDate { get; set; }
    
    [Required]
    public DateTime EndDate { get; set; }
    
    [Required]
    public string TournamentType { get; set; } = null!;
    
    [Required]
    public string Status { get; set; } = null!; // Upcoming, Active, Completed, Cancelled
    
    [Range(2, 128)]
    public int MaxTeams { get; set; }
    
    public int? InstitutionId { get; set; }
    
    [StringLength(100)]
    public string? OrganizerName { get; set; }
    
    [StringLength(100)]
    public string? OrganizerContact { get; set; }
}
