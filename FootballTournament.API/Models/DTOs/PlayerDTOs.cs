using System.ComponentModel.DataAnnotations;

namespace FootballTournament.API.Models.DTOs;

// ===================== PLAYER DTOs =====================

/// <summary>
/// Request to create a new player (accepts both playerName or firstName/lastName)
/// </summary>
public class CreatePlayerRequest
{
    public int? TournamentId { get; set; }
    
    public int? TeamId { get; set; }
    
    // Option 1: Single playerName (UI format)
    [StringLength(100)]
    public string? PlayerName { get; set; }
    
    // Option 2: Separate first/last name (API format)
    [StringLength(50)]
    public string? FirstName { get; set; }
    
    [StringLength(50)]
    public string? LastName { get; set; }
    
    public DateTime? DateOfBirth { get; set; }
    
    [Range(1, 99)]
    public int? JerseyNumber { get; set; }
    
    public string? Position { get; set; } // GK, DF, MF, FW
    
    // UI uses "grade", API uses "className"
    [StringLength(50)]
    public string? ClassName { get; set; }
    
    [StringLength(50)]
    public string? Grade { get; set; }
    
    [EmailAddress]
    public string? ContactEmail { get; set; }
    
    [Phone]
    public string? ContactPhone { get; set; }
    
    [StringLength(100)]
    public string? GuardianName { get; set; }
    
    [Phone]
    public string? GuardianContact { get; set; }
    
    public int? InstitutionId { get; set; }
    
    public bool IsCaptain { get; set; }
    
    /// <summary>
    /// Get the full player name (handles both formats)
    /// </summary>
    public string GetFullName()
    {
        if (!string.IsNullOrWhiteSpace(PlayerName))
            return PlayerName;
        
        return $"{FirstName ?? ""} {LastName ?? ""}".Trim();
    }
    
    /// <summary>
    /// Get the class/grade value (handles both formats)
    /// </summary>
    public string? GetClassName()
    {
        return ClassName ?? Grade;
    }
}

/// <summary>
/// Request to update a player
/// </summary>
public class UpdatePlayerRequest
{
    [Required]
    public int PlayerId { get; set; }
    
    public int? TeamId { get; set; }
    
    // Option 1: Single playerName (UI format)
    [StringLength(100)]
    public string? PlayerName { get; set; }
    
    // Option 2: Separate first/last name (API format)
    [StringLength(50)]
    public string? FirstName { get; set; }
    
    [StringLength(50)]
    public string? LastName { get; set; }
    
    public DateTime? DateOfBirth { get; set; }
    
    [Range(1, 99)]
    public int? JerseyNumber { get; set; }
    
    public string? Position { get; set; }
    
    // UI uses "grade", API uses "className"
    [StringLength(50)]
    public string? ClassName { get; set; }
    
    [StringLength(50)]
    public string? Grade { get; set; }
    
    [EmailAddress]
    public string? ContactEmail { get; set; }
    
    [Phone]
    public string? ContactPhone { get; set; }
    
    [StringLength(100)]
    public string? GuardianName { get; set; }
    
    [Phone]
    public string? GuardianContact { get; set; }
    
    public int? InstitutionId { get; set; }
    
    public bool IsCaptain { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Get the full player name (handles both formats)
    /// </summary>
    public string GetFullName()
    {
        if (!string.IsNullOrWhiteSpace(PlayerName))
            return PlayerName;
        
        return $"{FirstName ?? ""} {LastName ?? ""}".Trim();
    }
    
    /// <summary>
    /// Get the class/grade value (handles both formats)
    /// </summary>
    public string? GetClassName()
    {
        return ClassName ?? Grade;
    }
}

/// <summary>
/// Request to transfer player to another team
/// </summary>
public class TransferPlayerRequest
{
    [Required]
    public int PlayerId { get; set; }
    
    // Support both "ToTeamId" and "NewTeamId"
    public int? ToTeamId { get; set; }
    public int? NewTeamId { get; set; }
    
    [StringLength(500)]
    public string? Reason { get; set; }
    
