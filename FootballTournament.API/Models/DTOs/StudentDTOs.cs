using System.ComponentModel.DataAnnotations;

namespace FootballTournament.API.Models.DTOs;

// ===================== STUDENT DTOs =====================

/// <summary>
/// Request to create a new student
/// </summary>
public class CreateStudentRequest
{
    [Required(ErrorMessage = "Student full name is required")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    public string StudentFullName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Date of birth is required")]
    public DateTime DateOfBirth { get; set; }
    
    [StringLength(10)]
    public string? Gender { get; set; } // Male, Female, Other
    
    // Academic Info
    [StringLength(50)]
    public string? ClassGrade { get; set; }
    
    [StringLength(20)]
    public string? Section { get; set; }
    
    [StringLength(50)]
    public string? RollNumber { get; set; }
    
    [StringLength(50)]
    public string? AdmissionNumber { get; set; }
    
    // Health & Safety
    public bool HasMedicalCondition { get; set; } = false;
    
    public string? MedicalConditionDetails { get; set; }
    
    [StringLength(5)]
    public string? BloodGroup { get; set; } // A+, A-, B+, B-, AB+, AB-, O+, O-
    
    [StringLength(100)]
    public string? EmergencyContactName { get; set; }
    
    [StringLength(20)]
    [Phone(ErrorMessage = "Invalid phone number format")]
    public string? EmergencyContactNumber { get; set; }
    
    // Parent/Guardian Details (Optional)
    [StringLength(100)]
    public string? ParentGuardianName { get; set; }
    
    [StringLength(50)]
    public string? Relationship { get; set; }
    
    [StringLength(20)]
    [Phone(ErrorMessage = "Invalid phone number format")]
    public string? ParentContactNumber { get; set; }
    
    [StringLength(20)]
    [Phone(ErrorMessage = "Invalid phone number format")]
    public string? ParentAlternateContact { get; set; }
    
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(100)]
    public string? ParentEmail { get; set; }
    
    // Documents
    public string? StudentPhotoUrl { get; set; }
    public string? SchoolIdCardUrl { get; set; }
    public string? ConsentFormUrl { get; set; }
    
    // Institution/Tournament
    public int? InstitutionId { get; set; }
    public int? TournamentId { get; set; }
}

/// <summary>
/// Request to update an existing student
/// </summary>
public class UpdateStudentRequest
{
    [Required]
    public int StudentId { get; set; }
    
    [Required(ErrorMessage = "Student full name is required")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    public string StudentFullName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Date of birth is required")]
    public DateTime DateOfBirth { get; set; }
    
    [StringLength(10)]
    public string? Gender { get; set; }
    
    // Academic Info
    [StringLength(50)]
    public string? ClassGrade { get; set; }
    
    [StringLength(20)]
    public string? Section { get; set; }
    
    [StringLength(50)]
    public string? RollNumber { get; set; }
    
    [StringLength(50)]
    public string? AdmissionNumber { get; set; }
    
    // Health & Safety
    public bool HasMedicalCondition { get; set; }
    
    public string? MedicalConditionDetails { get; set; }
    
    [StringLength(5)]
    public string? BloodGroup { get; set; }
    
    [StringLength(100)]
    public string? EmergencyContactName { get; set; }
    
    [StringLength(20)]
    [Phone]
    public string? EmergencyContactNumber { get; set; }
    
    // Parent/Guardian Details
    [StringLength(100)]
    public string? ParentGuardianName { get; set; }
    
    [StringLength(50)]
    public string? Relationship { get; set; }
    
    [StringLength(20)]
    [Phone]
    public string? ParentContactNumber { get; set; }
    
    [StringLength(20)]
    [Phone]
    public string? ParentAlternateContact { get; set; }
    
    [EmailAddress]
    [StringLength(100)]
    public string? ParentEmail { get; set; }
    
    // Documents
    public string? StudentPhotoUrl { get; set; }
    public string? SchoolIdCardUrl { get; set; }
    public string? ConsentFormUrl { get; set; }
    
    // Status
    public bool IsActive { get; set; } = true;
    
    [StringLength(20)]
    public string? RegistrationStatus { get; set; } // Pending, Approved, Rejected
}

/// <summary>
/// Request to update student registration status
/// </summary>
public class UpdateStudentStatusRequest
{
    [Required]
    public int StudentId { get; set; }
    
    [Required]
    [StringLength(20)]
    public string Status { get; set; } = string.Empty; // Pending, Approved, Rejected
}

/// <summary>
/// Response with student statistics
/// </summary>
public class StudentStatsResponse
{
    public int TotalStudents { get; set; }
    public int PendingApprovals { get; set; }
    public int ApprovedStudents { get; set; }
    public int RejectedStudents { get; set; }
    public int StudentsWithMedicalConditions { get; set; }
}

/// <summary>
/// Search filter for students
/// </summary>
public class StudentSearchFilter
{
    public string? SearchTerm { get; set; }
    public int? InstitutionId { get; set; }
    public int? TournamentId { get; set; }
    public string? ClassGrade { get; set; }
    public string? Section { get; set; }
    public string? RegistrationStatus { get; set; }
    public bool? HasMedicalCondition { get; set; }
}
