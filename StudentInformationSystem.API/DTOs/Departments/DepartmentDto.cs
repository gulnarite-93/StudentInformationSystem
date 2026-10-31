namespace StudentInformationSystem.API.DTOs.Departments
{
    public class DepartmentDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int ProgrammeCount { get; set; }
        public int TeacherCount { get; set; }
    }
}
