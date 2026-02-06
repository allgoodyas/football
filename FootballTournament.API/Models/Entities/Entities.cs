namespace FootballTournament.API.Models.Entities;

// ===================== ENUMS =====================
public enum PlayerPosition
{
    GK,   // Goalkeeper
    DF,   // Defender (updated from DEF)
    MF,   // Midfielder (updated from MID)
    FW    // Forward (updated from FWD)
}

public enum RefereeRole
{
    MainReferee,
    AssistantReferee1,
    AssistantReferee2,
    FourthOfficial
}

public enum AvailabilityStatus
{
    Available,
    Busy,
    Unavailable,
    OnLeave
}

public enum VenueStatus
{
    Available,
    InUse,
    Maintenance,
    Reserved
}

// ===================== INSTITUTION =====================
public class Institution
{
    public int InstitutionId { get; set; }
    public string? InstitutionName { get; set; }
    public string? InstitutionCode { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? Website { get; set; }
    public string? LogoUrl { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedOn { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime? EditedOn { get; set; }
    public int? EditedBy { get; set; }
}

// ===================== USER =====================
public class User
{
    public int UserId { get; set; }
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? PasswordHash { get; set; }
    public string? PasswordSalt { get; set; }
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Role { get; set; } // Admin, Official, Spectator
    public int? InstitutionId { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastLogin { get; set; }
    public DateTime CreatedOn { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime? EditedOn { get; set; }
    public int? EditedBy { get; set; }
}

// ===================== TOURNAMENT =====================
public class Tournament
{
    public int TournamentId { get; set; }
    public string? TournamentName { get; set; }
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? TournamentType { get; set; } // League, Knockout, GroupKnockout
    public string? Status { get; set; } // Upcoming, Active, Completed, Cancelled
    public int MaxTeams { get; set; }
    public int? InstitutionId { get; set; }
    public string? InstitutionName { get; set; }
    public string? OrganizerName { get; set; }
    public string? OrganizerContact { get; set; }
    public DateTime CreatedOn { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime? EditedOn { get; set; }
    public int? EditedBy { get; set; }
}

// ===================== TEAM =====================
public class Team
{
    public int TeamId { get; set; }
    public int TournamentId { get; set; }
    public string? TeamName { get; set; }
    public string? TeamCode { get; set; }
    public string? ClassName { get; set; }
    public string? CaptainName { get; set; }
    public string? CaptainContact { get; set; }
    public string? CoachName { get; set; }
    public string? TeamColor { get; set; }
    public string? LogoUrl { get; set; }
    public string? GroupName { get; set; }
    public int? InstitutionId { get; set; }
    public string? InstitutionName { get; set; }
    public bool IsActive { get; set; }
    public bool IsEliminated { get; set; }
    public int MatchesPlayed { get; set; }
    public int Wins { get; set; }
    public int Draws { get; set; }
    public int Losses { get; set; }
    public int GoalsFor { get; set; }
    public int GoalsAgainst { get; set; }
    public int GoalDifference { get; set; }
    public int Points { get; set; }
    public DateTime CreatedOn { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime? EditedOn { get; set; }
    public int? EditedBy { get; set; }
}

public class TeamStanding
{
    public int TeamId { get; set; }
    public string? TeamName { get; set; }
    public string? TeamCode { get; set; }
    public string? GroupName { get; set; }
    public int MatchesPlayed { get; set; }
    public int Wins { get; set; }
    public int Draws { get; set; }
    public int Losses { get; set; }
    public int GoalsFor { get; set; }
    public int GoalsAgainst { get; set; }
    public int GoalDifference { get; set; }
    public int Points { get; set; }
    public long StandingPosition { get; set; }
}

// ===================== PLAYER =====================
public class Player
{
    public int PlayerId { get; set; }
    public int? TeamId { get; set; } // Nullable - player can be unassigned
    public int TournamentId { get; set; }
    public int? InstitutionId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? FullName { get; set; }
    
    // PlayerName - for UI compatibility (alias for FullName or computed)
    public string? PlayerName { get; set; }
    
    public DateTime? DateOfBirth { get; set; }
    public int? JerseyNumber { get; set; }
    public string? Position { get; set; } // GK, DF, MF, FW
    public string? ClassName { get; set; }
    
    // Grade - UI alias for ClassName
    public string? Grade => ClassName;
    
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? GuardianName { get; set; }
    public string? GuardianContact { get; set; }
    public string? PhotoUrl { get; set; }
    public bool IsCaptain { get; set; }
    public bool IsActive { get; set; }
    public string? TeamName { get; set; }
    public string? TeamCode { get; set; }
    public string? InstitutionName { get; set; }
    
    // Career statistics
    public int Goals { get; set; }
    public int GoalsScored => Goals; // UI alias
    public int Assists { get; set; }
    public int YellowCards { get; set; }
    public int RedCards { get; set; }
    public int MatchesPlayed { get; set; }
    
    public DateTime CreatedOn { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime? EditedOn { get; set; }
    public int? EditedBy { get; set; }
}

// ===================== PLAYER STATISTICS =====================
public class PlayerStatistics
{
    public int StatId { get; set; }
    public int PlayerId { get; set; }
    public int TournamentId { get; set; }
    public int Goals { get; set; }
    public int Assists { get; set; }
    public int YellowCards { get; set; }
    public int RedCards { get; set; }
    public int MatchesPlayed { get; set; }
    public int MinutesPlayed { get; set; }
    public int CleanSheets { get; set; }
    public int Saves { get; set; }
    public int ManOfTheMatch { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime? EditedOn { get; set; }
}

public class PlayerMatchStats
{
    public int MatchStatsId { get; set; }
    public int MatchId { get; set; }
    public int PlayerId { get; set; }
    public int TeamId { get; set; }
    public int Goals { get; set; }
    public int Assists { get; set; }
    public int YellowCards { get; set; }
    public int RedCards { get; set; }
    public int MinutesPlayed { get; set; }
    public bool IsManOfTheMatch { get; set; }
    public string? PlayerName { get; set; }
    public string? TeamName { get; set; }
}

public class PlayerTransfer
{
    public int TransferId { get; set; }
    public int PlayerId { get; set; }
    public int? FromTeamId { get; set; }
    public int? ToTeamId { get; set; }
    public DateTime TransferDate { get; set; }
    public string? Reason { get; set; }
    public string? PlayerName { get; set; }
    public string? FromTeamName { get; set; }
    public string? ToTeamName { get; set; }
    public DateTime CreatedOn { get; set; }
    public int? CreatedBy { get; set; }
}

// ===================== MATCH =====================
public class Match
{
    public int MatchId { get; set; }
    public int TournamentId { get; set; }
    public int? HomeTeamId { get; set; }
    public int? AwayTeamId { get; set; }
    public int? VenueId { get; set; }
    public DateTime MatchDate { get; set; }
    public TimeSpan? MatchTime { get; set; }
    public int? MatchNumber { get; set; }
    public string? Round { get; set; }
    public string? GroupName { get; set; }
    public int? HomeScore { get; set; }
    public int? AwayScore { get; set; }
    public int? HomeScoreHT { get; set; }
    public int? AwayScoreHT { get; set; }
    public int? HomePenalties { get; set; }
    public int? AwayPenalties { get; set; }
    public string? Status { get; set; } // Scheduled, Live, InProgress, Completed, Postponed, Cancelled
    public int? WinnerTeamId { get; set; }
    public bool IsKnockout { get; set; }
    public int? CurrentMinute { get; set; }
    public bool IsExtraTime { get; set; }
    public bool IsPenaltyShootout { get; set; }
    public int? RefereeUserId { get; set; }
    public string? RefereeName { get; set; }
    public string? Notes { get; set; }
    public string? HomeTeamName { get; set; }
    public string? HomeTeamCode { get; set; }
    public string? AwayTeamName { get; set; }
    public string? AwayTeamCode { get; set; }
    public string? VenueName { get; set; }
    public DateTime CreatedOn { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime? EditedOn { get; set; }
    public int? EditedBy { get; set; }
}

public class MatchEvent
{
    public int EventId { get; set; }
    public int MatchId { get; set; }
    public int TeamId { get; set; }
    public int? PlayerId { get; set; }
    public string? EventType { get; set; } // Goal, YellowCard, RedCard, Substitution, Injury, PenaltyMissed
    public int EventMinute { get; set; }
    public bool IsExtraTime { get; set; }
    public int? AssistPlayerId { get; set; }
    public int? SubstitutedPlayerId { get; set; }
    public string? Description { get; set; }
    public string? TeamName { get; set; }
    public string? TeamCode { get; set; }
    public string? PlayerName { get; set; }
    public int? JerseyNumber { get; set; }
    public string? AssistPlayerName { get; set; }
    public DateTime CreatedOn { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime? EditedOn { get; set; }
    public int? EditedBy { get; set; }
}

// ===================== REFEREE =====================
public class Referee
{
    public int RefereeId { get; set; }
    public int UserId { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Role { get; set; } // PE Teacher, Sports Coach, etc.
    public int ExperienceYears { get; set; }
    public string? Certification { get; set; }
    public string? Status { get; set; } // Available, Busy, Unavailable, OnLeave
    public string? PhotoUrl { get; set; }
    public int MatchesOfficiated { get; set; }
    public string? NextAssignment { get; set; }
    public DateTime? NextAssignmentDate { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedOn { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime? EditedOn { get; set; }
    public int? EditedBy { get; set; }
}

public class MatchReferee
{
    public int MatchRefereeId { get; set; }
    public int MatchId { get; set; }
    public int RefereeId { get; set; }
    public string? RefereeRole { get; set; } // MainReferee, AssistantReferee1, etc.
    public string? RefereeName { get; set; }
    public string? MatchInfo { get; set; }
    public DateTime? MatchDate { get; set; }
}

public class RefereeAvailability
{
    public int AvailabilityId { get; set; }
    public int RefereeId { get; set; }
    public DateTime Date { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
    public string? Status { get; set; }
    public string? Notes { get; set; }
}

// ===================== VENUE =====================
public class Venue
{
    public int VenueId { get; set; }
    public string? VenueName { get; set; }
    public string? Location { get; set; }
    public int? Capacity { get; set; }
    public bool HasFloodlights { get; set; }
    public string? FieldSize { get; set; }
    public string? SurfaceType { get; set; }
    public string? Facilities { get; set; }
    public string? Status { get; set; } // Available, InUse, Maintenance, Reserved
    public string? ImageUrl { get; set; }
    public int? InstitutionId { get; set; }
    public string? InstitutionName { get; set; }
    public int TotalMatchesHosted { get; set; }
    public decimal Rating { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedOn { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime? EditedOn { get; set; }
    public int? EditedBy { get; set; }
}

public class VenueBooking
{
    public int BookingId { get; set; }
    public int VenueId { get; set; }
    public int? MatchId { get; set; }
    public DateTime BookingDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string? Purpose { get; set; } // Match, Practice, Event
    public string? Status { get; set; } // Confirmed, Pending, Cancelled
    public string? BookedBy { get; set; }
    public string? VenueName { get; set; }
    public string? MatchInfo { get; set; }
}

public class VenueStats
{
    public long TotalVenues { get; set; }
    public long AvailableNow { get; set; }
    public long TotalMatchesHosted { get; set; }
}

public class VenueSchedule
{
    public int ScheduleId { get; set; }
    public int VenueId { get; set; }
    public string? VenueName { get; set; }
    public int? MatchId { get; set; }
    public string? EventType { get; set; } // Match, Practice, Maintenance, Event
    public string? Title { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string? Status { get; set; } // Scheduled, In Progress, Completed, Cancelled
}

public class FacilityStatus
{
    public string? Name { get; set; }
    public string? Icon { get; set; }
    public string? Status { get; set; } // OK, Needs Attention, Out of Service
    public string? Details { get; set; }
}

// ===================== ANNOUNCEMENT =====================
public class Announcement
{
    public int AnnouncementId { get; set; }
    public int? TournamentId { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
    public string? AnnouncementType { get; set; } // General, Urgent, Important, Info
    public string? TargetAudience { get; set; } // All, Teams, Parents, Referees, Staff
    public bool IsPublished { get; set; }
    public bool IsPinned { get; set; }
    public DateTime PublishDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public int ViewCount { get; set; }
    public string? CreatedByName { get; set; }
    public DateTime CreatedOn { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime? EditedOn { get; set; }
    public int? EditedBy { get; set; }
}

// ===================== STATISTICS VIEW MODELS =====================
public class DashboardStats
{
    public long TotalTeams { get; set; }
    public long TotalMatches { get; set; }
    public long CompletedMatches { get; set; }
    public long ScheduledMatches { get; set; }
    public long LiveMatches { get; set; }
    public long TotalGoals { get; set; }
    public long TotalPlayers { get; set; }
}

public class TopScorer
{
    public int PlayerId { get; set; }
    public string? PlayerName { get; set; }
    public int? TeamId { get; set; }
    public string? TeamName { get; set; }
    public string? TeamCode { get; set; }
    public string? Position { get; set; }
    public int? JerseyNumber { get; set; }
    public int Goals { get; set; }
    public int Assists { get; set; }
    public int MatchesPlayed { get; set; }
    public long Rank { get; set; }
}

public class TopAssister
{
    public int PlayerId { get; set; }
    public string? PlayerName { get; set; }
    public int? TeamId { get; set; }
    public string? TeamName { get; set; }
    public string? TeamCode { get; set; }
    public string? Position { get; set; }
    public int? JerseyNumber { get; set; }
    public int Assists { get; set; }
    public int Goals { get; set; }
    public int MatchesPlayed { get; set; }
    public long Rank { get; set; }
}

public class ManOfTheMatch
{
    public int MatchId { get; set; }
    public int PlayerId { get; set; }
    public string? PlayerName { get; set; }
    public int? JerseyNumber { get; set; }
    public int? TeamId { get; set; }
    public string? TeamName { get; set; }
    public string? TeamCode { get; set; }
    public DateTime MatchDate { get; set; }
    public string? Opponent { get; set; }
    public long Goals { get; set; }
    public long Assists { get; set; }
}

public class TournamentSummary
{
    public int TotalMatches { get; set; }
    public int CompletedMatches { get; set; }
    public int TotalGoals { get; set; }
    public decimal AverageGoalsPerMatch { get; set; }
    public int TotalYellowCards { get; set; }
    public int TotalRedCards { get; set; }
    public int HomeWins { get; set; }
    public int AwayWins { get; set; }
    public int Draws { get; set; }
    public int TotalPlayers { get; set; }
    public int TotalTeams { get; set; }
}

public class GoalsByTime
{
    public string? TimePeriod { get; set; } // 0-15, 16-30, 31-45, 45+
    public int Goals { get; set; }
    public decimal Percentage { get; set; }
}

public class GoalsByMatchDay
{
    public int MatchDay { get; set; }
    public DateTime Date { get; set; }
    public int Goals { get; set; }
    public int Matches { get; set; }
}

public class TeamStatistics
{
    public int TeamId { get; set; }
    public string? TeamName { get; set; }
    public string? TeamCode { get; set; }
    public int MatchesPlayed { get; set; }
    public int Wins { get; set; }
    public int Draws { get; set; }
    public int Losses { get; set; }
    public int GoalsFor { get; set; }
    public int GoalsAgainst { get; set; }
    public int GoalDifference { get; set; }
    public int Points { get; set; }
    public int CleanSheets { get; set; }
    public int YellowCards { get; set; }
    public int RedCards { get; set; }
    public decimal WinPercentage { get; set; }
    public decimal GoalsPerMatch { get; set; }
}

public class CardedPlayer
{
    public int PlayerId { get; set; }
    public string? PlayerName { get; set; }
    public int? TeamId { get; set; }
    public string? TeamName { get; set; }
    public int YellowCards { get; set; }
    public int RedCards { get; set; }
    public int TotalCards { get; set; }
}

// PlayerStats - Used for top scorers, top assists, and general player statistics
public class PlayerStats
{
    public int PlayerId { get; set; }
    public int? TeamId { get; set; }
    public string? PlayerName { get; set; }
    public int? JerseyNumber { get; set; }
    public string? Position { get; set; }
    public string? TeamName { get; set; }
    public string? TeamCode { get; set; }
    public int Goals { get; set; }
    public int Assists { get; set; }
    public int YellowCards { get; set; }
    public int RedCards { get; set; }
    public int MatchesPlayed { get; set; }
    public long Rank { get; set; }
}

// ===================== CFA TOURNAMENT 2026 - PRIORITY 1 FEATURES =====================

// ===================== ENUMS FOR CFA TOURNAMENT =====================

/// <summary>
/// Tournament category for CFA Tournament 2026
/// </summary>
public enum TournamentCategory
{
    SuperJunior,  // Only highest points from each group advances to final
    Junior,
    Senior        // Eligible for Airline Ticket prizes
}

/// <summary>
/// Goal type to differentiate for Golden Boot calculation
/// Post-match penalty shootout goals are NOT counted for Golden Boot
/// </summary>
public enum GoalType
{
    Regular,           // Normal goal during play
    Penalty,           // Penalty kick during match (counts for Golden Boot)
    OwnGoal,           // Own goal
    ShootoutPenalty    // Post-match penalty shootout (NOT counted for Golden Boot)
}

/// <summary>
/// Match status including Walkover
/// </summary>
public enum MatchStatusType
{
    Scheduled,
    Live,
    HalfTime,
    SecondHalf,
    ExtraTime,
    PenaltyShootout,
    Completed,
    Postponed,
    Cancelled,
    Walkover          // Team not present - auto 3-0 win for present team
}

// ===================== GOALKEEPER MATCH STATS (FOR GOLDEN GLOVE) =====================

/// <summary>
/// Goalkeeper statistics per match for Golden Glove calculation
/// Formula: Saves - Goals Conceded (must reach Final to be eligible)
/// </summary>
public class GoalkeeperMatchStats
{
    public int GoalkeeperStatsId { get; set; }
    public int MatchId { get; set; }
    public int PlayerId { get; set; }
    public int TeamId { get; set; }
    public int TournamentId { get; set; }
    
    // Stats tracked
    public int Saves { get; set; }              // Each save = +1 point
    public int GoalsConceded { get; set; }      // Each goal conceded = -1 point
    public int PenaltiesSaved { get; set; }     // Includes shootout penalties (counted for Golden Glove)
    public int PenaltiesFaced { get; set; }     // Total penalties faced
    public int MinutesPlayed { get; set; }
    public bool IsCleanSheet { get; set; }
    
    // Calculated
    public int GoldenGlovePoints => Saves - GoalsConceded;
    
    // Joined fields
    public string? PlayerName { get; set; }
    public string? TeamName { get; set; }
    public string? TeamCode { get; set; }
    public DateTime? MatchDate { get; set; }
    public string? Opponent { get; set; }
    
    // Audit
    public DateTime CreatedOn { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime? EditedOn { get; set; }
    public int? EditedBy { get; set; }
}

/// <summary>
/// Golden Glove leaderboard entry
/// </summary>
public class GoldenGloveStanding
{
    public int PlayerId { get; set; }
    public string? PlayerName { get; set; }
    public int TeamId { get; set; }
    public string? TeamName { get; set; }
    public string? TeamCode { get; set; }
    public int? JerseyNumber { get; set; }
    
    public int TotalSaves { get; set; }
    public int TotalGoalsConceded { get; set; }
    public int TotalPenaltiesSaved { get; set; }
    public int MatchesPlayed { get; set; }
    public int CleanSheets { get; set; }
    public int GoldenGlovePoints { get; set; }   // Total: Saves - GoalsConceded
    public bool ReachedFinal { get; set; }       // Must reach final to be eligible
    
    // Final match stats (for tie-breaker)
    public int FinalMatchSaves { get; set; }
    public int FinalMatchConceded { get; set; }
    public int FinalMatchPoints { get; set; }
    
    public long Rank { get; set; }
}

// ===================== DEFENDER RATING (FOR BEST DEFENDER) =====================

/// <summary>
/// Defender rating per match for Best Defender calculation
/// Top 2 defenders rated per match: 1st = 2 points, 2nd = 1 point
/// </summary>
public class DefenderMatchRating
{
    public int RatingId { get; set; }
    public int MatchId { get; set; }
    public int PlayerId { get; set; }
    public int TeamId { get; set; }
    public int TournamentId { get; set; }
    
    // Rating (1 = Best, 2 = Second Best)
    public int RatingPosition { get; set; }     // 1 or 2
    public int PointsAwarded { get; set; }      // 2 for 1st, 1 for 2nd
    
    // Evaluation criteria (for reference/display)
    public int TacklingScore { get; set; }      // 1-10
    public int InterceptionScore { get; set; } // 1-10
    public int MarkingScore { get; set; }       // 1-10
    public int BlockingScore { get; set; }      // 1-10 (includes clearances)
    public string? Notes { get; set; }
    
    // Joined fields
    public string? PlayerName { get; set; }
    public int? JerseyNumber { get; set; }
    public string? TeamName { get; set; }
    public string? TeamCode { get; set; }
    public DateTime? MatchDate { get; set; }
    public string? Opponent { get; set; }
    
    // Audit (rated by)
    public string? RatedByName { get; set; }    // Coach or scoresheet official
    public DateTime CreatedOn { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime? EditedOn { get; set; }
    public int? EditedBy { get; set; }
}

/// <summary>
/// Best Defender leaderboard entry
/// </summary>
public class BestDefenderStanding
{
    public int PlayerId { get; set; }
    public string? PlayerName { get; set; }
    public int TeamId { get; set; }
    public string? TeamName { get; set; }
    public string? TeamCode { get; set; }
    public int? JerseyNumber { get; set; }
    public string? Position { get; set; }
    
    public int TotalPoints { get; set; }        // Sum of all match points
    public int MatchesRated { get; set; }       // Number of matches where player was top 2
    public int FirstPlaceCount { get; set; }    // Times ranked #1 (2 points each)
    public int SecondPlaceCount { get; set; }   // Times ranked #2 (1 point each)
    public decimal AverageScore { get; set; }   // Average of all criteria scores
    
    public long Rank { get; set; }
}

// ===================== GOLDEN BOOT ENHANCED =====================

/// <summary>
/// Golden Boot leaderboard with CFA rules
/// Excludes post-match shootout penalties
/// Tie-breaker: Goals -> Assists -> Final match performance
/// </summary>
public class GoldenBootStanding
{
    public int PlayerId { get; set; }
    public string? PlayerName { get; set; }
    public int TeamId { get; set; }
    public string? TeamName { get; set; }
    public string? TeamCode { get; set; }
    public int? JerseyNumber { get; set; }
    public string? Position { get; set; }
    
    // Goal breakdown
    public int TotalGoals { get; set; }                // All goals (for display)
    public int EligibleGoals { get; set; }             // Excluding shootout penalties (for ranking)
    public int RegularGoals { get; set; }              // Goals during normal play
    public int PenaltyGoals { get; set; }              // In-match penalties (counted)
    public int ShootoutGoals { get; set; }             // Post-match shootout (NOT counted)
    
    public int Assists { get; set; }                   // First tie-breaker
    public int MatchesPlayed { get; set; }
    
    // Final match performance (second tie-breaker)
    public int FinalMatchGoals { get; set; }
    public int FinalMatchAssists { get; set; }
    public bool PlayedInFinal { get; set; }
    
    public long Rank { get; set; }
}

// ===================== MATCH EVENT EXTENDED =====================

/// <summary>
/// Extended match event with goal type for Golden Boot tracking
/// </summary>
public class MatchEventExtended : MatchEvent
{
    public string? GoalType { get; set; }              // Regular, Penalty, OwnGoal, ShootoutPenalty
    public bool IsShootoutPenalty { get; set; }        // Quick check for shootout goals
}

// ===================== WALKOVER RESULT =====================

/// <summary>
/// Walkover match result details
/// </summary>
public class WalkoverResult
{
    public int MatchId { get; set; }
    public int WinnerTeamId { get; set; }
    public int? AbsentTeamId { get; set; }
    public string? Reason { get; set; }
    public int DelayMinutes { get; set; }              // How late the team was (max 5 min rule)
    public DateTime DeclaredAt { get; set; }
    public string? DeclaredBy { get; set; }            // Referee name
    
    // Default walkover score is 3-0
    public int WinnerScore { get; set; } = 3;
    public int LoserScore { get; set; } = 0;
    
    // Joined fields
    public string? WinnerTeamName { get; set; }
    public string? AbsentTeamName { get; set; }
}

// ===================== AWARD SUMMARY (FOR AIRLINE TICKET) =====================

/// <summary>
/// Combined award points for Senior category (Airline Ticket calculation)
/// </summary>
public class AwardSummary
{
    public int PlayerId { get; set; }
    public string? PlayerName { get; set; }
    public int TeamId { get; set; }
    public string? TeamName { get; set; }
    public string? TeamCode { get; set; }
    public int? JerseyNumber { get; set; }
    public string? Position { get; set; }
    public string? Category { get; set; }              // Must be Senior for Airline Ticket
    
    // Golden Boot contribution (Goals * 2 + Assists * 1)
    public int GoldenBootGoals { get; set; }
    public int GoldenBootAssists { get; set; }
    public int GoldenBootPoints { get; set; }
    
    // Golden Glove contribution (aggregated points)
    public int GoldenGlovePoints { get; set; }
    
    // Best Defender contribution (aggregated points)
    public int BestDefenderPoints { get; set; }
    
    // Total for Airline Ticket ranking
    public int TotalAirlineTicketPoints { get; set; }
    
    public long Rank { get; set; }
}
