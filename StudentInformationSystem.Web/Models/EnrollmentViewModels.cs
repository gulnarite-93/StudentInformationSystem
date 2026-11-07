namespace StudentInformationSystem.Web.Models
{
    public class EnrollmentViewModel
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentEmail { get; set; } = string.Empty;
        public string? StudentNumber { get; set; }
        public string ProgrammeName { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public int Credits { get; set; }
        public int? TeacherId { get; set; }
        public string? TeacherName { get; set; }
        public string? TeacherTitle { get; set; }
        public int TermId { get; set; }
        public string TermName { get; set; } = string.Empty;
        public bool IsTermActive { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public string Status { get; set; } = "Enrolled";
    }

    public class StudentCourseRegistrationItemViewModel
    {
        public int CourseId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Credits { get; set; }
        public string? Description { get; set; }
        public int ProgrammeId { get; set; }
        public string ProgrammeName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public int TermId { get; set; }
        public string TermName { get; set; } = string.Empty;
        public bool IsTermActive { get; set; }
        public int? TeacherId { get; set; }
        public string? TeacherName { get; set; }
        public string? TeacherTitle { get; set; }
        public int EnrolledStudentsCount { get; set; }
        public bool IsAlreadyEnrolled { get; set; }
        public int? EnrollmentId { get; set; }
        public string? EnrollmentStatus { get; set; }
    }

    public class StudentCourseRegistrationPageViewModel
    {
        public List<EnrollmentViewModel> MyEnrollments { get; set; } = new();
        public List<StudentCourseRegistrationItemViewModel> AvailableCourses { get; set; } = new();
        public int TotalEnrolledCredits => MyEnrollments.Where(e => e.Status == "Enrolled").Sum(e => e.Credits);
        public int TotalEnrolledCount => MyEnrollments.Count(e => e.Status == "Enrolled");
        public string ActiveTermName => AvailableCourses.FirstOrDefault()?.TermName 
            ?? MyEnrollments.FirstOrDefault()?.TermName 
            ?? "Cari Semestr";
    }

    public class EnrolledStudentViewModel
    {
        public int EnrollmentId { get; set; }
        public int StudentId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? StudentNumber { get; set; }
        public string? PhoneNumber { get; set; }
        public string ProgrammeName { get; set; } = string.Empty;
        public int? AcademicYear { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public string Status { get; set; } = "Enrolled";
    }

    public class CourseStudentsViewModel
    {
        public CourseViewModel Course { get; set; } = new();
        public List<EnrolledStudentViewModel> Students { get; set; } = new();
    }

    public class AdminStudentEnrollmentsViewModel
    {
        public StudentViewModel Student { get; set; } = new();
        public List<EnrollmentViewModel> Enrollments { get; set; } = new();
        public List<CourseViewModel> AvailableCourses { get; set; } = new();
        public int TotalEnrolledCredits => Enrollments.Where(e => e.Status == "Enrolled").Sum(e => e.Credits);
    }
}
