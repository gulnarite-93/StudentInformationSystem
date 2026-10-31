namespace StudentInformationSystem.Web.Models
{
    public class ApiUserDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? StudentNumber { get; set; }
        public int? AcademicYear { get; set; }
        public int? ProgrammeId { get; set; }
        public string? ProgrammeName { get; set; }
        public int? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public string? Title { get; set; }
    }

    public class ApiLoginResponseDto
    {
        public bool Success { get; set; }
        public string? Token { get; set; }
        public string? Message { get; set; }
        public ApiUserDto? User { get; set; }
    }
}