    public int? GetTargetTeamId() => ToTeamId ?? NewTeamId;
}

/// <summary>
/// Request to bulk assign players to a team
/// </summary>
public class BulkAssignPlayersRequest
{
    [Required]
    public int TeamId { get; set; }
    
    [Required]
    [MinLength(1)]
    public List<int> PlayerIds { get; set; } = new();
}

/// <summary>
/// Filter parameters for searching players
/// </summary>
public class PlayerFilterRequest
{
    public int? TournamentId { get; set; }
    public int? TeamId { get; set; }
    public int? InstitutionId { get; set; }
    public string? Position { get; set; }
    public bool? UnassignedOnly { get; set; }
    public string? SearchTerm { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// Request to update player statistics for a match
/// </summary>
public class UpdatePlayerStatsRequest
{
    [Required]
    public int PlayerId { get; set; }
    
    [Required]
    public int MatchId { get; set; }
    
    [Range(0, 20)]
    public int Goals { get; set; }
    
    [Range(0, 20)]
    public int Assists { get; set; }
    
    [Range(0, 2)]
    public int YellowCards { get; set; }
    
    [Range(0, 1)]
    public int RedCards { get; set; }
    
    [Range(0, 150)]
    public int MinutesPlayed { get; set; }
    
    public bool IsManOfTheMatch { get; set; }
}

/// <summary>
/// Request to set team captain
/// </summary>
public class SetCaptainRequest
{
    [Required]
    public int TeamId { get; set; }
    
    [Required]
    public int PlayerId { get; set; }
}

// ===================== PLAYER STATS DTOs =====================

/// <summary>
/// Generic request to record a player action (goal, assist, card, etc.)
/// </summary>
public class RecordPlayerActionRequest
{
    [Required]
    public int MatchId { get; set; }
    
    [Required]
    public int PlayerId { get; set; }
    
    [Required]
    public int TeamId { get; set; }
    
    /// <summary>
    /// Action type: goal, assist, yellowcard, redcard, manofthematch
    /// </summary>
    [Required]
    public string? ActionType { get; set; }
    
    public int? Minute { get; set; }
    
    public int? MinutesPlayed { get; set; }
    
    public string? Description { get; set; }
}

/// <summary>
/// Request to record a goal
/// </summary>
public class RecordGoalRequest
{
    [Required]
    public int MatchId { get; set; }
    
    [Required]
    public int PlayerId { get; set; }
    
    [Required]
    public int TeamId { get; set; }
    
    public int? AssistPlayerId { get; set; }
    
    public int? Minute { get; set; }
    
    public string? Description { get; set; }
}

/// <summary>
/// Request to record an assist
/// </summary>
public class RecordAssistRequest
{
    [Required]
    public int MatchId { get; set; }
    
    [Required]
    public int PlayerId { get; set; }
    
    [Required]
    public int TeamId { get; set; }
    
    public int? Minute { get; set; }
    
    public string? Description { get; set; }
}

/// <summary>
/// Request to record a card (yellow or red)
/// </summary>
public class RecordCardRequest
{
    [Required]
    public int MatchId { get; set; }
    
    [Required]
    public int PlayerId { get; set; }
    
    [Required]
    public int TeamId { get; set; }
    
    public int? Minute { get; set; }
    
    public string? Reason { get; set; }
}

/// <summary>
/// Request to set man of the match
/// </summary>
public class SetManOfTheMatchRequest
{
    [Required]
    public int MatchId { get; set; }
    
    [Required]
    public int PlayerId { get; set; }
    
    [Required]
    public int TeamId { get; set; }
}

/// <summary>
/// Request to record complete player match stats
/// </summary>
public class RecordMatchStatsRequest
{
    [Required]
    public int MatchId { get; set; }
    
    [Required]
    public int PlayerId { get; set; }
    
    [Required]
    public int TeamId { get; set; }
    
    [Range(0, 20)]
    public int Goals { get; set; }
    
    [Range(0, 20)]
    public int Assists { get; set; }
    
    [Range(0, 2)]
    public int YellowCards { get; set; }
    
    [Range(0, 1)]
    public int RedCards { get; set; }
    
    [Range(0, 150)]
    public int MinutesPlayed { get; set; }
    
    public bool IsManOfTheMatch { get; set; }
}
