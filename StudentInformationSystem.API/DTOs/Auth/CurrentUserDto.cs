namespace StudentInformationSystem.API.DTOs.Auth
{
    public class CurrentUserDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}".Trim();
        public string Role { get; set; } = string.Empty;
        public string? StudentNumber { get; set; }
        public int? AcademicYear { get; set; }
        public int? ProgrammeId { get; set; }
        public string? ProgrammeName { get; set; }
        public int? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public string? Title { get; set; }
    }
}
