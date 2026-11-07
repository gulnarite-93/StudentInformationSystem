namespace StudentInformationSystem.API.DTOs.Enrollments
{
    public class EnrollmentDto
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
}
