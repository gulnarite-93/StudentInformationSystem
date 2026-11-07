using System.ComponentModel.DataAnnotations;

namespace StudentInformationSystem.API.DTOs.Grades
{
    public class GradeDto
    {
        public int Id { get; set; }
        public int EnrollmentId { get; set; }
        public int CourseId { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public int Credits { get; set; }
        public string TermName { get; set; } = string.Empty;
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string? StudentNumber { get; set; }
        public decimal? MidtermScore { get; set; }
        public decimal? AssignmentScore { get; set; }
        public decimal? FinalScore { get; set; }
        public decimal TotalScore { get; set; }
        public string LetterGrade { get; set; } = "F";
        public decimal GradePoint { get; set; } = 0.0m;
        public string? Feedback { get; set; }
        public DateTime EvaluatedAt { get; set; }
        public bool IsPassed => TotalScore >= 51.00m;
    }

    public class GradeSaveDto
    {
        [Required(ErrorMessage = "Qeydiyyat (Enrollment) ID tələb olunur.")]
        public int EnrollmentId { get; set; }

        [Range(0, 100, ErrorMessage = "Kollokvium balı 0 ilə 100 arasında olmalıdır.")]
        public decimal? MidtermScore { get; set; }

        [Range(0, 100, ErrorMessage = "Tapşırıq balı 0 ilə 100 arasında olmalıdır.")]
        public decimal? AssignmentScore { get; set; }

        [Range(0, 100, ErrorMessage = "İmtahan balı 0 ilə 100 arasında olmalıdır.")]
        public decimal? FinalScore { get; set; }

        [Range(0, 100, ErrorMessage = "Yekun bal 0 ilə 100 arasında olmalıdır.")]
        public decimal? TotalScore { get; set; }

        public string? Feedback { get; set; }
    }

    public class CourseGradesDto
    {
        public int CourseId { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public int Credits { get; set; }
        public string TeacherName { get; set; } = string.Empty;
        public string TermName { get; set; } = string.Empty;
        public int EnrolledStudentsCount { get; set; }
        public int GradedStudentsCount { get; set; }
        public decimal AverageScore { get; set; }
        public List<CourseStudentGradeDto> Students { get; set; } = new();
    }

    public class CourseStudentGradeDto
    {
        public int EnrollmentId { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string? StudentNumber { get; set; }
        public string Email { get; set; } = string.Empty;
        public bool HasGrade { get; set; }
        public int? GradeId { get; set; }
        public decimal? MidtermScore { get; set; }
        public decimal? AssignmentScore { get; set; }
        public decimal? FinalScore { get; set; }
        public decimal? TotalScore { get; set; }
        public string? LetterGrade { get; set; }
        public decimal? GradePoint { get; set; }
        public string? Feedback { get; set; }
        public DateTime? EvaluatedAt { get; set; }
    }

    public class StudentTranscriptDto
    {
        public int StudentId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? StudentNumber { get; set; }
        public string Email { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string ProgrammeName { get; set; } = string.Empty;
        public int? AcademicYear { get; set; }
        public decimal Gpa { get; set; }
        public int TotalCreditsCompleted { get; set; }
        public int TotalCreditsAttempted { get; set; }
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
        public List<TranscriptTermDto> Terms { get; set; } = new();
    }

    public class TranscriptTermDto
    {
        public int TermId { get; set; }
        public string TermName { get; set; } = string.Empty;
        public decimal TermGpa { get; set; }
        public int TermCredits { get; set; }
        public List<TranscriptCourseItemDto> Courses { get; set; } = new();
    }

    public class TranscriptCourseItemDto
    {
        public int CourseId { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public int Credits { get; set; }
        public decimal TotalScore { get; set; }
        public string LetterGrade { get; set; } = string.Empty;
        public decimal GradePoint { get; set; }
        public string Status { get; set; } = string.Empty; // Məqbul / Kəsr
    }

    public class StudentDashboardDto
    {
        public int StudentId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? StudentNumber { get; set; }
        public string ProgrammeName { get; set; } = string.Empty;
        public int? AcademicYear { get; set; }
        public decimal CurrentGpa { get; set; }
        public int TotalCredits { get; set; }
        public int EnrolledCoursesCount { get; set; }
        public decimal AttendancePercentage { get; set; }
        public int TotalLessons { get; set; }
        public int AttendedLessons { get; set; }
        public List<DashboardAnnouncementDto> RecentAnnouncements { get; set; } = new();
        public List<DashboardCourseDto> CurrentCourses { get; set; } = new();
    }

    public class DashboardAnnouncementDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public string? CourseName { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class DashboardCourseDto
    {
        public int CourseId { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public int Credits { get; set; }
        public string TeacherName { get; set; } = string.Empty;
        public bool HasGrade { get; set; }
        public decimal? TotalScore { get; set; }
        public string? LetterGrade { get; set; }
        public decimal? GradePoint { get; set; }
    }

    public class TeacherGradeCourseSummaryDto
    {
        public int CourseId { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public int Credits { get; set; }
        public string TermName { get; set; } = string.Empty;
        public int TotalEnrolled { get; set; }
        public int GradedCount { get; set; }
    }
}
