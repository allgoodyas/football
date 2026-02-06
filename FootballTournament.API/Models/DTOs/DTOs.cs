using System.ComponentModel.DataAnnotations;

namespace FootballTournament.API.Models.DTOs;

// ===================== AUTH DTOs =====================
public class LoginRequest
{
    [Required]
    public string UsernameOrEmail { get; set; } = null!;
    
    [Required]
    public string Password { get; set; } = null!;
}

public class RegisterRequest
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string Username { get; set; } = null!;
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;
    
    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = null!;
    
    [Required]
    [Compare("Password")]
    public string ConfirmPassword { get; set; } = null!;
    
    [Required]
    [StringLength(100)]
    public string FullName { get; set; } = null!;
    
    [Phone]
    public string? PhoneNumber { get; set; }
    
    // Note: Role is NOT accepted during registration for security.
    // All new users are created as Spectators. Admins can change roles via User Management.
}

public class RefreshTokenRequest
{
    [Required]
    public string Token { get; set; } = null!;
    
    [Required]
    public string RefreshToken { get; set; } = null!;
}

public class AuthResponse
{
    public int UserId { get; set; }
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? FullName { get; set; }
    public string? Role { get; set; }
    public string? Token { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime TokenExpiry { get; set; }
}

// ===================== TEAM DTOs =====================
public class CreateTeamRequest
{
    [Required]
    public int TournamentId { get; set; }
    
    [Required]
    [StringLength(100)]
    public string TeamName { get; set; } = null!;
    
    [Required]
    [StringLength(10)]
    public string TeamCode { get; set; } = null!;
    
    [StringLength(50)]
    public string? ClassName { get; set; }
    
    [StringLength(100)]
    public string? CaptainName { get; set; }
    
    [StringLength(100)]
    public string? CaptainContact { get; set; }
    
    [StringLength(100)]
    public string? CoachName { get; set; }
    
    [StringLength(20)]
    public string? TeamColor { get; set; }
    
    [StringLength(10)]
    public string? GroupName { get; set; }
    
    public int? InstitutionId { get; set; }
}

public class UpdateTeamRequest
{
    [Required]
    public int TeamId { get; set; }
    
    [Required]
    [StringLength(100)]
    public string TeamName { get; set; } = null!;
    
    [Required]
    [StringLength(10)]
    public string TeamCode { get; set; } = null!;
    
    [StringLength(50)]
    public string? ClassName { get; set; }
    
    [StringLength(100)]
    public string? CaptainName { get; set; }
    
    [StringLength(100)]
    public string? CaptainContact { get; set; }
    
    [StringLength(100)]
    public string? CoachName { get; set; }
    
    [StringLength(20)]
    public string? TeamColor { get; set; }
    
    [StringLength(10)]
    public string? GroupName { get; set; }
    
    public int? InstitutionId { get; set; }
}

// ===================== MATCH DTOs =====================
public class CreateMatchRequest
{
    [Required]
    public int TournamentId { get; set; }
    
    [Required]
    public DateTime MatchDate { get; set; }
    
    [Required]
    public string Round { get; set; } = null!;
    
    public int? HomeTeamId { get; set; }
    public int? AwayTeamId { get; set; }
    public int? VenueId { get; set; }
    public TimeSpan? MatchTime { get; set; }
    public string? GroupName { get; set; }
    public bool IsKnockout { get; set; }
    public string? RefereeName { get; set; }
    public string? Notes { get; set; }
}

public class UpdateMatchRequest
{
    [Required]
    public int MatchId { get; set; }
    
    [Required]
    public DateTime MatchDate { get; set; }
    
    [Required]
    public string Round { get; set; } = null!;
    
    public int? HomeTeamId { get; set; }
    public int? AwayTeamId { get; set; }
    public int? VenueId { get; set; }
    public TimeSpan? MatchTime { get; set; }
    public string? GroupName { get; set; }
    public bool IsKnockout { get; set; }
    public string? RefereeName { get; set; }
    public string? Notes { get; set; }
}

public class UpdateMatchScoreRequest
{
    [Required]
    public int MatchId { get; set; }
    
    [Required]
    [Range(0, 100)]
    public int HomeScore { get; set; }
    
    [Required]
    [Range(0, 100)]
    public int AwayScore { get; set; }
    
    [Required]
    public string Status { get; set; } = null!; // Live, Completed, HalfTime
    
    public int? CurrentMinute { get; set; }
    public int? HomeScoreHT { get; set; }
    public int? AwayScoreHT { get; set; }
    public int? HomePenalties { get; set; }
    public int? AwayPenalties { get; set; }
    public bool IsExtraTime { get; set; }
    public bool IsPenaltyShootout { get; set; }
}

public class AddMatchEventRequest
{
    [Required]
    public int MatchId { get; set; }
    
    [Required]
    public int TeamId { get; set; }
    
    [Required]
    public string EventType { get; set; } = null!; // Goal, YellowCard, RedCard, Substitution
    
    [Required]
    [Range(0, 150)]
    public int EventMinute { get; set; }
    
    public int? PlayerId { get; set; }
    public bool IsExtraTime { get; set; }
    public int? AssistPlayerId { get; set; }
    public int? SubstitutedPlayerId { get; set; }
    public string? Description { get; set; }
    
    /// <summary>
    /// Goal type: Regular, Penalty, OwnGoal, ShootoutPenalty
    /// ShootoutPenalty goals are NOT counted for Golden Boot
    /// </summary>
    public string? GoalType { get; set; } = "Regular";
}

// ===================== ANNOUNCEMENT DTOs =====================
public class CreateAnnouncementRequest
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = null!;
    
    [Required]
    public string Content { get; set; } = null!;
    
    public int? TournamentId { get; set; }
    public string? AnnouncementType { get; set; } // General, Urgent, Important, Info
    public string? TargetAudience { get; set; } // All, Teams, Parents, Referees, Staff
    public bool IsPinned { get; set; }
    public DateTime? ExpiryDate { get; set; }
}

// ===================== USER MANAGEMENT DTOs =====================
public class UserDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public string Role { get; set; } = null!;
    public bool IsActive { get; set; }
    public DateTime CreatedOn { get; set; }
}

public class UpdateUserRoleRequest
{
    [Required]
    public int UserId { get; set; }
    
    [Required]
    [RegularExpression("^(Admin|Official|Spectator)$", ErrorMessage = "Role must be Admin, Official, or Spectator")]
    public string Role { get; set; } = null!;
}

public class UpdateUserStatusRequest
{
    [Required]
    public int UserId { get; set; }
    
    [Required]
    public bool IsActive { get; set; }
}

public class UpdateAnnouncementRequest
{
    [Required]
    public int AnnouncementId { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = null!;
    
    [Required]
    public string Content { get; set; } = null!;
    
    public string? AnnouncementType { get; set; }
    public string? TargetAudience { get; set; }
    public bool IsPinned { get; set; }
    public DateTime? ExpiryDate { get; set; }
}
