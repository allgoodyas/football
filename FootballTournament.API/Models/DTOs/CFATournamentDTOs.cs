using System.ComponentModel.DataAnnotations;

namespace FootballTournament.API.Models.DTOs;

// ===================== CFA TOURNAMENT 2026 - DTOs =====================

// ===================== TOURNAMENT CATEGORY DTOs =====================

/// <summary>
/// Update tournament to set category
/// </summary>
public class UpdateTournamentCategoryRequest
{
    [Required]
    public int TournamentId { get; set; }
    
    [Required]
    public string Category { get; set; } = null!; // SuperJunior, Junior, Senior
}

// ===================== GOALKEEPER STATS DTOs =====================

/// <summary>
/// Record goalkeeper statistics for a match (Golden Glove tracking)
/// </summary>
public class RecordGoalkeeperStatsRequest
{
    [Required]
    public int MatchId { get; set; }
    
    [Required]
    public int PlayerId { get; set; }
    
    [Required]
    public int TeamId { get; set; }
    
    [Required]
    [Range(0, 100)]
    public int Saves { get; set; }
    
    [Required]
    [Range(0, 50)]
    public int GoalsConceded { get; set; }
    
    [Range(0, 20)]
    public int PenaltiesSaved { get; set; }
    
    [Range(0, 20)]
    public int PenaltiesFaced { get; set; }
    
    [Range(0, 150)]
    public int MinutesPlayed { get; set; }
}

/// <summary>
/// Update existing goalkeeper stats
/// </summary>
public class UpdateGoalkeeperStatsRequest : RecordGoalkeeperStatsRequest
{
    [Required]
    public int GoalkeeperStatsId { get; set; }
}

// ===================== DEFENDER RATING DTOs =====================

/// <summary>
/// Record defender rating for a match (Best Defender tracking)
/// </summary>
public class RecordDefenderRatingRequest
{
    [Required]
    public int MatchId { get; set; }
    
    [Required]
    public int PlayerId { get; set; }
    
    [Required]
    public int TeamId { get; set; }
    
    /// <summary>
    /// 1 = Best defender (2 points), 2 = Second best (1 point)
    /// </summary>
    [Required]
    [Range(1, 2)]
    public int RatingPosition { get; set; }
    
    // Evaluation criteria (1-10 scale)
    [Range(1, 10)]
    public int TacklingScore { get; set; }
    
    [Range(1, 10)]
    public int InterceptionScore { get; set; }
    
    [Range(1, 10)]
    public int MarkingScore { get; set; }
    
    [Range(1, 10)]
    public int BlockingScore { get; set; }
    
    [StringLength(500)]
    public string? Notes { get; set; }
}

/// <summary>
/// Submit both top defenders for a match at once
/// </summary>
public class SubmitMatchDefenderRatingsRequest
{
    [Required]
    public int MatchId { get; set; }
    
    [Required]
    public DefenderRatingEntry FirstPlace { get; set; } = null!;
    
    [Required]
    public DefenderRatingEntry SecondPlace { get; set; } = null!;
}

public class DefenderRatingEntry
{
    [Required]
    public int PlayerId { get; set; }
    
    [Required]
    public int TeamId { get; set; }
    
    [Range(1, 10)]
    public int TacklingScore { get; set; }
    
    [Range(1, 10)]
    public int InterceptionScore { get; set; }
    
    [Range(1, 10)]
    public int MarkingScore { get; set; }
    
    [Range(1, 10)]
    public int BlockingScore { get; set; }
    
    [StringLength(500)]
    public string? Notes { get; set; }
}

// ===================== GOAL TYPE DTOs =====================

/// <summary>
/// Add a goal event with type specification
/// </summary>
public class AddGoalEventRequest
{
    [Required]
    public int MatchId { get; set; }
    
    [Required]
    public int TeamId { get; set; }
    
    [Required]
    public int PlayerId { get; set; }
    
