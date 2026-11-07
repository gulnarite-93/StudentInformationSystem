namespace StudentInformationSystem.API.DTOs.Enrollments
{
    public class StudentCourseRegistrationDto
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

    public class EnrolledStudentDto
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
}
