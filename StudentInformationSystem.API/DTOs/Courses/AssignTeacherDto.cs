namespace StudentInformationSystem.API.DTOs.Courses
{
    public class AssignTeacherDto
    {
        public int? TeacherId { get; set; }
    }

    public class TeacherSimpleDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Title { get; set; }
        public string? DepartmentName { get; set; }
    }
}