    [Required]
    [Range(0, 150)]
    public int EventMinute { get; set; }
    
    /// <summary>
    /// Goal type: Regular, Penalty, OwnGoal, ShootoutPenalty
    /// </summary>
    [Required]
    public string GoalType { get; set; } = "Regular";
    
    public int? AssistPlayerId { get; set; }
    
    public bool IsExtraTime { get; set; }
    
    [StringLength(500)]
    public string? Description { get; set; }
}

/// <summary>
/// Record penalty shootout result with individual penalties
/// </summary>
public class RecordPenaltyShootoutRequest
{
    [Required]
    public int MatchId { get; set; }
    
    [Required]
    public List<PenaltyKickEntry> HomeTeamPenalties { get; set; } = new();
    
    [Required]
    public List<PenaltyKickEntry> AwayTeamPenalties { get; set; } = new();
}

public class PenaltyKickEntry
{
    [Required]
    public int PlayerId { get; set; }
    
    [Required]
    public int Order { get; set; } // 1-11+
    
    [Required]
    public bool Scored { get; set; }
    
    public bool IsSuddenDeath { get; set; }
}

// ===================== WALKOVER DTOs =====================

/// <summary>
/// Declare a match as walkover
/// </summary>
public class DeclareWalkoverRequest
{
    [Required]
    public int MatchId { get; set; }
    
    /// <summary>
    /// Team ID of the team that showed up (winner)
    /// </summary>
    [Required]
    public int WinnerTeamId { get; set; }
    
    /// <summary>
    /// Team ID of the absent team
    /// </summary>
    [Required]
    public int AbsentTeamId { get; set; }
    
    [StringLength(500)]
    public string? Reason { get; set; }
    
    /// <summary>
    /// How many minutes late (max 5 per CFA rules)
    /// </summary>
    [Range(0, 60)]
    public int DelayMinutes { get; set; } = 5;
}

// ===================== AWARD LEADERBOARD REQUEST DTOs =====================

/// <summary>
/// Request parameters for Golden Boot leaderboard
/// </summary>
public class GoldenBootRequest
{
    [Required]
    public int TournamentId { get; set; }
    
    public int Limit { get; set; } = 10;
    
    /// <summary>
    /// Filter by category (optional)
    /// </summary>
    public string? Category { get; set; }
}

/// <summary>
/// Request parameters for Golden Glove leaderboard
/// </summary>
public class GoldenGloveRequest
{
    [Required]
    public int TournamentId { get; set; }
    
    public int Limit { get; set; } = 10;
    
    /// <summary>
    /// If true, only show goalkeepers who reached the final
    /// </summary>
    public bool EligibleOnly { get; set; } = false;
}

/// <summary>
/// Request parameters for Best Defender leaderboard
/// </summary>
public class BestDefenderRequest
{
    [Required]
    public int TournamentId { get; set; }
    
    public int Limit { get; set; } = 10;
}

/// <summary>
/// Request parameters for Airline Ticket standings (Senior category only)
/// </summary>
public class AirlineTicketRequest
{
    [Required]
    public int TournamentId { get; set; }
    
    public int Limit { get; set; } = 10;
}

// ===================== MATCH STATUS UPDATE DTOs =====================

/// <summary>
/// Update match status (including walkover)
/// </summary>
public class UpdateMatchStatusRequest
{
    [Required]
    public int MatchId { get; set; }
    
    /// <summary>
    /// Status: Scheduled, Live, HalfTime, SecondHalf, ExtraTime, 
    /// PenaltyShootout, Completed, Postponed, Cancelled, Walkover
    /// </summary>
    [Required]
    public string Status { get; set; } = null!;
    
    /// <summary>
    /// Required if status is Walkover
    /// </summary>
    public int? WinnerTeamId { get; set; }
    
    public int? CurrentMinute { get; set; }
    
    [StringLength(500)]
    public string? Notes { get; set; }
}
