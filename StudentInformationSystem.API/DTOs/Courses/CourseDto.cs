namespace StudentInformationSystem.API.DTOs.Courses
{
    public class CourseDto
    {
        public int Id { get; set; }
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
    }
}
