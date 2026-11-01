namespace StudentInformationSystem.API.DTOs.Students
{
    public class StudentProfileDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}".Trim();
        public string? StudentNumber { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public int? AcademicYear { get; set; }
        public int? ProgrammeId { get; set; }
        public string? ProgrammeName { get; set; }
        public string? DepartmentName { get; set; }
        public DateTime CreatedAt { get; set; }
        public int EnrolledCoursesCount { get; set; }
        public int CompletedCoursesCount { get; set; }
    }
}
