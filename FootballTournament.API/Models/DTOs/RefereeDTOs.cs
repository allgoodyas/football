using System.ComponentModel.DataAnnotations;

namespace FootballTournament.API.Models.DTOs;

// ===================== REFEREE DTOs =====================
public class CreateRefereeRequest
{
    [Required]
    [StringLength(100)]
    public string FullName { get; set; } = null!;
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;
    
    [Phone]
    public string? PhoneNumber { get; set; }
    
    [StringLength(100)]
    public string? Role { get; set; } // PE Teacher, Sports Coach, etc.
    
    [Range(0, 50)]
    public int ExperienceYears { get; set; }
    
    [StringLength(100)]
    public string? Certification { get; set; }
}

public class UpdateRefereeRequest
{
    [Required]
    public int RefereeId { get; set; }
    
    [Required]
    [StringLength(100)]
    public string FullName { get; set; } = null!;
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;
    
    [Phone]
    public string? PhoneNumber { get; set; }
    
    [StringLength(100)]
    public string? Role { get; set; }
    
    [Range(0, 50)]
    public int ExperienceYears { get; set; }
    
    [StringLength(100)]
    public string? Certification { get; set; }
}

public class UpdateRefereeAvailabilityRequest
{
    [Required]
    public int RefereeId { get; set; }
    
    [Required]
    public DateTime Date { get; set; }
    
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
    
    [Required]
    public string Status { get; set; } = null!; // Available, Busy, Unavailable, OnLeave
    
    public string? Notes { get; set; }
}

public class AssignRefereeRequest
{
    [Required]
    public int MatchId { get; set; }
    
    [Required]
    public int RefereeId { get; set; }
    
    [Required]
    public string RefereeRole { get; set; } = null!; // MainReferee, AssistantReferee1, etc.
}

public class BulkAssignRefereesRequest
{
    [Required]
    public int MatchId { get; set; }
    
    [Required]
    [MinLength(1)]
    public List<RefereeAssignment> Assignments { get; set; } = new();
}

public class RefereeAssignment
{
    [Required]
    public int RefereeId { get; set; }
    
    [Required]
    public string RefereeRole { get; set; } = null!;
}

public class RefereeFilterRequest
{
    public string? Status { get; set; }
    public DateTime? AvailableDate { get; set; }
    public string? SearchTerm { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class RefereeScheduleRequest
{
    [Required]
    public int RefereeId { get; set; }
    
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
