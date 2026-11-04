using System.ComponentModel.DataAnnotations;

namespace StudentInformationSystem.Web.Models
{
    public class TeacherGradeCourseSummaryViewModel
    {
        public int CourseId { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public int Credits { get; set; }
        public string TermName { get; set; } = string.Empty;
        public int TotalEnrolled { get; set; }
        public int GradedCount { get; set; }
    }

    public class CourseGradesViewModel
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
        public List<CourseStudentGradeViewModel> Students { get; set; } = new();
    }

    public class CourseStudentGradeViewModel
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

    public class GradeSaveInputModel
    {
        [Required]
        public int EnrollmentId { get; set; }

        public int CourseId { get; set; }
        public string? StudentName { get; set; }

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

    public class StudentGradeItemViewModel
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

    public class TranscriptViewModel
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
        public List<TranscriptTermViewModel> Terms { get; set; } = new();
    }

    public class TranscriptTermViewModel
    {
        public int TermId { get; set; }
        public string TermName { get; set; } = string.Empty;
        public decimal TermGpa { get; set; }
        public int TermCredits { get; set; }
        public List<TranscriptCourseItemViewModel> Courses { get; set; } = new();
    }

    public class TranscriptCourseItemViewModel
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

    public class StudentDashboardViewModel
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
        public List<DashboardAnnouncementViewModel> RecentAnnouncements { get; set; } = new();
        public List<DashboardCourseViewModel> CurrentCourses { get; set; } = new();
    }

    public class DashboardAnnouncementViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public string? CourseName { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class DashboardCourseViewModel
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
}
